namespace Core.Api.ApiClient.Models;

public sealed record ApiResponseWrapper<T>(
    bool Success,
    string Message,
    T? AdditionalData,
    int StatusCode,
    string? Instance,
    string? ErrorType = null,
    string? Detail = null,
    string? ErrorId = null)
{
    public bool IsSuccessful => Success && StatusCode >= 200 && StatusCode < 300;

    public static ApiResponseWrapper<T> CreateSuccessResponse(
        T data,
        string message = "",
        int statusCode = 200,
        string? instance = null) =>
        new(true, message, data, statusCode, instance);

    public static ApiResponseWrapper<T> CreateErrorResponse(
        string message,
        int statusCode = 500,
        string? errorType = null,
        string? detail = null,
        string? errorId = null,
        string? instance = null) =>
        new(false, message, default, statusCode, instance, errorType, detail, errorId);
}