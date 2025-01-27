using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.ToolKit.SessionManagement;

public class SessionHelper : ISessionHelper
{
    public async Task SetAsync<T>(ISession session, string key, T value)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var jsonValue = JsonSerializer.Serialize(value);
        session.SetString(key, jsonValue);
        await Task.CompletedTask;
    }

    public async Task<T?> GetAsync<T>(ISession session, string key)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var jsonValue = session.GetString(key);
        return await Task.FromResult(jsonValue == null ? default : JsonSerializer.Deserialize<T>(jsonValue));
    }

    public async Task RemoveAsync(ISession session, string key)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        session.Remove(key);
        await Task.CompletedTask;
    }

    public async Task ClearAsync(ISession session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        session.Clear();
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(ISession session, string key)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        return await Task.FromResult(session.GetString(key) != null);
    }
}