using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Session;

/// <summary>
/// Defines operations for reading, writing and removing HTTP cookies with consistent
/// security defaults (HttpOnly, Secure, SameSite).
/// </summary>
public interface ICookieManager
{
    /// <summary>
    /// Appends a cookie with <paramref name="key"/> and <paramref name="value"/> to
    /// <paramref name="response"/>, using <paramref name="options"/> or the secure defaults.
    /// </summary>
    void Set(HttpResponse response, string key, string value, CookieOptions? options = null); // NOSONAR S1700 - cookie API convention; VB.NET callers can use fully-qualified name

    /// <summary>
    /// Returns the value of the cookie identified by <paramref name="key"/>, or
    /// <see langword="null"/> when the cookie is absent.
    /// </summary>
    string? Get(HttpRequest request, string key); // NOSONAR S1700 - cookie API convention

    /// <summary>
    /// Deletes the cookie identified by <paramref name="key"/> from <paramref name="response"/>.
    /// </summary>
    void Remove(HttpResponse response, string key);

    /// <summary>
    /// Returns the default <see cref="CookieOptions"/> used when no explicit options are provided
    /// (HttpOnly, Secure, SameSite=Strict, 7-day expiry).
    /// </summary>
    CookieOptions GetDefaultOptions();
}
