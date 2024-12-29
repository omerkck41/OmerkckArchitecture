using Microsoft.AspNetCore.Http;

namespace Core.ToolKit.SessionManagement;

public static class CookieHelper
{
    /// <summary>
    /// Sets a cookie with specified options.
    /// </summary>
    /// <param name="response">HttpResponse object to set the cookie on.</param>
    /// <param name="key">Key for the cookie.</param>
    /// <param name="value">Value to store in the cookie.</param>
    /// <param name="options">Options to configure the cookie behavior.</param>
    public static void Set(HttpResponse response, string key, string value, CookieOptions options)
    {
        if (response == null) throw new ArgumentNullException(nameof(response));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        response.Cookies.Append(key, value, options);
    }

    /// <summary>
    /// Retrieves the value of a specific cookie.
    /// </summary>
    /// <param name="request">HttpRequest object to retrieve the cookie from.</param>
    /// <param name="key">Key for the cookie to retrieve.</param>
    /// <returns>Value of the cookie or null if not found.</returns>
    public static string? Get(HttpRequest request, string key)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        return request.Cookies[key];
    }

    /// <summary>
    /// Deletes a specific cookie.
    /// </summary>
    /// <param name="response">HttpResponse object to delete the cookie from.</param>
    /// <param name="key">Key for the cookie to delete.</param>
    public static void Remove(HttpResponse response, string key)
    {
        if (response == null) throw new ArgumentNullException(nameof(response));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        response.Cookies.Delete(key);
    }
}