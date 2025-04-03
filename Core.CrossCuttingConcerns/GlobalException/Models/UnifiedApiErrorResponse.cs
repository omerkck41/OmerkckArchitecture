using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.CrossCuttingConcerns.GlobalException.Models;

/// <summary>
/// RFC 7807 Problem Details standardına uyumlu hata yanıtı.
/// .NET 9.0'un record özellikleriyle geliştirilmiş versiyon.
/// </summary>
public sealed record UnifiedApiErrorResponse
{
    public string? ErrorId { get; init; }
    public bool Success { get; init; } = false;
    public required int StatusCode { get; init; }
    public string? Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? ErrorType { get; init; }
    public string? Detail { get; init; }
    public string? Instance { get; init; }
    public object? AdditionalData { get; init; }

    // Immutable (değişmez) yapı için private constructor
    private UnifiedApiErrorResponse() { }

    /// <summary>
    /// Factory method for creating error responses from exceptions
    /// </summary>
    public static UnifiedApiErrorResponse FromException(CustomException ex)
    {
        return new UnifiedApiErrorResponse
        {
            // Artık doğrudan StatusCode yerine ExplicitStatusCode kullanıyoruz.
            // Eğer ExplicitStatusCode null ise 500 (Internal Server Error) olarak ayarlanıyor.
            StatusCode = ex.ExplicitStatusCode ?? StatusCodes.Status500InternalServerError,
            Title = ex.GetType().Name,
            Message = ex.Message,
            ErrorType = ex.GetType().Name,
            Detail = ex.Message,
            AdditionalData = ex.AdditionalData
        };
    }

    public UnifiedApiErrorResponse WithAdditionalData(object data)
    {
        return this with { AdditionalData = data };
    }

    /// <summary>
    /// Validation hataları için özel factory
    /// </summary>
    public static UnifiedApiErrorResponse FromValidationException(ValidationException ex, string errorId, string? detail = null)
    {
        return new UnifiedApiErrorResponse
        {
            ErrorId = errorId,
            StatusCode = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Message = "Validation error",
            ErrorType = nameof(ValidationException),
            Detail = detail ?? "Validation failed",
            AdditionalData = ex.Errors
        };
    }

    /// <summary>
    /// .NET 9.0 için With metodu özelleştirmesi
    /// </summary>
    public UnifiedApiErrorResponse WithDetail(string detail)
    {
        return this with { Detail = detail };
    }

    public static UnifiedApiErrorResponse CreateInternalServerError(string errorId, string? detail = null)
    {
        return new UnifiedApiErrorResponse
        {
            ErrorId = errorId,
            StatusCode = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Message = "An unexpected error occurred",
            ErrorType = "InternalServerError",
            Detail = detail ?? "See logs for details",
            AdditionalData = null
        };
    }
}
