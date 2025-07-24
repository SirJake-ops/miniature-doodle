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
        try
        {
            context.Messages.Add(message);
            await context.SaveChangesAsync();

            await sender.SendAsync(nameof(Subscription.MessageAdded), message);
            return message;
        }
        catch (Exception e)
        {
            throw new MessageNotSentException("Messavge could not be sent: " + e.Message);
        }
    }
}
