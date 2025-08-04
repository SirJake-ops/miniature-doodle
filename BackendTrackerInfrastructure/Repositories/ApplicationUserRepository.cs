using BackendTrackerApplication.Dtos;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Interfaces;
using BackendTrackerInfrastructure.Exceptions;
using BackendTrackerInfrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BackendTrackerInfrastructure.Repositories;

public class ApplicationUserRepository(IDbContextFactory<ApplicationContext> _context) : IApplicationUserRepository
{
    public Task<ApplicationUser> GetUserById(Guid userId)
    {
        throw new NotImplementedException();
    }

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

    public async Task<ApplicationUser?> CreateUserAsync(ApplicationUser user)
    {
        await using var context = await _context.CreateDbContextAsync();
        var createdUser =  await context.ApplicationUsers.AddAsync(user);

        return createdUser.Entity;

    }

    public async Task UpdateUserAsync(Guid userId, ApplicationUser user)
    {
        await using var context = await _context.CreateDbContextAsync();
        var existingUser = await context.ApplicationUsers.FindAsync(userId) ?? throw new ApplicationUserExceptions("User does not exist");
        
        var updatedUser = new ApplicationUser 
        {
            UserName = user.UserName,
            Email = user.Email,
            Password = user.Password,
            Role = user.Role,
            Messages = user.Messages,
            Conversations = user.Conversations,
            SubmittedTickets = user.SubmittedTickets,
            AssignedTickets = user.AssignedTickets
        };
        
        context.Entry(existingUser).CurrentValues.SetValues(updatedUser);
        await context.SaveChangesAsync();
    }

    public Task DeleteUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}