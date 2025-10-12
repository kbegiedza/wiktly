using System.Collections.Immutable;
using System.Security.Claims;
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
    [HttpPost("login")]
    [Produces("application/json")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token([FromForm] OpenIddictRequest payload)
    {
        if (payload.Username is null)
        {
            throw new ArgumentNullException(nameof(payload.Username));
        }

        var user = await _userManager.FindByEmailAsync(payload.Username);
        if (user == null)
        {
            return Forbid("The username or password is invalid.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, payload.Password, false);
        if (!result.Succeeded)
        {
            return Forbid("The username or password is invalid.");
        }

        var identity = await CreateClaimsIdentity(user);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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
                    (await _userManager.GetRolesAsync(user)).ToImmutableArray());

        identity.SetScopes(new[]
        {
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
        });

        return identity;
    }
}
