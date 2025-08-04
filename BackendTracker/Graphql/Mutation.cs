using AutoMapper;
using BackendTracker.Entities.Message;
using BackendTrackerApplication.Dtos;
using BackendTrackerApplication.Services.Messaging;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation.Exceptions;
using BackendTrackerPresentation.Graphql.GraphqlTypes;
using BackendTrackerPresentation.Graphql.Subscriptions;
using HotChocolate.Subscriptions;
using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackendTrackerPresentation.Graphql;

public class Mutation(
    IDbContextFactory<ApplicationContext> dbContextFactory,
    IMessageService messageService,
    IMapper mapper)
{
    public async Task<ApplicationUserDto?> CreateUser(ApplicationUserInput user)
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
            var createdUser = mapper.Map<ApplicationUserDto>(applicationUser);
            return createdUser;
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

    public async Task<DeleteMessageDto> DeleteMessage(Message message, [Service] ITopicEventSender sender)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var trackedMessage = await context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id)
                             ?? throw new MessageNotFoundException("Message not found");

        var user = await context.ApplicationUsers.FirstOrDefaultAsync(m => m.Id == message.SenderId)
                   ?? throw new UserNotFoundGraphql("User not found for deleting a message");

        user.Messages.Remove(trackedMessage);
        context.Messages.Remove(trackedMessage);
        await context.SaveChangesAsync();

        var sendMessage = mapper.Map<MessageDto>(trackedMessage);
        await messageService.SendAsync(sendMessage);
        await sender.SendAsync(nameof(Subscription.MessageDeleted), trackedMessage);
        return mapper.Map<DeleteMessageDto>(trackedMessage);
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