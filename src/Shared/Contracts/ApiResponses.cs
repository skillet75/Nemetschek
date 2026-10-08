namespace Shared.Contracts;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode = 500)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(string message, Exception innerException, int statusCode = 500)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

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

public sealed record DiceRollResponse(
    Guid Id,
    Guid UserId,
    int Die1,
    int Die2,
    int Sum,
    DateTime CreatedAtUtc);
