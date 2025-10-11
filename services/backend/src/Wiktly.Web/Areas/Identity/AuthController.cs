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
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthController(ILogger<AuthController> logger,
                          SignInManager<IdentityUser> signInManager,
                          UserManager<IdentityUser> userManager,
                          IOpenIddictTokenManager tokenManager)
        : base(logger)
    {
        _userManager = userManager;
        _tokenManager = tokenManager;
        _signInManager = signInManager;
    }

    // [AllowAnonymous]
    // [HttpPost("login")]
    // public async Task<IActionResult> Login(LoginPayload payload)
    // {
    //     var user = await _userManager.FindByEmailAsync(payload.Username);
    //
    //     if (user is null)
    //     {
    //         return Unauthorized("Invalid email or password.");
    //     }
    //
    //     var signIn = await _signInManager.CheckPasswordSignInAsync(user, payload.Password, false);
    //
    //     if (!signIn.Succeeded)
    //     {
    //         return Unauthorized("Invalid email or password.");
    //     }
    //
    //     if (signIn.IsLockedOut)
    //     {
    //         return Forbid("This account is locked out.");
    //     }
    //
    //     if (signIn.RequiresTwoFactor)
    //     {
    //         return BadRequest("Not supported yet.");
    //     }
    //
    //     return Ok(new LoginResponse($"dummy-token-for: {payload.Email}"));
    // }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Name = User.Identity?.Name ?? "unknown"
        });
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
                .SetClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

        identity.SetScopes(new[]
        {
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
        });
        
        return identity;
    }
}