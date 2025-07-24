using BackendTracker.Entities.Message;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation.Graphql.GraphqlTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BackendTrackerPresentation.Graphql;

public class Query(IDbContextFactory<ApplicationContext> dbContextFactory)
{
    private readonly ApplicationContext _context = dbContextFactory.CreateDbContext();

    public string Hello() => "Hello From Graph";

    [Authorize]
    public async Task<IEnumerable<Conversation>> GetConversations(Guid userId) =>
        await _context.Conversations
            .Where(m => m.Id == userId)
            .AsNoTracking()
            .ToListAsync();

    public async Task<ApplicationUser?> GetUser(UserSearchInput searchInput) =>
        await _context.ApplicationUsers
            .Where(m => m.UserName == searchInput.UserName
                        && m.Email == searchInput.UserEmail)
            .FirstOrDefaultAsync();

    [Authorize]
    public async Task<IEnumerable<Message>> GetMessagesUser(Guid userId) =>
        await _context.Messages
            .Where(m => m.Id == userId)
            .AsNoTracking()
            .ToListAsync();
}