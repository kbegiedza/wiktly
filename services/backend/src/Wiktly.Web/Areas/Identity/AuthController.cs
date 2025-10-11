using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ulfsoft.Web.Mvc;

namespace Wiktly.Web.Areas.Identity;

[Authorize]
public class AuthController : BaseApiController
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthController(ILogger<AuthController> logger, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        : base(logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginPayload payload)
    {
        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var signIn = await _signInManager.CheckPasswordSignInAsync(user, payload.Password, false);

        if (!signIn.Succeeded)
        {
            return Unauthorized("Invalid email or password.");
        }

        if (signIn.IsLockedOut)
        {
            return Forbid("This account is locked out.");
        }

        if (signIn.RequiresTwoFactor)
        {
            return BadRequest("Not supported yet.");
        }

        return Ok(new LoginResponse($"dummy-token-for: {payload.Email}"));
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Name = User.Identity?.Name ?? "unknown"
        });
    }
}

public record LoginPayload(string Email, string Password);

public record LoginResponse(string Token);