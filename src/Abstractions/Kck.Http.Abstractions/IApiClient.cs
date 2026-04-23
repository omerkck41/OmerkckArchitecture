namespace Kck.Http.Abstractions;

public interface IApiClient
{
    Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default);
    Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null, CancellationToken ct = default);
    Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null, CancellationToken ct = default);
    Task<ApiResponse<T>> DeleteAsync<T>(string url, CancellationToken ct = default);
    Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body = null,
        IDictionary<string, string>? headers = null, CancellationToken ct = default);
}
