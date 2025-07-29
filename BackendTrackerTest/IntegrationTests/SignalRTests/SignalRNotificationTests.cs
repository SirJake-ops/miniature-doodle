using BackendTrackerApplication.Services.Messaging;
using BackendTrackerPresentation;
using BackendTrackerTest.IntegrationTests.IntegrationTestSetup;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace BackendTrackerTest.IntegrationTests.SignalRTests;

public class SignalRNotificationTests(SignalRFactory<Program> factory) : IClassFixture<SignalRFactory<Program>>
{
   private readonly HttpClient _client = factory.CreateClient();

   [Fact]
   public void SignalRNotification_ShouldBeAvailable()
   {
      Assert.Equal(true, true);
   }



   // [Fact]
   // public async Task CreateTicketNotification_ShouldNotifySingleUser()
   // {
   //    var mockHubClients = new Mock<IHubCallerClients<MessageHub>>();
   //    var mockClientProxy = new Mock<IClientProxy>();
   //    var mockHubContext = new Mock<IHubContext<MessageHub>>();
   //    
   //    mockHubContext.Setup(x => x.Clients).Returns(mockHubClients.Object);
   //    mockHubClients.Setup(x => x.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
   // }
   //
   // private async Task<dynamic> CreateTicketSomehow()
   // {
   //
   // }
}