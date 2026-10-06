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
}
