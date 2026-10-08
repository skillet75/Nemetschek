using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
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
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["service"] = "UserAccess.Api";
            };
        });
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers();
        builder.Services.AddScoped<TokenAuthenticationService>();
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.AddUserPersistence(builder.Configuration, builder.Environment);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            dbContext.Database.Migrate();
        }

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
