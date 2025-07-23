using BackendTracker.Entities.Message;
using BackendTracker.GraphQueries.GraphqlTypes;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerInfrastructure.Graphql;
using BackendTrackerInfrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
}