using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Session;

internal sealed class CookieManager : ICookieManager
{
    public void Set(HttpResponse response, string key, string value, CookieOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrEmpty(key);

        response.Cookies.Append(key, value, options ?? GetDefaultOptions());
    }

    public string? Get(HttpRequest request, string key)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return request.Cookies[key];
    }

    public void Remove(HttpResponse response, string key)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrEmpty(key);

        response.Cookies.Delete(key);
    }

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
