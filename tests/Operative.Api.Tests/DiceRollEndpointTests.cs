using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Operative.Api.Infrastructure.Persistence;
using Shared.Contracts;
using Xunit;

namespace Operative.Api.Tests;

public sealed class DiceRollEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Issuer = "https://localhost";
    private const string Audience = "UserAccess.Api";
    private const string SigningKey = "development-jwt-signing-key-for-interview-2026";
    private readonly WebApplicationFactory<Program> _factory;

    public DiceRollEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
    }

    [Fact]
    public async Task PostDiceRoll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/dice/roll", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostDiceRoll_WithValidToken_ReturnsTwoDiceAndPersistsRecord()
    {
        using var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId));

        var response = await client.PostAsync("/api/dice/roll", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<DiceRollResponse>>();
        Assert.NotNull(payload);
        Assert.Equal(userId, payload!.Data.UserId);
        Assert.InRange(payload.Data.Die1, 1, 6);
        Assert.InRange(payload.Data.Die2, 1, 6);
        Assert.Equal(payload.Data.Die1 + payload.Data.Die2, payload.Data.Sum);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OperativeDbContext>();
        var savedRoll = await dbContext.DiceRolls.SingleOrDefaultAsync(x => x.Id == payload.Data.Id);
        Assert.NotNull(savedRoll);
        Assert.Equal(userId, savedRoll!.UserId);
    }

    private static string CreateToken(Guid userId)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())],
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
