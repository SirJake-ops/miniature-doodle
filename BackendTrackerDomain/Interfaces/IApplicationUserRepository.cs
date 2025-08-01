using BackendTrackerDomain.Entity.ApplicationUser;

namespace BackendTrackerDomain.Interfaces;

public interface IApplicationUserRepository
{
    Task<IEnumerable<ApplicationUser>> GetUsersAsync();
    Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
    Task<ApplicationUser?> CreateUserAsync(ApplicationUser user);
    Task UpdateUserAsync(Guid userId, ApplicationUser user);
    Task DeleteUserAsync(Guid userId);
}