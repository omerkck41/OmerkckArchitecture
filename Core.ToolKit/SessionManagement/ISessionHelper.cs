using Microsoft.AspNetCore.Http;

namespace Core.ToolKit.SessionManagement;

public interface ISessionHelper
{
    Task SetAsync<T>(ISession session, string key, T value);
    Task<T?> GetAsync<T>(ISession session, string key);
    Task RemoveAsync(ISession session, string key);
    Task ClearAsync(ISession session);
    Task<bool> ExistsAsync(ISession session, string key);
}