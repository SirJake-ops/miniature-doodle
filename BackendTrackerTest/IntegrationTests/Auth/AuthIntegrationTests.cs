using BackendTrackerPresentation;
using BackendTrackerTest.IntegrationTests.IntegrationTestSetup;
using System.Net.Http.Json;
using System.Text.Json;

namespace BackendTrackerTest.IntegrationTests.Auth;

public class AuthIntegrationTests(BackendTrackerFactory<Program> factory)
    : IClassFixture<BackendTrackerFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_ShouldReturnTokenWhenUserIsValid()
    {
        var loginRequest = new 
        {
            UserName = "testuser",
            Password = "123abc"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.EnsureSuccessStatusCode();
        
        var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(responseData.TryGetProperty("token", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

}