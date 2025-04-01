namespace Core.Api.ApiClient.Models;

public sealed record ApiResponseWrapper<T>(
    bool Success,
    string Message,
    T? Data,
    int StatusCode,
    string LocationHeader,
    string? ErrorType,
    string? Detail,
    object? AdditionalData)
{
    public bool IsSuccessful => Success && StatusCode >= 200 && StatusCode < 300;

    public static ApiResponseWrapper<T> CreateSuccessResponse(T data, string message = "", int statusCode = 200) =>
        new ApiResponseWrapper<T>(true, message, data, statusCode, string.Empty, null, null, null);

    public static ApiResponseWrapper<T> CreateErrorResponse(
        string message,
        int statusCode = 500,
        string? detail = null,
        string? errorType = null,
        object? additionalData = null) =>
        new ApiResponseWrapper<T>(false, message, default, statusCode, string.Empty, errorType, detail, additionalData);
}