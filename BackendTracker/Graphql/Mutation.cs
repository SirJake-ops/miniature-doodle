using BackendTracker.Entities.Message;
using BackendTracker.Ticket.Enums;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation.Exceptions;
using BackendTrackerPresentation.Graphql.GraphqlTypes;
using BackendTrackerPresentation.Graphql.Subscriptions;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackendTrackerPresentation.Graphql;

public class Mutation(IDbContextFactory<ApplicationContext> dbContextFactory)
{
    public async Task<ApplicationUser?> CreateUser(ApplicationUserInput user)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        try
        {
            var hasher = new PasswordHasher<ApplicationUser>();

            var applicationUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                RefreshToken = "",
                RefreshTokenExpiryTime = DateTime.Now.AddHours(24)
                    .ToUniversalTime(),
                LastLoginTime = DateTime.Now.ToUniversalTime(),
                IsOnline = false,
                Messages = new List<Message>(),
                Conversations = new List<Conversation>()
            };

            applicationUser.Password = hasher.HashPassword(applicationUser,
                user.Password ?? throw new InvalidOperationException("Password cannot be null"));

            context.ApplicationUsers.Add(applicationUser);
            await context.SaveChangesAsync();
            return applicationUser;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Message> AddMessage(Message message, [Service] ITopicEventSender sender)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var user = await context.ApplicationUsers.FirstOrDefaultAsync(m => m.Id == message.SenderId)
                   ?? throw new UserNotFoundGraphql("User not found for adding creating a message");
        user.Messages.Add(message);
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        await sender.SendAsync(nameof(Subscription.MessageAdded), message);
        return message;
    }

    public async Task<Conversation> AddConversation(Conversation conversation, [Service] ITopicEventSender sender)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        var user = await context.ApplicationUsers.FirstOrDefaultAsync(m => m.Id == conversation.InitialSenderId)
                   ?? throw new UserNotFoundGraphql("User not found for adding creating a conversation");
        
        user.Conversations.Add(conversation);
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();
        
        await sender.SendAsync(nameof(Subscription.ConversationAdded), conversation);
        return conversation;
    }
}