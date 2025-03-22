namespace Core.Api.ApiClient.Models;

public class ApiResponseWrapper<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T Data { get; set; } = default!;
    public int StatusCode { get; set; }
    public string LocationHeader { get; set; } = string.Empty;
}