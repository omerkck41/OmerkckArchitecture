using Core.Api.ApiClient.Exceptions;
using Core.Api.ApiClient.Models;
using Core.Api.ApiClient.Services.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace Core.Api.ApiClient.Services.Implementations;

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _httpClient;

    public ApiClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static async Task<ApiResponseWrapper<T>> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true // camelCase ile PascalCase'i eşleştir
            };

            var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<T>>(responseContent, options);

            if (apiResponse == null)
                throw new ApiException("API response deserialization failed.");

            if (!apiResponse.IsSuccessful)
                throw new ApiException(apiResponse.Message, apiResponse.StatusCode, apiResponse.ErrorType, apiResponse.Detail, apiResponse.AdditionalData);

            return apiResponse;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Failed to process response: {ex.Message} - {responseContent}", ex);
        }
    }

    public async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            return (await HandleResponseAsync<T>(response, cancellationToken)).Data!;
        }
        catch (HttpRequestException httpEx)
        {
            // HttpRequestException için özel işlemler
            throw new ApiException($"GET {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            // Diğer tüm hatalar için genel işlemler
            throw new ApiException($"GET {requestUri} failed: {ex.Message}", ex);
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, data, cancellationToken);
            return (await HandleResponseAsync<TResponse>(response, cancellationToken)).Data!;
        }
        catch (HttpRequestException httpEx)
        {
            // HttpRequestException için özel işlemler
            throw new ApiException($"POST {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            // Diğer tüm hatalar için genel işlemler
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
            // HttpRequestException için özel işlemler
            throw new ApiException($"PUT {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            // Diğer tüm hatalar için genel işlemler
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
            // HttpRequestException için özel işlemler
            throw new ApiException($"PATCH {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            // Diğer tüm hatalar için genel işlemler
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
            // HttpRequestException için özel işlemler
            throw new ApiException($"DELETE {requestUri} failed due to a network error: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            // Diğer tüm hatalar için genel işlemler
            throw new ApiException($"DELETE {requestUri} failed: {ex.Message}", ex);
        }
    }
}