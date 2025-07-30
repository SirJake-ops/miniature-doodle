using BackendTracker.Entities.Message;
using BackendTrackerApplication.Services.Messaging;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Entity.Ticket.FileUpload;
using BackendTrackerDomain.Interfaces;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerInfrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Environment = BackendTracker.Ticket.Enums.Environment;

namespace BackendTrackerTest.IntegrationTests.IntegrationTestSetup;

public class SignalRFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
   public Mock<IHubContext<MessageHub>> MockHubContext { get; set; }
   public Mock<IClientProxy> MockClientProxy { get; set; }
   protected override void ConfigureWebHost(IWebHostBuilder builder)
   {
      var MockClientProxy = new Mock<IClientProxy>();
      MockHubContext = new Mock<IHubContext<MessageHub>>();

      MockHubContext.Setup(x => x.Clients.User(It.IsAny<string>())).Returns(MockClientProxy.Object);
      MockHubContext.Setup(a => a.Clients.Group(It.IsAny<string>())).Returns(MockClientProxy.Object);
      
      builder.ConfigureServices(services =>
      {
         var descriptors = services
            .Where(d => d.ServiceType.FullName != null &&
                        d.ServiceType.FullName.Contains("ApplicationContext"))
            .ToList();

         foreach (var descriptor in descriptors)
            services.Remove(descriptor);
         
         var dbConnectionDescriptor = services
            .Where(d => d.ServiceType.FullName != null &&
                        d.ServiceType.FullName.Contains("DbConnection")).ToList();
         
         foreach (var descriptor in dbConnectionDescriptor)
            services.Remove(descriptor);

         services.RemoveAll<ITicketRepository>();
         services.AddScoped<ITicketRepository, TicketRepository>();
         services.AddScoped<IMessageService, MessageService>();
         services.AddSignalR(options =>
         {
            options.EnableDetailedErrors = true;
         });
         
         services.Replace(ServiceDescriptor.Scoped<IHubContext<MessageHub>>(_ => MockHubContext.Object));

         services.AddPooledDbContextFactory<ApplicationContext>(options =>
            options.UseInMemoryDatabase("InMemoryDbForTesting"));


         var serviceProvider = services.BuildServiceProvider();
         using var scope = serviceProvider.CreateScope();
         var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationContext>>();
         using var db = factory.CreateDbContext();
         db.Database.EnsureCreated();
         SeedData(db);
         
      });
   }
   
   private void SeedData(ApplicationContext context)
   {
      if (context.ApplicationUsers.Any()) return;

      var hasher = new PasswordHasher<ApplicationUser>();

      var user = new ApplicationUser
      {
         Id = Guid.NewGuid(),
         UserName = "testuser",
         Email = "testEmail@test.com",
         Role = "User",
         Messages = new List<Message>(),
         Conversations = new List<Conversation>()
      };

      user.Password = hasher.HashPassword(user, "123abc");

      var ticket = new BackendTrackerDomain.Entity.Ticket.Ticket
      {
         TicketId = Guid.NewGuid(),
         Title = "Test Ticket",
         Description = "This is a test ticket.",
         SubmitterId = user.Id,
         AssigneeId = user.Id,
         Environment = Environment.Browser,
         StepsToReproduce = "1. Do this\n2. Do that then do this again but with some more pizzazz",
         ExpectedResult = "Expected result is this the computer will blow up!",
         Files = new List<TicketFile>(),
         Submitter = user,
         Assignee = user,
         IsResolved = false,
      };

      user.AssignedTickets.Add(ticket);

      context.ApplicationUsers.Add(user);
      context.Tickets.Add(ticket);
      context.SaveChanges();
   }
}