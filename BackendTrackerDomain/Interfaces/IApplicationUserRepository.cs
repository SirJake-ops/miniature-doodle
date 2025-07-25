using BackendTrackerDomain.Entity.ApplicationUser;

namespace BackendTrackerDomain.Interfaces;

public interface IApplicationUserRepository
{
    Task<IEnumerable<ApplicationUser>> GetUsersAsync();
    Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
}