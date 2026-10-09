using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;
using UserAccess.Api.Infrastructure.Persistence;
using Xunit;

namespace UserAccess.Api.Tests;

public sealed class UserRegistrationEndpointTests : IClassFixture<UserAccessApiFactory>
{
    private readonly UserAccessApiFactory _factory;

    public UserRegistrationEndpointTests(UserAccessApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void TestHost_UsesItsIsolatedDatabaseAndSigningKey()
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();

        Assert.Equal(_factory.ConnectionString, configuration.GetConnectionString("DefaultConnection"));
        Assert.Equal("integration-test-jwt-signing-key-at-least-32-bytes", configuration["Jwt:Key"]);
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        Assert.Equal(_factory.DatabasePath, dbContext.Database.GetDbConnection().DataSource);
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
        Assert.False((await response.Content.ReadAsStringAsync()).Contains("password", StringComparison.OrdinalIgnoreCase));

        var getResponse = await client.GetAsync($"/api/users/{payload.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(fetchedUser);
        Assert.Equal(payload.Data.Id, fetchedUser!.Id);
        Assert.Equal(request.Email, fetchedUser.Email);
    }

    [Fact]
    public async Task GetUsers_WithUnknownId_ReturnsNotFound()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithInvalidPayload_ReturnsStructuredValidationProblem()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            FirstName = "",
            LastName = "Lovelace",
            Email = "not-an-email",
            Password = "short"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var problem = document.RootElement;
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Equal("One or more validation errors occurred.", problem.GetProperty("title").GetString());
        Assert.Equal("UserAccess.Api", problem.GetProperty("service").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        var errors = problem.GetProperty("errors");
        Assert.Contains("FirstName", errors.EnumerateObject().Select(error => error.Name));
        Assert.Contains("Email", errors.EnumerateObject().Select(error => error.Name));
        Assert.Contains("Password", errors.EnumerateObject().Select(error => error.Name));
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

    [Fact]
    public async Task PostAuthToken_WithUnknownEmail_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/token", new CreateTokenRequest
        {
            Email = $"missing-{Guid.NewGuid():N}@example.com",
            Password = "Test123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
