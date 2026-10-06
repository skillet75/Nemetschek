using Shared.Contracts;

namespace Operative.Api.Infrastructure;

public static class OperativeApiBootstrap
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
            service = "Operative.Api",
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow
        }));

        app.MapGet("/api/info", () => Results.Ok(ApiResponse<string>.Ok(
            "Operative.Api",
            "Operative microservice for dice operations and user history.")));

        return app;
    }
}
