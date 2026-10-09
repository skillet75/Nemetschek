using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<DiceRollResponse>>>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Data.Items.Count);
        Assert.All(payload.Data.Items, x => Assert.Equal(userId, x.UserId));

        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;
        var currentDay = DateTime.UtcNow.Day;

        var yearResponse = await client.GetAsync($"/api/dice/history?year={currentYear}");
        var yearPayload = await yearResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<DiceRollResponse>>>();
        Assert.NotNull(yearPayload);
        Assert.Equal(2, yearPayload!.Data.Items.Count);

        var monthResponse = await client.GetAsync($"/api/dice/history?year={currentYear}&month={currentMonth}");
        var monthPayload = await monthResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<DiceRollResponse>>>();
        Assert.NotNull(monthPayload);
        Assert.Equal(2, monthPayload!.Data.Items.Count);

        var dayResponse = await client.GetAsync($"/api/dice/history?year={currentYear}&month={currentMonth}&day={currentDay}");
        var dayPayload = await dayResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<DiceRollResponse>>>();
        Assert.NotNull(dayPayload);
        Assert.Equal(2, dayPayload!.Data.Items.Count);
    }

    [Fact]
    public async Task OpenApiDocument_DescribesDiceHistorySortQueryValuesAndPrecedence()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var parameters = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/dice/history")
            .GetProperty("get")
            .GetProperty("parameters");

        AssertQueryParameterDescription(parameters, "dateSort", "Accepted values: asc or desc.");
        AssertQueryParameterDescription(parameters, "dateSort", "sum is sorted first and date/time breaks ties.");
        AssertQueryParameterDescription(parameters, "sumSort", "Accepted values: asc or desc.");
        AssertQueryParameterDescription(parameters, "sumSort", "sum is sorted first and date/time second.");
    }

    [Fact]
    public async Task GetDiceHistory_WithCombinedSorts_UsesDiceSumFirstThenDateAsSecondaryKey()
    {
        using var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId));

        DiceRoll oldestSameSum;
        DiceRoll newestSameSum;
        DiceRoll highValue;
        DiceRoll lowValue;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OperativeDbContext>();
            oldestSameSum = new DiceRoll(userId, 3, 4);
            newestSameSum = new DiceRoll(userId, 2, 5);
            highValue = new DiceRoll(userId, 5, 4);
            lowValue = new DiceRoll(userId, 1, 1);

            SetCreatedAtUtc(oldestSameSum, DateTime.UtcNow.AddMinutes(-30));
            SetCreatedAtUtc(newestSameSum, DateTime.UtcNow.AddMinutes(-10));
            SetCreatedAtUtc(highValue, DateTime.UtcNow.AddMinutes(-20));
            SetCreatedAtUtc(lowValue, DateTime.UtcNow.AddMinutes(-40));

            dbContext.DiceRolls.AddRange(oldestSameSum, newestSameSum, highValue, lowValue);
            await dbContext.SaveChangesAsync();
        }

        async Task<Guid[]> GetHistoryIds(string sortingQuery)
        {
            var response = await client.GetAsync($"/api/dice/history?{sortingQuery}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<DiceRollResponse>>>();
            Assert.NotNull(payload);
            Assert.Equal(4, payload!.Data.Items.Count);
            return payload.Data.Items.Select(roll => roll.Id).ToArray();
        }

        Assert.Equal(
            new[] { highValue.Id, oldestSameSum.Id, newestSameSum.Id, lowValue.Id },
            await GetHistoryIds("dateSort=asc&sumSort=desc"));
        Assert.Equal(
            new[] { lowValue.Id, newestSameSum.Id, oldestSameSum.Id, highValue.Id },
            await GetHistoryIds("sumSort=asc"));
        Assert.Equal(
            new[] { lowValue.Id, oldestSameSum.Id, highValue.Id, newestSameSum.Id },
            await GetHistoryIds("dateSort=asc"));
        Assert.Equal(
            new[] { newestSameSum.Id, highValue.Id, oldestSameSum.Id, lowValue.Id },
            await GetHistoryIds("dateSort=desc"));
        Assert.Equal(
            new[] { lowValue.Id, newestSameSum.Id, oldestSameSum.Id, highValue.Id },
            await GetHistoryIds("sumSort=asc&dateSort=desc"));
    }

    private static void SetCreatedAtUtc(DiceRoll roll, DateTime createdAtUtc)
    {
        var property = typeof(DiceRoll).GetProperty(nameof(DiceRoll.CreatedAtUtc));
        Assert.NotNull(property);
        property!.SetValue(roll, createdAtUtc);
    }

    private static void AssertQueryParameterDescription(JsonElement parameters, string name, string expectedDescription)
    {
        var parameter = Assert.Single(parameters.EnumerateArray(), item => item.GetProperty("name").GetString() == name);
        Assert.Equal("query", parameter.GetProperty("in").GetString());
        Assert.Contains(expectedDescription, parameter.GetProperty("description").GetString());
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
