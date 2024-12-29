using Core.Integration.Models;

namespace Core.Integration.Interfaces;

public interface IApiClient
{
    Task<ApiResponse<T>> SendRequestAsync<T>(ApiRequest request);
}