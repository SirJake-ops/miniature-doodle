using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerInfrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendTrackerApplication.Dtos;
using MailKit.Security;
using LoginRequest = BackendTracker.Auth.LoginRequest;

namespace BackendTrackerPresentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IDbContextFactory<ApplicationContext> applicationContext, IConfiguration configuration)
    : ControllerBase
{
    [Authorize]
    [HttpGet("user")]
    public async Task<IActionResult> CurrentUser()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var id)) return Unauthorized();

        using var context = await applicationContext.CreateDbContextAsync();
        var user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return Unauthorized();

        return Ok(new ApplicationUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        using var context = await applicationContext.CreateDbContextAsync();

        var ident = (loginRequest.UserName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(ident)) return Unauthorized("Username or email is required.");

        var user = await context.ApplicationUsers.FirstOrDefaultAsync(user =>
            user.UserName == ident || user.Email == ident);
        if (user == null) return Unauthorized("Invalid username/email.");

        var hasher = new PasswordHasher<ApplicationUser>();
        var result = hasher.VerifyHashedPassword(user, user.Password!, loginRequest.Password ?? throw new AuthenticationException("Password is required."));
        if (result == PasswordVerificationResult.Failed) return Unauthorized("Invalid password.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role ?? "User"),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new ApplicationUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

}
