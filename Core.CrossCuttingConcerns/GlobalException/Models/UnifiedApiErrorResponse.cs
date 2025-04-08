using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using System.Text.Json.Serialization;

namespace Core.CrossCuttingConcerns.GlobalException.Models
{
    /// <summary>
    /// RFC 7807 Problem Details standardına uyumlu hata yanıtı.
    /// .NET 9.0'un record özellikleriyle geliştirilmiş versiyon.
    /// </summary>
    public sealed record UnifiedApiErrorResponse
    {
        public string? ErrorId { get; init; }
        public bool Success { get; init; } = false;
        public int StatusCode { get; init; }
        public string? Type { get; init; }
        public string Title { get; init; }
        public string Message { get; init; }
        public string? ErrorType { get; init; }
        public string? Detail { get; init; }
        public string? Instance { get; init; }
        public object? AdditionalData { get; init; }

        [JsonConstructor]
        public UnifiedApiErrorResponse(
            int statusCode,
            string title,
            string message,
            string? errorId,
            bool success,
            string? type,
            string? errorType,
            string? detail,
            string? instance,
            object? additionalData)
        {
            StatusCode = statusCode;
            Title = title;
            Message = message;
            ErrorId = errorId;
            Success = success;
            Type = type;
            ErrorType = errorType;
            Detail = detail;
            Instance = instance;
            AdditionalData = additionalData;
        }

        // Diğer factory metotlarınız değişmeden kullanılabilir.
        public static UnifiedApiErrorResponse FromException(CustomException ex)
        {
            return new UnifiedApiErrorResponse(
                ex.ExplicitStatusCode ?? StatusCodes.Status500InternalServerError,
                ex.GetType().Name,
                ex.Message,
                null,
                false,
                null,
                ex.GetType().Name,
                ex.Message,
                null,
                ex.AdditionalData
            );
        }

        public UnifiedApiErrorResponse WithAdditionalData(object data) => this with { AdditionalData = data };

        public static UnifiedApiErrorResponse FromValidationException(ValidationException ex, string errorId, string? detail = null)
        {
            return new UnifiedApiErrorResponse(
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "Validation error",
                errorId,
                false,
                null,
                nameof(ValidationException),
                detail ?? "Validation failed",
                null,
                ex.Errors
            );
        }

        public UnifiedApiErrorResponse WithDetail(string detail) => this with { Detail = detail };

        public static UnifiedApiErrorResponse CreateInternalServerError(string errorId, string? detail = null)
        {
            return new UnifiedApiErrorResponse(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred",
                errorId,
                false,
                null,
                "InternalServerError",
                detail ?? "See logs for details",
                null,
                null
            );
        }
    }
}
