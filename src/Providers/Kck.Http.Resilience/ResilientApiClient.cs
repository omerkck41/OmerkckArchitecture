using System.Net.Http.Json;
using Kck.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.Http.Resilience;

public sealed partial class ResilientApiClient(
    HttpClient httpClient,
    ILogger<ResilientApiClient> logger) : IApiClient
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "HTTP {Method} {Url} returned {StatusCode}: {Error}")]
    private static partial void LogHttpFailure(ILogger logger, HttpMethod method, string url, int statusCode, string error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "HTTP {Method} {Url} timed out")]
    private static partial void LogHttpTimeout(ILogger logger, HttpMethod method, string url);

    public Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Get, url, ct: ct);

    public Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Post, url, body, ct: ct);

    public Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Put, url, body, ct: ct);

    public Task<ApiResponse<T>> DeleteAsync<T>(string url, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Delete, url, ct: ct);

    public async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body = null,
        IDictionary<string, string>? headers = null, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);

            if (body is not null)
                request.Content = JsonContent.Create(body);

            if (headers is not null)
            {
                foreach (var (key, value) in headers)
                    request.Headers.TryAddWithoutValidation(key, value);
            }

            var response = await httpClient.SendAsync(request, ct).ConfigureAwait(false);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>(ct).ConfigureAwait(false);
                return ApiResponse<T>.Success(data!, statusCode);
            }

            var error = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            LogHttpFailure(logger, method, url, statusCode, error);
            return ApiResponse<T>.Failure(error, statusCode);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            LogHttpTimeout(logger, method, url);
            return ApiResponse<T>.Failure("Request timed out", 408);
        }
    }
}
