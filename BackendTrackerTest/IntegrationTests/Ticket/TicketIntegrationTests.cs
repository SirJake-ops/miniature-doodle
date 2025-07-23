using BackendTracker.Auth;
using BackendTracker.Ticket.NewFolder;
using BackendTracker.Ticket.PayloadAndResponse;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Ticket.FileUpload;
using BackendTrackerTest.IntegrationTests.IntegrationTestSetup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using BackendTrackerApplication.DTOs;
using BackendTrackerInfrastructure.Authentication;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerPresentation;
using Xunit.Abstractions;
using Environment = BackendTracker.Ticket.Enums.Environment;

namespace BackendTrackerTest.IntegrationTests.Ticket;

public class TicketIntegrationTests(BackendTrackerFactory<Program> factory, ITestOutputHelper testOutputHelper)
    : IClassFixture<BackendTrackerFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private ApplicationUser _user = null!;
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

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async void GetTickets_ShouldReturnTicketsForSubmitter()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}");

        var body = response.Content.ReadAsStringAsync().Result;
        testOutputHelper.WriteLine(body);

        response.EnsureSuccessStatusCode();

        var tickets = await response.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>();

        Assert.NotEmpty(tickets);
    }

    [Fact]
    public async void CreateTicket_ShouldCreateAndReturnATicketResponse()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

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
        });

        var body = response.Content.ReadAsStringAsync().Result;
        testOutputHelper.WriteLine(body);

        var ticket = await response.Content.ReadFromJsonAsync<TicketResponse>();

        Assert.NotNull(ticket);
        Assert.Equal(ticket.Title, "Test Ticket");
        Assert.Equal(ticket.SubmitterId, _user.Id);
    }


    [Fact]
    public async void DeleteTicket_ShouldBeAbleToDelete()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);


        var response = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}");

        var tickets = await response.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>();
        Guid ticketIdToDelete = tickets.FirstOrDefault().TicketId;


        var ticketDeleteResponse = await _client.DeleteAsync($"/api/tickets/{ticketIdToDelete}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    //
    // [Fact]
    // public async void UpdateTicketProp_ShouldPatchSingleProperty()
    // {
    //     _client.DefaultRequestHeaders.Authorization =
    //         new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
    //
    //
    //     var response = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}");
    //
    //     var tickets = await response.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>();
    //     Guid ticketToUpdate = tickets!.First().TicketId;
    //
    //     var updatedTitle = "Updated Ticket Title";
    //
    //     Dictionary<string, object> updatePayload = new Dictionary<string, object>
    //     {
    //         {
    //             "Title", updatedTitle
    //         }
    //     };
    //
    //     var patchResponse = await _client.PatchAsJsonAsync($"/api/tickets/{ticketToUpdate}", updatePayload);
    //
    //     patchResponse.EnsureSuccessStatusCode();
    //
    //     var verifyResponse = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}");
    //     var updatedTickets = await verifyResponse.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>();
    //
    //     Assert.Contains(updatedTickets, t => t.TicketId == ticketToUpdate && t.Title == updatedTitle);
    // }

    [Fact]
    public async void AssignTicketToUser_ShouldAssignTicketToUser()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
    
        var response = await _client.GetAsync($"/api/tickets?submitterId={_user.Id}");
    
        var tickets = await response.Content.ReadFromJsonAsync<List<BackendTrackerDomain.Entity.Ticket.Ticket>>();
    
        Guid ticketIdToAssign = tickets!.First().TicketId;
    
        var userToAssign = new UserTicketAssignDto
        {
            UserId = _user.Id 
        };
    
        var assignResponse = await _client.PostAsJsonAsync($"/api/tickets/{ticketIdToAssign}", userToAssign);
        assignResponse.EnsureSuccessStatusCode();
    
        var assignedTicket = await assignResponse.Content.ReadFromJsonAsync<TicketResponse>();
    
        var db = await factory.Services.GetRequiredService<IDbContextFactory<ApplicationContext>>().CreateDbContextAsync();
    
        var updatedUser = await db.ApplicationUsers
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == _user.Id);
    
    
        Assert.Equal(1, updatedUser!.AssignedTickets.Count);
        Assert.NotNull(assignedTicket);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}