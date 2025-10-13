using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Ulfsoft.Web.Mvc;

namespace Wiktly.Web.Areas.Identity;

[Authorize]
public class AuthController : BaseApiController
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthController(ILogger<AuthController> logger,
                          UserManager<IdentityUser> userManager,
                          SignInManager<IdentityUser> signInManager)
        : base(logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var userIdentity = User.Identity;

        if (userIdentity is null)
        {
            return NotFound("User not found");
        }

        var claims = User.Claims
                         .DistinctBy(c => c.Type)
                         .ToDictionary(c => c.Type, c => c.Value);

        var authDetails = new
        {
            userIdentity.Name,
            userIdentity.IsAuthenticated,
            Claims = claims
        };

        return Ok(authDetails);
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("token")]
    [Produces("application/json")]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            if (request.Username is null)
            {
                return BadRequest("Username is required");
            }

            if (request.Password is null)
            {
                return BadRequest("Password is required");
            }

            var user = await _userManager.FindByEmailAsync(request.Username);
            if (user == null)
            {
                return Forbid("The username or password is invalid.");
            }

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!passwordCheck.Succeeded)
            {
                return Forbid("The username or password is invalid.");
            }

            var identity = await CreateClaimsIdentity(user);

            var result = SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return result;
        }

        if (request.IsRefreshTokenGrantType())
        {
            if (request.RefreshToken is null)
            {
                return BadRequest("Refresh token is required");
            }

            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var userP = User;
            if (result.Principal is null)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var user = await _userManager.GetUserAsync(result.Principal);
            if (user is null)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var identity = await CreateClaimsIdentity(user);

            var refreshedResult = SignIn(new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return refreshedResult;
        }

        return BadRequest("*.*");
    }

    /// <summary>
    /// Create a ClaimsIdentity for the user
    /// </summary>
    private async Task<ClaimsIdentity> CreateClaimsIdentity(IdentityUser user)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaims(OpenIddictConstants.Claims.Role,
                    [..await _userManager.GetRolesAsync(user)]);

        identity.SetScopes(Scopes);

        return identity;
    }
    
    private static string[] Scopes =>
    [
        OpenIddictConstants.Scopes.OpenId,
        OpenIddictConstants.Scopes.Email,
        OpenIddictConstants.Scopes.Profile,
        OpenIddictConstants.Scopes.Roles,
        OpenIddictConstants.Scopes.OfflineAccess
    ];
}
