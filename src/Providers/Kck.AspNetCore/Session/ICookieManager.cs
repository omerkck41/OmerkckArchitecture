using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Session;

public interface ICookieManager
{
    void Set(HttpResponse response, string key, string value, CookieOptions? options = null);
    string? Get(HttpRequest request, string key);
    void Remove(HttpResponse response, string key);
    CookieOptions GetDefaultOptions();
}
