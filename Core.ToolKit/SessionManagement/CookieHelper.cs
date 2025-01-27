using Microsoft.AspNetCore.Http;

namespace Core.ToolKit.SessionManagement;

public class CookieHelper : ICookieHelper
{
    public async Task SetAsync(HttpResponse response, string key, string value, CookieOptions options)
    {
        if (response == null) throw new ArgumentNullException(nameof(response));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        response.Cookies.Append(key, value, options);
        await Task.CompletedTask;
    }

    public async Task<string?> GetAsync(HttpRequest request, string key)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        return await Task.FromResult(request.Cookies[key]);
    }

    public async Task RemoveAsync(HttpResponse response, string key)
    {
        if (response == null) throw new ArgumentNullException(nameof(response));
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        response.Cookies.Delete(key);
        await Task.CompletedTask;
    }

    public CookieOptions GetDefaultCookieOptions()
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