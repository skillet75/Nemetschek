using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Shared.Contracts;
using UserAccess.Api.Application.Authentication;
using UserAccess.Api.Infrastructure.Persistence;

namespace UserAccess.Api.Infrastructure;

public static class UserAccessApiBootstrap
{
    public static WebApplication BuildApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        builder.Services.AddOpenApi();
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers().AddApiErrorHandling("UserAccess.Api");
        builder.Services.AddScoped<TokenAuthenticationService>();
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.AddUserPersistence(builder.Configuration, builder.Environment);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            dbContext.Database.Migrate();
        }

        app.UseApiErrorHandling();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapHealthChecks("/health");

        app.MapGet("/api/info", () => Results.Ok(ApiResponse<string>.Ok(
            "UserAccess.Api",
            "User access microservice for user registration and authentication.")));

        app.MapControllers();

        return app;
    }
}
