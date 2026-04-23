namespace Kck.Http.Abstractions;

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResponse<T> Success(T data, int statusCode = 200) =>
        new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    public static ApiResponse<T> Failure(string error, int statusCode) =>
        new() { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };
}
