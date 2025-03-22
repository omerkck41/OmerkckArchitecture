namespace Core.Api.ApiClient.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorType { get; }
    public string? Detail { get; }
    public object? AdditionalData { get; }

    public ApiException(string message) : base(message) { }

    public ApiException(string message, Exception innerException) : base(message, innerException) { }

    public ApiException(string message, int statusCode, string? errorType = null, string? detail = null, object? additionalData = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorType = errorType;
        Detail = detail;
        AdditionalData = additionalData;
    }
}