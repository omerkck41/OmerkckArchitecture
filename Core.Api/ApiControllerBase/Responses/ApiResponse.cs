namespace Core.Api.ApiControllerBase.Responses;

/// <summary>
/// API çağrılarında standart yanıt formatı.
/// </summary>
public record ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public string Message { get; init; } = string.Empty;
    public T? AdditionalData { get; init; }
    public int StatusCode { get; init; }
    public string? Instance { get; init; }

    // Opsiyonel hata detayları
    public string? ErrorType { get; init; }
    public string? Detail { get; init; }
    public string? ErrorId { get; init; }
}