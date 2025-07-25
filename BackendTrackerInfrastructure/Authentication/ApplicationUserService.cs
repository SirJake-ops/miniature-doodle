using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using BackendTracker.Auth;
using BackendTrackerApplication.Dtos;
using BackendTrackerApplication.Interfaces;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerInfrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BackendTrackerApplication.Services;

public class ApplicationUserService(IDbContextFactory<ApplicationContext> _context, IConfiguration configuration) : IApplicationUserService
{
    public async Task<AuthResponse> Login(LoginRequest loginRequest)
    {
        await using var context = await _context.CreateDbContextAsync();

        var user = await context.ApplicationUsers.FirstOrDefaultAsync(user =>
            user.UserName == loginRequest.UserName) ?? throw new AuthenticationException("User not found.");

        var hasher = new PasswordHasher<ApplicationUser>();
        var result = hasher.VerifyHashedPassword(user, user.Password!, loginRequest.Password);
        if (result == PasswordVerificationResult.Failed) throw new AuthenticationException("Invalid password."); 

        var token = GenerateJwtToken(user);
        return new AuthResponse
        {
            Token = token,
            User = new ApplicationUserDto
            {
               UserName = user.UserName,
                Email = user.Email,
            } 
        };
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