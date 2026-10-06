namespace Shared.Contracts;

public sealed record ApiResponse<T>(T Data, string Message, DateTimeOffset Timestamp)
{
    public static ApiResponse<T> Ok(T data, string message = "Request completed successfully.") =>
        new(data, message, DateTimeOffset.UtcNow);
}

public sealed record ErrorResponse(string Message, int StatusCode, string? TraceId = null)
{
    public static ErrorResponse FromException(Exception exception, int statusCode = 500, string? traceId = null) =>
        new(exception.Message, statusCode, traceId ?? Guid.NewGuid().ToString("N"));
}
