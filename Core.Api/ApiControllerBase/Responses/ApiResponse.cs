namespace Core.Api.ApiControllerBase.Responses;

/// <summary>
/// API yanıtlarının standart formatı.
/// </summary>
public record ApiResponse<T>(
    bool Success,
    string Message,
    T AdditionalData,
    int StatusCode,
    string Instance
);