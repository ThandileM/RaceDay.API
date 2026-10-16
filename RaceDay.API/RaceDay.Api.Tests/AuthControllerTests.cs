
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Session_WithoutLogin_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/Auth/session");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new
        {
            Email = "unknown@example.com",
            Password = "WrongPassword123!",
            Role = "Participant"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login", request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Register_WithoutDateOfBirth_ReturnsBadRequest()
    {
        var request = new
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "nodob@example.com",
            Password = "Password123!",
            PhoneNumber = "+27821234567",
            Role = "Participant",
            DateOfBirth = (DateTime?)null
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/Register", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        var request = new
        {
            FirstName = "Test",
            LastName = "User",
            Email = "invalidrole@example.com",
            Password = "Password123!",
            PhoneNumber = "+27821234567",
            Role = "Administrator",
            DateOfBirth = new DateTime(2000, 1, 1)
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/Register", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task Register_WithValidDetails_ReturnsOk()
    {
        var request = new
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = $"participant_{Guid.NewGuid():N}@example.com",
            Password = "Race123!",
            DateOfBirth = new DateTime(2000, 1, 1),
            PhoneNumber = "+27821234567",
            Role = "Participant"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/Register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange: register a new user first.
        var email = $"login_{Guid.NewGuid():N}@example.com";

        var registerRequest = new
        {
            FirstName = "Test",
            LastName = "Organiser",
            Email = email,
            Password = "Race123!",
            DateOfBirth = new DateTime(1995, 5, 10),
            PhoneNumber = "+27821234567",
            Role = "Organiser"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/Register", registerRequest);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Act: log in with the registered credentials.
        var loginRequest = new
        {
            Email = email,
            Password = "Race123!",
            Role = "Organiser"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login", loginRequest);

        var responseBody = await loginResponse.Content.ReadAsStringAsync();

        Console.WriteLine($"Login status: {(int)loginResponse.StatusCode}");
        Console.WriteLine($"Login response: {responseBody}");


        // Assert
    
        Assert.True(
            loginResponse.StatusCode == HttpStatusCode.OK,
            $"Login returned {(int)loginResponse.StatusCode}: {responseBody}");
        Assert.Contains("Login successful.", responseBody);
    }

}
