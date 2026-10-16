
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RaceDay.Api.Tests;

public class EventsControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EventsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

    private static async Task<int> RegisterUser(
        HttpClient client, string role)
    {
        var email = $"{role.ToLower()}_{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync(
            "/api/Auth/Register",
            new
            {
                FirstName = "Test",
                LastName = role,
                Email = email,
                Password = "Race123!",
                DateOfBirth = new DateTime(1995, 5, 10),
                PhoneNumber = "+27821234567",
                Role = role
            });

        response.EnsureSuccessStatusCode();

        // The registration endpoint doesn't return UserId.
        // Login returns it, so log in to establish the session.
        var login = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new
            {
                Email = email,
                Password = "Race123!",
                Role = role
            });

        login.EnsureSuccessStatusCode();

        var result = await login.Content.ReadFromJsonAsync<LoginResult>();
        Assert.NotNull(result);

        return result.UserId;
    }

    private sealed class LoginResult
    {
        public int UserId { get; set; }
        public string? Role { get; set; }
    }

    private static object EventRequest(int organiserId) => new
    {
        OrganiserId = organiserId,
        Name = "RaceDay Test Event",
        Description = "Automated test event",
        EventDate = new DateTime(2030, 5, 10),
        Location = "Cape Town",
        Distance = 10.0m,
        BannerImageUrl = (string?)null
    };

    [Fact]
    public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/Events", EventRequest(1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_ReturnsForbidden()
    {
        using var client = CreateClient();
        var participantId = await RegisterUser(client, "Participant");

        var response = await client.PostAsJsonAsync(
            "/api/Events", EventRequest(participantId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        using var client = CreateClient();
        var organiserId = await RegisterUser(client, "Organiser");

        var response = await client.PostAsJsonAsync(
            "/api/Events", EventRequest(organiserId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
