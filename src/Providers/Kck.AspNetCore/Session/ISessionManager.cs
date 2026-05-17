using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Session;

/// <summary>
/// Defines type-safe operations for storing, retrieving and removing values from an
/// ASP.NET Core <see cref="ISession"/> using JSON serialisation.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Serialises <paramref name="value"/> as JSON and stores it in <paramref name="session"/>
    /// under <paramref name="key"/>.
    /// </summary>
    void Set<T>(ISession session, string key, T value);

    /// <summary>
    /// Retrieves and deserialises the value stored under <paramref name="key"/>, or returns
    /// the default for <typeparamref name="T"/> when the key is absent.
    /// </summary>
    T? Get<T>(ISession session, string key);

    /// <summary>Removes the entry identified by <paramref name="key"/> from <paramref name="session"/>.</summary>
    void Remove(ISession session, string key);

    /// <summary>Removes all entries from <paramref name="session"/>.</summary>
    void Clear(ISession session);

    /// <summary>
    /// Returns <see langword="true"/> when an entry for <paramref name="key"/> exists in
    /// <paramref name="session"/>; otherwise <see langword="false"/>.
    /// </summary>
    bool Exists(ISession session, string key);
}
