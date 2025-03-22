namespace Core.Api.ApiClient.Models;

public class ApiResponseWrapper<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public string LocationHeader { get; set; } = string.Empty;
    public string? ErrorType { get; set; }
    public string? Detail { get; set; }
    public object? AdditionalData { get; set; }

    public bool IsSuccessful => Success && StatusCode >= 200 && StatusCode < 300;

    public static ApiResponseWrapper<T> CreateSuccessResponse(T data, string message = "", int statusCode = 200)
    {
        return new ApiResponseWrapper<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponseWrapper<T> CreateErrorResponse(string message, int statusCode = 500, string? detail = null, string? errorType = null, object? additionalData = null)
    {
        return new ApiResponseWrapper<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Detail = detail,
            ErrorType = errorType,
            AdditionalData = additionalData
        };
    }
}