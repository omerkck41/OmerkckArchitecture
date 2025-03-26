namespace Core.Api.ApiControllerBase.Responses;

/// <summary>
/// API yanıtlarının standart formatı.
/// </summary>
public record ApiResponse<T>(
    bool Success,
    string Message,
    T Data,
    int StatusCode,
    string LocationHeader
);