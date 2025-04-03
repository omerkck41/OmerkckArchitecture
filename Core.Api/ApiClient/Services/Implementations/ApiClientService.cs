using Core.Api.ApiClient.Exceptions;
using Core.Api.ApiClient.Models;
using Core.Api.ApiClient.Services.Interfaces;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
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

    private static async Task<ApiResponseWrapper<T>> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            // 401 ise UnauthorizedException fırlat
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var unauthorizedContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new UnAuthorizedException($"Unauthorized: {unauthorizedContent}");
            }

            // ReadFromJsonAsync ile doğrudan stream üzerinden deserialize işlemi yapılır.
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<T>>(_jsonOptions, cancellationToken);

            if (apiResponse == null)
                throw new ApiException("API response deserialization failed.");

            if (!apiResponse.IsSuccessful)
                throw new ApiException(apiResponse.Message, apiResponse.StatusCode, apiResponse.ErrorType, apiResponse.Detail, apiResponse.AdditionalData);

            return apiResponse;
        }
        catch (Exception ex)
        {
            // Hata durumunda response içeriğini tekrar string olarak okuyarak detaylı mesaj döndürür.
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiException($"Failed to process response: {ex.Message} - {responseContent}", ex);
        }
    }

    public async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        // Cache kontrolü: Aynı URI için daha önce alınan sonuç varsa direkt döner.
        if (_cache.TryGetValue(requestUri, out T? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            var apiResponse = await HandleResponseAsync<T>(response, cancellationToken);
            var data = apiResponse.Data!;

            // Cache'e ekleme: 60 saniyelik süre ile.
            _cache.Set(requestUri, data, TimeSpan.FromSeconds(60));
            return data;
        }
        catch (HttpRequestException httpEx)
        {
            throw new ApiException($"GET {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            throw new ApiException($"GET {requestUri} failed: {ex.Message}", ex);
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, data, cancellationToken);

            // Eğer 401 dönüyorsa, UnauthorizedException fırlatıyoruz
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                // İçerikte dönen hata mesajını ekliyoruz
                throw new UnAuthorizedException($"POST {requestUri} failed: {content}");
            }

            // 401 haricindeki hatalar için yine HandleResponseAsync kullanılabilir
            var apiResponse = await HandleResponseAsync<TResponse>(response, cancellationToken);
            return apiResponse.Data!;
        }
        catch (HttpRequestException httpEx)
        {
            throw new ApiException($"POST {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            throw new ApiException($"POST {requestUri} failed: {ex.Message}", ex);
        }
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(requestUri, data, cancellationToken);
            return (await HandleResponseAsync<TResponse>(response, cancellationToken)).Data!;
        }
        catch (HttpRequestException httpEx)
        {
            throw new ApiException($"PUT {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            throw new ApiException($"PUT {requestUri} failed: {ex.Message}", ex);
        }
    }

    public async Task<TResponse> PatchAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync(requestUri, data, cancellationToken);
            return (await HandleResponseAsync<TResponse>(response, cancellationToken)).Data!;
        }
        catch (HttpRequestException httpEx)
        {
            throw new ApiException($"PATCH {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            throw new ApiException($"PATCH {requestUri} failed: {ex.Message}", ex);
        }
    }

    public async Task DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
            await HandleResponseAsync<object>(response, cancellationToken);
        }
        catch (HttpRequestException httpEx)
        {
            throw new ApiException($"DELETE {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            throw new ApiException($"DELETE {requestUri} failed: {ex.Message}", ex);
        }
    }

    // Birden fazla GET isteğini paralel olarak yönetmek için method.
    public async Task<IEnumerable<T>> GetMultipleAsync<T>(IEnumerable<string> requestUris, CancellationToken cancellationToken = default)
    {
        var tasks = requestUris.Select(uri => GetAsync<T>(uri, cancellationToken));
        return await Task.WhenAll(tasks);
    }
}