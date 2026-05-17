namespace Kck.Http.Abstractions;

/// <summary>
/// Wraps the result of an HTTP call with a success flag, deserialized body, HTTP status code, and an optional error message.
/// </summary>
public sealed class ApiResponse<T>
{
    /// <summary>Gets a value indicating whether the HTTP request completed successfully (2xx status).</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Gets the deserialized response body, or <see langword="null"/> on failure.</summary>
    public T? Data { get; init; }

    /// <summary>Gets the HTTP status code returned by the server.</summary>
    public int StatusCode { get; init; }

    /// <summary>Gets the error message when <see cref="IsSuccess"/> is <see langword="false"/>, or <see langword="null"/> on success.</summary>
    public string? ErrorMessage { get; init; }

#pragma warning disable CA1000
    /// <summary>Creates a successful <see cref="ApiResponse{T}"/> with the given data and optional status code.</summary>
    public static ApiResponse<T> Success(T data, int statusCode = 200) =>
        new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    /// <summary>Creates a failed <see cref="ApiResponse{T}"/> with the given error message and HTTP status code.</summary>
    public static ApiResponse<T> Failure(string error, int statusCode) =>
        new() { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };
#pragma warning restore CA1000
}
