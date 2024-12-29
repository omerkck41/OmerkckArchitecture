using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.ToolKit.SessionManagement;

public static class SessionHelper
{
    /// <summary>
    /// Sets a key-value pair in the session storage as JSON.
    /// </summary>
    /// <typeparam name="T">Type of the value to store.</typeparam>
    /// <param name="session">Current session object.</param>
    /// <param name="key">Key for the session entry.</param>
    /// <param name="value">Value to store.</param>
    public static void Set<T>(this ISession session, string key, T value)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var jsonValue = JsonSerializer.Serialize(value);
        session.SetString(key, jsonValue);
    }

    /// <summary>
    /// Gets a value from the session storage as a specific type.
    /// </summary>
    /// <typeparam name="T">Type of the value to retrieve.</typeparam>
    /// <param name="session">Current session object.</param>
    /// <param name="key">Key for the session entry.</param>
    /// <returns>Deserialized object of type T or default if not found.</returns>
    public static T? Get<T>(this ISession session, string key)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var jsonValue = session.GetString(key);
        return jsonValue == null ? default : JsonSerializer.Deserialize<T>(jsonValue);
    }

    /// <summary>
    /// Removes a specific key-value pair from the session storage.
    /// </summary>
    /// <param name="session">Current session object.</param>
    /// <param name="key">Key for the session entry to remove.</param>
    public static void Remove(this ISession session, string key)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        session.Remove(key);
    }

    /// <summary>
    /// Clears all entries from the session storage.
    /// </summary>
    /// <param name="session">Current session object.</param>
    public static void Clear(this ISession session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        session.Clear();
    }

    /// <summary>
    /// Checks if a specific key exists in the session storage.
    /// </summary>
    /// <param name="session">Current session object.</param>
    /// <param name="key">Key to check for existence.</param>
    /// <returns>True if the key exists, otherwise false.</returns>
    public static bool Exists(this ISession session, string key)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        return session.GetString(key) != null;
    }
}