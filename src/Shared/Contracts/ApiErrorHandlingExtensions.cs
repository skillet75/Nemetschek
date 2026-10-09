using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shared.Contracts;

public static class ApiErrorHandlingExtensions
{
    public static IMvcBuilder AddApiErrorHandling(this IMvcBuilder mvcBuilder, string serviceName)
    {
        mvcBuilder.Services.Configure<ApiErrorHandlingOptions>(options => options.ServiceName = serviceName);
        mvcBuilder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["service"] = serviceName;
            };
        });

        return mvcBuilder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Instance = context.HttpContext.Request.Path
                };
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                problemDetails.Extensions["service"] = serviceName;

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });
    }

    public static IApplicationBuilder UseApiErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            var apiException = exception as ApiException;
            var statusCode = apiException?.StatusCode ?? StatusCodes.Status500InternalServerError;
            var isSafeClientError = apiException is not null && statusCode is >= 400 and < 500;
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = ReasonPhrases.GetReasonPhrase(statusCode),
                Detail = isSafeClientError ? apiException!.Message : "An unexpected error occurred.",
                Instance = context.Request.Path,
                Type = "about:blank"
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            problem.Extensions["service"] = context.RequestServices.GetRequiredService<IOptions<ApiErrorHandlingOptions>>().Value.ServiceName;
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await JsonSerializer.SerializeAsync(context.Response.Body, problem, cancellationToken: context.RequestAborted);
        }));

        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.HasStarted || context.Response.StatusCode < 400)
            {
                return;
            }

            var statusCode = context.Response.StatusCode;
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = ReasonPhrases.GetReasonPhrase(statusCode),
                Detail = "The request could not be processed.",
                Instance = context.Request.Path,
                Type = "about:blank"
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            problem.Extensions["service"] = context.RequestServices.GetRequiredService<IOptions<ApiErrorHandlingOptions>>().Value.ServiceName;
            context.Response.ContentType = "application/problem+json";
            await JsonSerializer.SerializeAsync(context.Response.Body, problem, cancellationToken: context.RequestAborted);
        });

        return app;
    }
}

public sealed class ApiErrorHandlingOptions
{
    public string ServiceName { get; set; } = string.Empty;
}
