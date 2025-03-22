using Core.Api.ApiClient.Exceptions;
using Core.Api.ApiClient.Models;
using Core.Api.ApiClient.Services.Interfaces;
using System.Net.Http.Json;

namespace Core.Api.ApiClient.Services.Implementations;

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _httpClient;

    public ApiClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new ApiException($"GET {requestUri} failed: {response.StatusCode} - {errorMessage}");
            }
            return await ReadResponseContentAsync<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error in GET {requestUri}: {ex.Message}", ex);
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, data, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new ApiException($"POST {requestUri} failed: {response.StatusCode} - {errorMessage}");
            }
            return await ReadResponseContentAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error in POST {requestUri}: {ex.Message}", ex);
        }
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(requestUri, data, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new ApiException($"PUT {requestUri} failed: {response.StatusCode} - {errorMessage}");
            }
            return await ReadResponseContentAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error in PUT {requestUri}: {ex.Message}", ex);
        }
    }

    public async Task<TResponse> PatchAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync(requestUri, data, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new ApiException($"PATCH {requestUri} failed: {response.StatusCode} - {errorMessage}");
            }
            return await ReadResponseContentAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error in PATCH {requestUri}: {ex.Message}", ex);
        }
    }

    public async Task DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new ApiException($"DELETE {requestUri} failed: {response.StatusCode} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error in DELETE {requestUri}: {ex.Message}", ex);
        }
    }

    private async Task<T> ReadResponseContentAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<T>>(cancellationToken);
        if (wrapper == null)
            throw new ApiException("Null response wrapper received.");

        return wrapper.Data;
    }
}