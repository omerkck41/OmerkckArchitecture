namespace Core.Api.ApiClient.Models;

public sealed record ApiResponseWrapper<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? AdditionalData { get; init; }
    public int StatusCode { get; init; }
    public string? Instance { get; init; }
    public string? ErrorType { get; init; }
    public string? Detail { get; init; }
    public string? ErrorId { get; init; }

    public bool IsSuccessful => Success && StatusCode is >= 200 and < 300;

    public static ApiResponseWrapper<T> CreateSuccessResponse(T data, string message = "", int statusCode = 200, string? instance = null)
        => new()
        {
            Success = true,
            Message = message,
            AdditionalData = data,
            StatusCode = statusCode,
            Instance = instance
        };

    public static ApiResponseWrapper<T> CreateErrorResponse(
        string message,
        int statusCode = 500,
        string? errorType = null,
        string? detail = null,
        string? errorId = null,
        string? instance = null)
        => new()
        {
            Success = false,
            Message = message,
            AdditionalData = default,
            StatusCode = statusCode,
            Instance = instance,
            ErrorType = errorType,
            Detail = detail,
            ErrorId = errorId
        };
}