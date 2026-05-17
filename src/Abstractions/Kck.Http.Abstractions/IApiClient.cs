namespace Kck.Http.Abstractions;

/// <summary>
/// Typed HTTP client abstraction that wraps GET, POST, PUT, DELETE, and custom-method calls into strongly-typed <see cref="ApiResponse{T}"/> results.
/// </summary>
public interface IApiClient
{
    /// <summary>Sends a GET request to <paramref name="url"/> and deserializes the response body as <typeparamref name="T"/>.</summary>
    Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default);

    /// <summary>Sends a POST request with an optional JSON <paramref name="body"/> and deserializes the response as <typeparamref name="T"/>.</summary>
    Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null, CancellationToken ct = default);

    /// <summary>Sends a PUT request with an optional JSON <paramref name="body"/> and deserializes the response as <typeparamref name="T"/>.</summary>
    Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null, CancellationToken ct = default);

    /// <summary>Sends a DELETE request to <paramref name="url"/> and deserializes the response as <typeparamref name="T"/>.</summary>
    Task<ApiResponse<T>> DeleteAsync<T>(string url, CancellationToken ct = default);

    /// <summary>Sends an HTTP request using the specified <paramref name="method"/>, URL, body, and additional headers.</summary>
    Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body = null,
        IDictionary<string, string>? headers = null, CancellationToken ct = default);
}
