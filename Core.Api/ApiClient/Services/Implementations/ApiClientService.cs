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

    private static async Task<ApiResponseWrapper<T>> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponseWrapper<T>>(responseContent);
            if (apiResponse == null)
                throw new ApiException("API response deserialization failed.");

            if (!apiResponse.IsSuccessful)
                throw new ApiException(apiResponse.Message);

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
            return (await HandleResponseAsync<TResponse>(response, cancellationToken)).Data!;
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
        catch (Exception ex)
        {
            throw new ApiException($"DELETE {requestUri} failed: {ex.Message}", ex);
        }
    }
}