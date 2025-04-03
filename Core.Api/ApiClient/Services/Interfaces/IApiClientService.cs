using Core.Api.ApiClient.Models;

namespace Core.Api.ApiClient.Services.Interfaces;

public interface IApiClientService
{
    Task<ApiResponseWrapper<T>> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
    Task<ApiResponseWrapper<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default);
    Task<ApiResponseWrapper<TResponse>> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default);
    Task<ApiResponseWrapper<TResponse>> PatchAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default);
    Task<ApiResponseWrapper<bool>> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);

    Task<IEnumerable<ApiResponseWrapper<T>>> GetMultipleAsync<T>(IEnumerable<string> requestUris, CancellationToken cancellationToken = default);
}