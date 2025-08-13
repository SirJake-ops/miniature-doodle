using AutoMapper;
using BackendTracker.Entities.Message;
using BackendTrackerApplication.Mapping.MappingProfiles;
using BackendTrackerApplication.Services.Messaging;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation.Graphql;
using BackendTrackerPresentation.Graphql.GraphqlTypes;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BackendTrackerTest.GrahpqlTests;

public class MutationTestFixture : IDisposable
{
    public readonly IDbContextFactory<ApplicationContext> ContextFactory;
    public readonly IMapper Mapper;
    public readonly IMessageService MessageService;
    public readonly ServiceProvider ServiceProvider;

    public MutationTestFixture()
    {
        var services = new ServiceCollection();

        services.AddDbContextFactory<ApplicationContext>(options =>
        {
            options.UseInMemoryDatabase("GraphqlTestDb" + Guid.NewGuid());
        });

        services.AddLogging();

        var configurations = new MapperConfiguration(config =>
        {
            config.AddProfile<ApplicationUserMappingProfile>();
            config.AddProfile<TicketMappingProfile>();
            config.AddProfile<MessageMappingProfile>();
        }, new LoggerFactory());
        
        configurations.AssertConfigurationIsValid();

        services.AddSignalR();
        services.AddSingleton<IMapper>(new Mapper(configurations));
        services.AddScoped<IMessageService, MessageService>();
        ServiceProvider = services.BuildServiceProvider();

        ContextFactory = ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationContext>>();
        Mapper = ServiceProvider.GetRequiredService<IMapper>();
        MessageService = ServiceProvider.GetRequiredService<IMessageService>();

        SeedTestData();
    }

    public void Dispose()
    {
        using var context = ContextFactory.CreateDbContext();
        context.Database.EnsureDeleted();
        ServiceProvider?.Dispose();
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

public class MutationTest(MutationTestFixture fixture) : IClassFixture<MutationTestFixture>
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory = fixture.ContextFactory;
    private readonly IMapper _mapper = fixture.Mapper;
    private readonly IMessageService _messageService = fixture.MessageService;

    [Fact]
    public async Task CreateUser_ShouldCreateNewUser()
    {
        var mutation = new Mutation(_contextFactory,_messageService, _mapper);

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

        var mutation = new Mutation(_contextFactory, _messageService, _mapper);
        var sender = new Mock<ITopicEventSender>();

        var createdMessage = await mutation.AddMessage(newMessage, sender.Object);

        Assert.NotNull(createdMessage);
        Assert.Equal(newMessage.Content, createdMessage.Content);
        Assert.Equal(newMessage.SenderId, createdMessage.SenderId);
        Assert.Equal(user.Id, createdMessage.SenderId);
    }

    [Fact]
    public async Task DeleteMessage_ShouldDeleteMessage()
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

        var mutation = new Mutation(_contextFactory, _messageService, _mapper);
        var sender = new Mock<ITopicEventSender>();
        
        var createdMessage = await mutation.AddMessage(newMessage, sender.Object);
        Assert.NotNull(createdMessage);

        
        var deletedMessage = await mutation.DeleteMessage(createdMessage, sender.Object);
        Assert.NotNull(deletedMessage);

        var messages = query.GetMessagesUser(user.Id);
        Assert.NotNull(messages);
        
        Assert.Empty(user.Messages);
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

        var mutation = new Mutation(_contextFactory, _messageService, _mapper);
        var sender = new Mock<ITopicEventSender>();

        var createdConversation = await mutation.AddConversation(conversation, sender.Object);

        Assert.NotNull(createdConversation);
        Assert.Equal(conversation.InitialSenderId, createdConversation.InitialSenderId);
        Assert.Equal(conversation.InitialReceiverId, createdConversation.InitialReceiverId);
        Assert.Equal(conversation.LastMessageTime, createdConversation.LastMessageTime);
        Assert.Equal(conversation.Id, createdConversation.Id);
    }
}