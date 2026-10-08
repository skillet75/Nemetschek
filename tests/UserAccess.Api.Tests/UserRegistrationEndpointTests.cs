using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Contracts;
using Xunit;

namespace UserAccess.Api.Tests;

public sealed class UserRegistrationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserRegistrationEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostUsers_WithValidPayload_ReturnsCreatedUser()
    {
        using var client = _factory.CreateClient();
        var email = $"ada-{Guid.NewGuid():N}@example.com";

        var request = new CreateUserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = email,
            Password = "Test123!",
            Image = "data:image/png;base64,abc123"
        };

        var response = await client.PostAsJsonAsync("/api/users", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        Assert.NotNull(payload);
        Assert.Equal("Ada", payload!.Data.FirstName);
        Assert.Equal(request.Email, payload.Data.Email);
    }

    [Fact]
    public async Task PostUsers_WithDuplicateEmail_ReturnsConflict()
    {
        using var client = _factory.CreateClient();
        var email = $"grace-{Guid.NewGuid():N}@example.com";

        var request = new CreateUserRequest
        {
            FirstName = "Grace",
            LastName = "Hopper",
            Email = email,
            Password = "Test123!"
        };

        var firstResponse = await client.PostAsJsonAsync("/api/users", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync("/api/users", request);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task PostAuthToken_WithValidCredentials_ReturnsJwt()
    {
        using var client = _factory.CreateClient();
        var email = $"token-{Guid.NewGuid():N}@example.com";
        var password = "Test123!";

        var createUser = new CreateUserRequest
        {
            FirstName = "Alan",
            LastName = "Turing",
            Email = email,
            Password = password
        };

        var createResponse = await client.PostAsJsonAsync("/api/users", createUser);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var request = new CreateTokenRequest
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync("/api/auth/token", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Data.AccessToken));
        Assert.Equal("Bearer", payload.Data.TokenType);
        Assert.True(payload.Data.ExpiresAtUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task PostAuthToken_WithInvalidCredentials_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        var email = $"invalid-{Guid.NewGuid():N}@example.com";

        var createUser = new CreateUserRequest
        {
            FirstName = "Margaret",
            LastName = "Hamilton",
            Email = email,
            Password = "Test123!"
        };

        var createResponse = await client.PostAsJsonAsync("/api/users", createUser);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var request = new CreateTokenRequest
        {
            Email = email,
            Password = "WrongPassword!"
        };

        var response = await client.PostAsJsonAsync("/api/auth/token", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
