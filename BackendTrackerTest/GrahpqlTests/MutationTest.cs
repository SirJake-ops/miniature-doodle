using BackendTracker.Entities.Message;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerInfrastructure.Persistence.Context;
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
    public async void CreateUser_ShouldCreateNewUser()
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
    public async void CreateMessage_ShouldCreateNewMessage()
    {
        var mutation = new Mutation(_contextFactory);
        var sender = new Mock<ITopicEventSender>();
        
        var newMessage = new Message
        {
            Id = Guid.NewGuid(),
            Content = "Test message content",
            SenderId = Guid.NewGuid(),
        };
        
        var createMessage = await mutation.AddMessage(newMessage, sender.Object);
        
        Assert.NotNull(createMessage);
        Assert.Equal(newMessage.SenderId, createMessage.SenderId);
        Assert.Equal(newMessage.ReceiverId, createMessage.ReceiverId);
    }
}
