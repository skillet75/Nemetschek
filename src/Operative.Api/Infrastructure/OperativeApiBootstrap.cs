using Microsoft.AspNetCore.Diagnostics;
using Shared.Contracts;
using Operative.Api.Infrastructure.Persistence;

namespace Operative.Api.Infrastructure;

public static class OperativeApiBootstrap
{
    public static WebApplication BuildApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["service"] = "Operative.Api";
            };
        });
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOperativePersistence(builder.Configuration, builder.Environment);

        var app = builder.Build();

        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error ??
                    new ApiException("An unexpected error occurred.", 500);

                var statusCode = exception is ApiException apiException
                    ? apiException.StatusCode
                    : 500;

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var response = ErrorResponse.FromException(exception, statusCode, context.TraceIdentifier);
                await context.Response.WriteAsJsonAsync(response);
            });
        });

        app.UseStatusCodePages(async statusCodeContext =>
        {
            if (statusCodeContext.HttpContext.Response.HasStarted)
            {
                return;
            }

            statusCodeContext.HttpContext.Response.ContentType = "application/json";
            var statusCode = statusCodeContext.HttpContext.Response.StatusCode;
            await statusCodeContext.HttpContext.Response.WriteAsJsonAsync(
                new ErrorResponse("The request could not be processed.", statusCode, statusCodeContext.HttpContext.TraceIdentifier));
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapHealthChecks("/health");

        app.MapGet("/api/info", () => Results.Ok(ApiResponse<string>.Ok(
            "Operative.Api",
            "Operative microservice for dice operations and user history.")));

        return app;
    }
}
