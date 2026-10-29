using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Kck.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Http.Resilience;

/// <summary>
/// Implementation of <see cref="IApiClient"/> that wraps <see cref="HttpClient"/> with
/// structured error logging and timeout handling, returning typed <see cref="ApiResponse{T}"/>
/// results instead of throwing on non-success status codes.
/// </summary>
public sealed partial class ResilientApiClient : IApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<ResilientApiClient> logger;
    private readonly JsonSerializerOptions? _serializerOptions;

    /// <summary>Creates the client with the web JSON defaults.</summary>
    public ResilientApiClient(HttpClient httpClient, ILogger<ResilientApiClient> logger)
        : this(httpClient, logger, Options.Create(new ResilienceOptions()))
    {
    }

    /// <summary>Creates the client with <see cref="ResilienceOptions.SerializerOptions"/>.</summary>
    [ActivatorUtilitiesConstructor] // the typed-client factory would see two candidate constructors
    public ResilientApiClient(HttpClient httpClient, ILogger<ResilientApiClient> logger, IOptions<ResilienceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.httpClient = httpClient;
        this.logger = logger;
        _serializerOptions = options.Value.SerializerOptions;
    }

    // The trimmer treats IsReflectionEnabledByDefault as a constant: false under Native AOT, so the reflection-based web
    // defaults are never reached there and the caller gets a clear error instead.
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Guarded by JsonSerializer.IsReflectionEnabledByDefault.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Guarded by JsonSerializer.IsReflectionEnabledByDefault.")]
    private static JsonSerializerOptions DefaultSerializerOptions() =>
        JsonSerializer.IsReflectionEnabledByDefault
            ? JsonSerializerOptions.Web
            : throw new InvalidOperationException(
                "Reflection-based JSON serialization is disabled (Native AOT or trimming). Set " +
                "ResilienceOptions.SerializerOptions through AddKckHttpResilience(o => ...) to options whose " +
                "TypeInfoResolver is a source-generated JsonSerializerContext covering the body and response types.");

    [LoggerMessage(Level = LogLevel.Warning, Message = "HTTP {Method} {Url} returned {StatusCode}: {Error}")]
    private static partial void LogHttpFailure(ILogger logger, HttpMethod method, string url, int statusCode, string error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "HTTP {Method} {Url} timed out")]
    private static partial void LogHttpTimeout(ILogger logger, HttpMethod method, string url);

    /// <summary>Sends a GET request to <paramref name="url"/> and deserialises the response body as <typeparamref name="T"/>.</summary>
    public Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Get, url, ct: ct);

    /// <summary>Sends a POST request to <paramref name="url"/> with an optional JSON <paramref name="body"/>.</summary>
    public Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Post, url, body, ct: ct);

    /// <summary>Sends a PUT request to <paramref name="url"/> with an optional JSON <paramref name="body"/>.</summary>
    public Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Put, url, body, ct: ct);

    /// <summary>Sends a DELETE request to <paramref name="url"/> and deserialises the response body as <typeparamref name="T"/>.</summary>
    public Task<ApiResponse<T>> DeleteAsync<T>(string url, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Delete, url, ct: ct);

    /// <summary>
    /// Builds and sends an HTTP request using the given <paramref name="method"/>, <paramref name="url"/>,
    /// optional <paramref name="body"/> and custom <paramref name="headers"/>.  Returns a failure
    /// response (408) on timeout instead of propagating the exception.
    /// </summary>
    public async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body = null,
        IDictionary<string, string>? headers = null, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);

            var serializerOptions = _serializerOptions ?? DefaultSerializerOptions();
            if (body is not null)
                request.Content = JsonContent.Create(body, serializerOptions.GetTypeInfo(body.GetType()));

            if (headers is not null)
            {
                foreach (var (key, value) in headers)
                    request.Headers.TryAddWithoutValidation(key, value);
            }

            var response = await httpClient.SendAsync(request, ct).ConfigureAwait(false);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var typeInfo = (JsonTypeInfo<T>)serializerOptions.GetTypeInfo(typeof(T));
                var data = await response.Content.ReadFromJsonAsync(typeInfo, ct).ConfigureAwait(false);
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
