using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Shared.Contracts;
using Operative.Api.Application.Authentication;
using Operative.Api.Infrastructure.Authentication;
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

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter a valid token issued by UserAccess.Api."
                };

                return Task.CompletedTask;
            });
            options.AddOperationTransformer((operation, context, _) =>
            {
                var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;
                if (endpointMetadata.OfType<IAuthorizeData>().Any() &&
                    !endpointMetadata.OfType<IAllowAnonymous>().Any())
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                    });
                }

                return Task.CompletedTask;
            });
        });
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
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");
        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
            string.IsNullOrWhiteSpace(jwtSettings.Audience) ||
            Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
        {
            throw new InvalidOperationException(
                "JWT issuer, audience, and a signing key of at least 32 bytes must be configured.");
        }

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
                };
            });
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
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

        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference().AllowAnonymous();
        }

        app.MapHealthChecks("/health").AllowAnonymous();

        app.MapGet("/api/info", () => Results.Ok(ApiResponse<string>.Ok(
            "Operative.Api",
            "Operative microservice for dice operations and user history.")))
            .AllowAnonymous();

        app.MapGet("/api/me", (ICurrentUser currentUser) =>
                Results.Ok(ApiResponse<Guid>.Ok(currentUser.UserId, "Authenticated user.")))
            .WithName("GetCurrentUser")
            .WithSummary("Returns the authenticated user's identifier")
            .WithDescription("Resolves the user identifier from the validated JWT subject claim.")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        return app;
    }
}
