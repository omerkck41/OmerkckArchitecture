namespace Core.Api.ApiControllerBase.Responses;

public record ApiResponse<T>(
        bool Success,
        string Message,
        T Data,
        int StatusCode,
        string LocationHeader
    );