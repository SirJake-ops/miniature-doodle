using BackendTrackerDomain.Entity.ApplicationUser;

namespace BackendTrackerApplication.Dtos;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public ApplicationUserDto User { get; set; } = null!;
}