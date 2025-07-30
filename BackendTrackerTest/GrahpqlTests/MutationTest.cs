using BackendTracker.Entities.Message;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation.Exceptions;
using BackendTrackerPresentation.Graphql;
using BackendTrackerPresentation.Graphql.GraphqlTypes;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BackendTrackerTest.GrahpqlTests;

public class MutationTestFixture : IDisposable
{
    public readonly IDbContextFactory<ApplicationContext> ContextFactory;

    public MutationTestFixture()
    {
        var services = new ServiceCollection();

        services.AddDbContextFactory<ApplicationContext>(options =>
        {
            options.UseInMemoryDatabase("GraphqlTestDb" + Guid.NewGuid());
        });

        ContextFactory = services.BuildServiceProvider().GetRequiredService<IDbContextFactory<ApplicationContext>>();

        SeedTestData();
    }

    public void Dispose()
    {
        using var context = ContextFactory.CreateDbContext();
        context.Database.EnsureDeleted();
    }

    private void SeedTestData()
    {
        using var context = ContextFactory.CreateDbContext();

        context.ApplicationUsers.Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "TEST_USER",
            Email = "testEmail@yahoo.com",
            Password = "testPassword",
            Messages = new List<Message>(),
            Conversations = new List<Conversation>()
        });

        context.SaveChanges();
    }
}

public class MutationTest : IClassFixture<MutationTestFixture>
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    public MutationTest(MutationTestFixture fixture)
    {
        _contextFactory = fixture.ContextFactory;
    }

    [Fact]
    public async Task CreateUser_ShouldCreateNewUser()
    {
        var mutation = new Mutation(_contextFactory);

        var newUser = new ApplicationUserInput
        {
            UserName = "NewUser",
            Email = "newUserEmail@test.com",
            Password = "123Password",
            Role = "User"
        };

        var createdUser = await mutation.CreateUser(newUser);

        Assert.NotNull(createdUser);
        Assert.Equal(newUser.UserName, createdUser.UserName);
    }

    [Fact]
    public async Task CreateMessage_ShouldShowTheUserReceivedAndUserSentMessage()
    {
        var query = new Query(_contextFactory);
        var user = await query.GetUser(new UserSearchInput
        {
            UserName = "TEST_USER",
            UserEmail = "testEmail@yahoo.com",
        });

        Assert.NotNull(user);

        var newMessage = new Message
        {
            Id = Guid.NewGuid(),
            Content = "Test message content",
            SenderId = user!.Id,
            ReceiverId = Guid.NewGuid(),
            SentTime = DateTime.UtcNow,
            IsRead = false
        };

        Assert.NotNull(newMessage);

        var mutation = new Mutation(_contextFactory);
        var sender = new Mock<ITopicEventSender>();

        var createdMessage = await mutation.AddMessage(newMessage, sender.Object);

        Assert.NotNull(createdMessage);
        Assert.Equal(newMessage.Content, createdMessage.Content);
        Assert.Equal(newMessage.SenderId, createdMessage.SenderId);
        Assert.Equal(user.Id, createdMessage.SenderId);
    }

    [Fact]
    public async Task CreateConversation_ShouldCreateNewConversation()
    {
        var query = new Query(_contextFactory);
        var user = await query.GetUser(new UserSearchInput
        {
            UserName = "TEST_USER",
            UserEmail = "testEmail@yahoo.com",
        });

        Assert.NotNull(user);

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            InitialSenderId = user!.Id,
            InitialReceiverId = Guid.NewGuid(),
            LastMessageTime = DateTime.UtcNow
        };

        var mutation = new Mutation(_contextFactory);
        var sender = new Mock<ITopicEventSender>();

        var createdConversation = await mutation.AddConversation(conversation, sender.Object);

        Assert.NotNull(createdConversation);
        Assert.Equal(conversation.InitialSenderId, createdConversation.InitialSenderId);
        Assert.Equal(conversation.InitialReceiverId, createdConversation.InitialReceiverId);
        Assert.Equal(conversation.LastMessageTime, createdConversation.LastMessageTime);
        Assert.Equal(conversation.Id, createdConversation.Id);
    }
}