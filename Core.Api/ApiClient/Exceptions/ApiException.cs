using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Api.ApiClient.Exceptions;

public class ApiException : CustomException
{
    public ApiException(string message) : base(message)
    {
    }

    public ApiException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ApiException(
        string message,
        int statusCode,
        string? errorType = null,
        string? detail = null,
        object? additionalData = null)
        : base(message, statusCode, additionalData, null)
    {
        // İsteğe bağlı: errorType ve detail bilgilerini AdditionalData içine ekleyebilir veya ayrı property olarak atayabilirsiniz.
    }
}