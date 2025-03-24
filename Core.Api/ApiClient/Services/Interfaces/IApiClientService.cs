namespace Core.Api.ApiClient.Services.Interfaces;

public interface IApiClientService
{
    Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
    Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default);
    Task<TResponse> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default);
    Task<TResponse> PatchAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default);
    Task DeleteAsync(string requestUri, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetMultipleAsync<T>(IEnumerable<string> requestUris, CancellationToken cancellationToken = default);
}