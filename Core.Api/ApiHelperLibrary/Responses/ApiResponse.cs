namespace Core.Api.ApiHelperLibrary.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public T Data { get; set; } = default!;
    public int StatusCode { get; set; }
    public string LocationHeader { get; set; } = default!;
}