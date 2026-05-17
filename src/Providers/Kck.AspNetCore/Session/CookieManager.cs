using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Session;

/// <summary>
/// Default implementation of <see cref="ICookieManager"/> that enforces secure cookie defaults
/// (HttpOnly, Secure, SameSite=Strict, 7-day absolute expiry).
/// </summary>
internal sealed class CookieManager : ICookieManager
{
    /// <inheritdoc/>
    public void Set(HttpResponse response, string key, string value, CookieOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrEmpty(key);

        response.Cookies.Append(key, value, options ?? GetDefaultOptions());
    }

    /// <inheritdoc/>
    public string? Get(HttpRequest request, string key)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return request.Cookies[key];
    }

    /// <inheritdoc/>
    public void Remove(HttpResponse response, string key)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrEmpty(key);

        response.Cookies.Delete(key);
    }

    /// <inheritdoc/>
    public CookieOptions GetDefaultOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };
    }
}
