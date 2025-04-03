using Core.Api.ApiClient.Models;
using Core.Api.ApiClient.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json;

namespace Core.Api.ApiClient.Services.Implementations;

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // IMemoryCache enjeksiyonu caching mekanizması için eklendi.
    public ApiClientService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    private async Task<ApiResponseWrapper<T>> HandleResponseAsync<T>(HttpResponseMessage response, string requestUri, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return ApiResponseWrapper<T>.CreateErrorResponse("Request failed", (int)response.StatusCode, detail: errorContent, instance: requestUri);
        }

        var data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return ApiResponseWrapper<T>.CreateSuccessResponse(data!, "Success", (int)response.StatusCode, requestUri);
    }

    public async Task<ApiResponseWrapper<T>> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue(requestUri, out ApiResponseWrapper<T> cachedResponse))
                return cachedResponse;

            var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            var result = await HandleResponseAsync<T>(response, requestUri, cancellationToken);

            if (result.IsSuccessful)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                _cache.Set(requestUri, result, cacheOptions);
            }

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponseWrapper<T>.CreateErrorResponse("Unhandled exception", 500, ex.GetType().Name, ex.Message, instance: requestUri);
        }
    }

    public async Task<ApiResponseWrapper<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, data, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, requestUri, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponseWrapper<TResponse>.CreateErrorResponse("Unhandled exception", 500, ex.GetType().Name, ex.Message, instance: requestUri);
        }
    }

    public async Task<ApiResponseWrapper<TResponse>> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(requestUri, data, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, requestUri, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponseWrapper<TResponse>.CreateErrorResponse("Unhandled exception", 500, ex.GetType().Name, ex.Message, instance: requestUri);
        }
    }

    public async Task<ApiResponseWrapper<TResponse>> PatchAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = JsonContent.Create(data)
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, requestUri, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponseWrapper<TResponse>.CreateErrorResponse("Unhandled exception", 500, ex.GetType().Name, ex.Message, instance: requestUri);
        }
    }

    public async Task<ApiResponseWrapper<bool>> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
            return await HandleResponseAsync<bool>(response, requestUri, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponseWrapper<bool>.CreateErrorResponse("Unhandled exception", 500, ex.GetType().Name, ex.Message, instance: requestUri);
        }
    }

    public async Task<IEnumerable<ApiResponseWrapper<T>>> GetMultipleAsync<T>(IEnumerable<string> requestUris, CancellationToken cancellationToken = default)
    {
        var tasks = requestUris.Select(uri => GetAsync<T>(uri, cancellationToken));
        return await Task.WhenAll(tasks);
    }
}