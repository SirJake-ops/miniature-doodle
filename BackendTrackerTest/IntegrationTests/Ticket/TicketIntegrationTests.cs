using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BackendTracker.Auth;
using BackendTracker.Ticket.NewFolder;
using BackendTracker.Ticket.PayloadAndResponse;
using BackendTrackerApplication.Dtos;
using BackendTrackerApplication.DTOs;
using BackendTrackerDomain.Entity.Ticket.FileUpload;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation;
using BackendTrackerTest.IntegrationTests.IntegrationTestSetup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Environment = BackendTracker.Ticket.Enums.Environment;

namespace BackendTrackerTest.IntegrationTests.Ticket;

public class TicketIntegrationTests(BackendTrackerFactory<Program> factory)
    : IClassFixture<BackendTrackerFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private ApplicationUserDto _user = null!;
    private string _token = null!;

    public async ValueTask InitializeAsync()
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

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task GetTickets_ShouldReturnTicketsForSubmitter()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}", TestContext.Current.CancellationToken);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(body);

        response.EnsureSuccessStatusCode();

        var tickets = await response.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(tickets);
        Assert.All(tickets, ticket => Assert.Equal(_user.Id, ticket.SubmitterId));
    }
    
    [Fact]
    public async Task CreateTicket_ShouldCreateAndReturnATicketResponse()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);
    
        var response = await _client.PostAsJsonAsync("/api/tickets", new TicketRequestBody
        {
            Environment = Environment.Device,
            Title = "Test Ticket",
            Description = "This is a test ticket.",
            StepsToReproduce = "1. Do this\n2. Do that",
            ExpectedResult = "Expected outcome",
            SubmitterId = _user.Id,
            Files = new List<TicketFile>(),
            IsResolved = false
        }, cancellationToken: TestContext.Current.CancellationToken);
    
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    
        var ticket = await response.Content.ReadFromJsonAsync<TicketResponse>(cancellationToken: TestContext.Current.CancellationToken);
    
        Assert.NotNull(ticket);
        Assert.Equal("Test Ticket", ticket.Title);
        Assert.Equal(ticket.SubmitterId, _user.Id);
    }


    [Fact]
    public async Task DeleteTicket_ShouldBeAbleToDelete()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);


        var response = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}", TestContext.Current.CancellationToken);

        var tickets = await response.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>(cancellationToken: TestContext.Current.CancellationToken);

        if (tickets != null)
        {
            Guid ticketIdToDelete = tickets.FirstOrDefault()!.TicketId;
            Assert.NotEqual(Guid.Empty, ticketIdToDelete);


            var ticketDeleteResponse = await _client.DeleteAsync($"/api/tickets/{ticketIdToDelete}", TestContext.Current.CancellationToken);
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignTicketToUser_ShouldAssignTicketToUser()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);
    
        var response = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}", TestContext.Current.CancellationToken);
    
        var tickets = await response.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>(cancellationToken: TestContext.Current.CancellationToken);
    
        Guid ticketIdToAssign = tickets!.First().TicketId;
    
        var userToAssign = new UserTicketAssignDto
        {
            UserId = _user.Id 
        };
    
        var assignResponse = await _client.PostAsJsonAsync($"/api/tickets/{ticketIdToAssign}", userToAssign, cancellationToken: TestContext.Current.CancellationToken);
        assignResponse.EnsureSuccessStatusCode();
    
        var assignedTicket = await assignResponse.Content.ReadFromJsonAsync<TicketResponse>(cancellationToken: TestContext.Current.CancellationToken);
    
        var db = await factory.Services.GetRequiredService<IDbContextFactory<ApplicationContext>>().CreateDbContextAsync(TestContext.Current.CancellationToken);
    
        var updatedUser = await db.ApplicationUsers
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == _user.Id, cancellationToken: TestContext.Current.CancellationToken);
    
    
        Assert.Single(updatedUser!.AssignedTickets);
        Assert.NotNull(assignedTicket);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateTicket_ShouldUpdateTicket()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}", TestContext.Current.CancellationToken);

        var tickets = await response.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>(cancellationToken: TestContext.Current.CancellationToken);

        Guid ticketIdToUpdate = tickets!.First().TicketId;

        var updateResponse = await _client.PutAsJsonAsync($"/api/tickets/{ticketIdToUpdate}", new TicketRequestBody
        {
            Environment = Environment.Browser,
            Title = "Updated Test Ticket",
            Description = "This is an updated test ticket.",
            SubmitterId = _user.Id,
            StepsToReproduce = "1. Do this\n2. Do that",
            ExpectedResult = "Expected outcome",
            Files = new List<TicketFile>(),
            IsResolved = false
        }, cancellationToken: TestContext.Current.CancellationToken);

        updateResponse.EnsureSuccessStatusCode();

        var updatedTicket = await updateResponse.Content.ReadFromJsonAsync<TicketResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(updatedTicket);
        Assert.Equal("Updated Test Ticket", updatedTicket.Title);
    }
}