using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Session;

/// <summary>
/// JSON-backed implementation of <see cref="ISessionManager"/> that stores typed values in
/// an ASP.NET Core <see cref="ISession"/> using <see cref="JsonSerializer"/>.
/// </summary>
internal sealed class SessionManager : ISessionManager
{
    /// <inheritdoc/>
    public void Set<T>(ISession session, string key, T value)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        session.SetString(key, JsonSerializer.Serialize(value));
    }

    /// <inheritdoc/>
    public T? Get<T>(ISession session, string key)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var json = session.GetString(key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }

    /// <inheritdoc/>
    public void Remove(ISession session, string key)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        session.Remove(key);
    }

    /// <inheritdoc/>
    public void Clear(ISession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.Clear();
    }

    /// <inheritdoc/>
    public bool Exists(ISession session, string key)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return session.GetString(key) is not null;
    }
}
