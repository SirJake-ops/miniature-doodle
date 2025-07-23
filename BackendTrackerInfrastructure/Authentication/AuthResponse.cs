using BackendTrackerDomain.Entity.ApplicationUser;

namespace BackendTrackerInfrastructure.Authentication;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}