namespace Kck.AspNetCore.Responses;

/// <summary>
/// Immutable record that wraps an API response payload together with status metadata
/// such as success flag, message, HTTP status code, and optional error details.
/// </summary>
public sealed record ApiResponse<T>
{
    /// <summary>Gets whether the operation completed successfully.</summary>
    public bool Success { get; init; } = true;

    /// <summary>Gets a human-readable message describing the outcome.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the response payload returned to the caller.</summary>
    public T? Data { get; init; }

    /// <summary>Gets the HTTP status code associated with this response.</summary>
    public int StatusCode { get; init; }

    /// <summary>Gets the request path that produced this response.</summary>
    public string? Instance { get; init; }

    /// <summary>Gets a short machine-readable error category (e.g. "ValidationError").</summary>
    public string? ErrorType { get; init; }

    /// <summary>Gets a detailed developer-facing error description.</summary>
    public string? Detail { get; init; }

    /// <summary>Gets the distributed trace identifier for correlating logs.</summary>
    public string? TraceId { get; init; }
}
