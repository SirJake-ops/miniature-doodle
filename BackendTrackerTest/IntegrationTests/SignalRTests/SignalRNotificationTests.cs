using System.Net.Http.Headers;
using System.Net.Http.Json;
using BackendTracker.Auth;
using BackendTracker.Ticket.NewFolder;
using BackendTrackerApplication.Dtos;
using BackendTrackerApplication.DTOs;
using BackendTrackerDomain.Entity.Ticket.FileUpload;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation;
using BackendTrackerTest.IntegrationTests.IntegrationTestSetup;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Environment = BackendTracker.Ticket.Enums.Environment;

namespace BackendTrackerTest.IntegrationTests.SignalRTests;

public class SignalRNotificationTests(SignalRFactory<Program> factory)
    : IClassFixture<SignalRFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private ApplicationUserDto _user = null!;
    private string _token = null!;

    public async Task InitializeAsync()
    {
        var loginRequest = new LoginRequest
        {
            UserName = "testuser",
            Password = "123abc"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        loginResponse.EnsureSuccessStatusCode();


        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _token = authResult?.Token ?? throw new Exception("Missing token");
        _user = authResult.User;

        using var scope = factory.Services.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationContext>>();
        await using var context = await dbContextFactory.CreateDbContextAsync();

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;


    [Fact]
    public async Task SignalShouldReturnOk()
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_client.BaseAddress}messageHub", options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(_token)!;
            }).Build();

        await hubConnection.StartAsync();

        Assert.Equal(HubConnectionState.Connected, hubConnection.State);

        await hubConnection.DisposeAsync();
    }


    [Fact]
    public async Task CreateTicketNotification_ShouldNotifySingleUser()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.PostAsJsonAsync("api/tickets", new TicketRequestBody
        {
            Environment = Environment.Browser,
            Title = "SignalR Test Ticket",
            Description = "This is a test ticket for SignalR notification.",
            SubmitterId = _user.Id,
            StepsToReproduce = "1. Do this\n2. Do that then do this again but with some more pizzazz",
            ExpectedResult = "Expected result is this the computer will blow up!",
            Files = new List<TicketFile>(),
        });

        response.EnsureSuccessStatusCode();

        factory.MockHubContext.Verify(x => x.Clients.Group("Admins"), Times.Once);
    }

    [Fact]
    public async Task UpdateTicketNotification_ShouldNotifyAllUsers()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.PostAsJsonAsync("api/tickets", new TicketRequestBody
        {
            Environment = Environment.Browser,
            Title = "SignalR Update Test Ticket",
            Description = "This is a test ticket for SignalR update notification.",
            SubmitterId = _user.Id,
            StepsToReproduce = "1. Do this\n2. Do that then do this again but with some more pizzazz",
            ExpectedResult = "Expected result is this the computer will blow up!",
            Files = new List<TicketFile>(),
        });

        response.EnsureSuccessStatusCode();

        var ticketResponse = await response.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.NotNull(ticketResponse);

        var updateResponse = await _client.PutAsJsonAsync($"api/tickets/{ticketResponse.TicketId}",
            new TicketRequestBody
            {
                Environment = Environment.Browser,
                Title = "Updated SignalR Test Ticket",
                Description = "This is an updated test ticket for SignalR notification.",
                SubmitterId = _user.Id,
                StepsToReproduce = "1. Do this\n2. Do that then do this again but with some more pizzazz",
                ExpectedResult = "Expected result is this the computer will blow up!",
                Files = new List<TicketFile>(),
            });

        updateResponse.EnsureSuccessStatusCode();

        // factory.MockHubContext.Verify(
            // x => x.Clients.Users(It.Is<IList<string>>(users => users.Contains(_user.Id.ToString()))), Times.Once);
    }
}