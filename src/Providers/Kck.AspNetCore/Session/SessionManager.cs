using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Session;

internal sealed class SessionManager : ISessionManager
{
    public void Set<T>(ISession session, string key, T value)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public T? Get<T>(ISession session, string key)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var json = session.GetString(key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public void Remove(ISession session, string key)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        session.Remove(key);
    }

    public void Clear(ISession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.Clear();
    }

    public bool Exists(ISession session, string key)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return session.GetString(key) is not null;
    }
}
