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
using Operative.Api.Domain.Entities;
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

    [Fact]
    public async Task GetDiceHistory_WithUserScopedFilter_ReturnsOnlyCurrentUsersRecords()
    {
        using var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId));

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OperativeDbContext>();
            dbContext.DiceRolls.AddRange(
                new DiceRoll(userId, 1, 2),
                new DiceRoll(userId, 3, 4),
                new DiceRoll(otherUserId, 5, 6));
            await dbContext.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/dice/history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DiceRollResponse>>>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Data.Count);
        Assert.All(payload.Data, x => Assert.Equal(userId, x.UserId));

        var currentYear = DateTime.UtcNow.Year;
        var monthYear = $"{DateTime.UtcNow.Month:D2}/{currentYear}";
        var day = DateTime.UtcNow.Day.ToString();

        var yearResponse = await client.GetAsync($"/api/dice/history?year={currentYear}");
        var yearPayload = await yearResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DiceRollResponse>>>();
        Assert.NotNull(yearPayload);
        Assert.Equal(2, yearPayload!.Data.Count);

        var monthYearResponse = await client.GetAsync($"/api/dice/history?monthYear={Uri.EscapeDataString(monthYear)}");
        var monthYearPayload = await monthYearResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DiceRollResponse>>>();
        Assert.NotNull(monthYearPayload);
        Assert.Equal(2, monthYearPayload!.Data.Count);

        var dayResponse = await client.GetAsync($"/api/dice/history?day={Uri.EscapeDataString(day)}");
        var dayPayload = await dayResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DiceRollResponse>>>();
        Assert.NotNull(dayPayload);
        Assert.Equal(2, dayPayload!.Data.Count);
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
