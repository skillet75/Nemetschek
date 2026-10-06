using Shared.Contracts;

namespace UserAccess.Api.Infrastructure;

public static class UserAccessApiBootstrap
{
    public static WebApplication BuildApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails();
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapGet("/health", () => Results.Ok(new
        {
            service = "UserAccess.Api",
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow
        }));

        app.MapGet("/api/info", () => Results.Ok(ApiResponse<string>.Ok(
            "UserAccess.Api",
            "User access microservice for user registration and authentication.")));

        return app;
    }
}
