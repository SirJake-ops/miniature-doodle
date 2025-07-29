using BackendTracker.Entities.Message;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation.Graphql;
using BackendTrackerPresentation.Graphql.GraphqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTrackerTest.GrahpqlTests;

public class QueryTestFixture : IDisposable
{
    public readonly IDbContextFactory<ApplicationContext> ContextFactory;

    public QueryTestFixture()
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
            Email = "testEmail@test.com",
            Password = "testPassword",
            Messages = new List<Message>(),
            Conversations = new List<Conversation>()
        });

        context.Conversations.Add(new Conversation
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            InitialSenderId = Guid.NewGuid(),
            InitialReceiverId = Guid.NewGuid(),
            LastMessageTime = DateTime.Parse("2023-10-01T00:00:00Z")
        });

        context.Messages.Add(new Message
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            SenderId = Guid.NewGuid(),
            ReceiverId = Guid.NewGuid(),
            Content = "Test message content",
            SentTime = DateTime.UtcNow
        });


        context.SaveChanges();
    }
}

public class QueryTest : IClassFixture<QueryTestFixture>
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    public QueryTest(QueryTestFixture fixture)
    {
        _contextFactory = fixture.ContextFactory;
    }

    [Fact]
    public async Task GetUser_ShouldReturnSingleUser()
    {
        var query = new Query(_contextFactory);
        var userSearchInput = new UserSearchInput
        {
            UserName = "TEST_USER",
            UserEmail = "testEmail@test.com"
        };

        var user = await query.GetUser(userSearchInput);

        Assert.NotNull(user);
        Assert.Equal("TEST_USER", user.UserName);
        Assert.Equal("testEmail@test.com", user.Email);
    }

    [Fact]
    public async Task GetConversation_ShouldReturnSingleConversation()
    {
        var query = new Query(_contextFactory);
        var conversationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var dateTime = DateTime.Parse("2023-10-01T00:00:00Z");

        var conversations = await query.GetConversations(conversationId);
        conversations = conversations.ToList();

        Assert.NotNull(conversations);
        Assert.Single(conversations);
        Assert.Equal(dateTime, conversations.ElementAt(0).LastMessageTime);
    }

    [Fact]
    public async Task GetMessages_ShouldReturnMessagesForUser()
    {
        var query = new Query(_contextFactory);
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var messages = await query.GetMessagesUser(userId);
        messages = messages.ToList();

        Assert.NotNull(messages);
        Assert.Single(messages);
        Assert.Equal("Test message content", messages.ElementAt(0).Content);
    }
}