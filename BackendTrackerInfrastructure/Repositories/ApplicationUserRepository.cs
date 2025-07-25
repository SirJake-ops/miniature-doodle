using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Interfaces;
using BackendTrackerInfrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BackendTrackerInfrastructure.Repositories;

public class ApplicationUserRepository(IDbContextFactory<ApplicationContext> _context) : IApplicationUserRepository
{
    public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
    {
        await using var context = await _context.CreateDbContextAsync();
        return await context.ApplicationUsers.ToListAsync();
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
    {
        await using var context = await _context.CreateDbContextAsync();
        return await context.ApplicationUsers
            .Include(u => u.SubmittedTickets)
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}