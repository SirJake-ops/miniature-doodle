using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerInfrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendTrackerApplication.Dtos;
using LoginRequest = BackendTracker.Auth.LoginRequest;

namespace BackendTrackerPresentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IDbContextFactory<ApplicationContext> applicationContext, IConfiguration configuration)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        using var context = await applicationContext.CreateDbContextAsync();

        var user = await context.ApplicationUsers.FirstOrDefaultAsync(user =>
            user.UserName == loginRequest.UserName)!;
        if (user == null) return Unauthorized("Invalid username.");

        var hasher = new PasswordHasher<ApplicationUser>();
        var result = hasher.VerifyHashedPassword(user, user.Password!, loginRequest.Password);
        if (result == PasswordVerificationResult.Failed) return Unauthorized("Invalid password.");

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = new ApplicationUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
            } 
        });
    }


    private string GenerateJwtToken(ApplicationUser user)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}