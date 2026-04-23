namespace Kck.AspNetCore.Responses;

public sealed record ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public int StatusCode { get; init; }
    public string? Instance { get; init; }
    public string? ErrorType { get; init; }
    public string? Detail { get; init; }
    public string? TraceId { get; init; }
}
