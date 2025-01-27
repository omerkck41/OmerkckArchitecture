using Microsoft.AspNetCore.Http;

namespace Core.ToolKit.SessionManagement;

public interface ICookieHelper
{
    Task SetAsync(HttpResponse response, string key, string value, CookieOptions options);
    Task<string?> GetAsync(HttpRequest request, string key);
    Task RemoveAsync(HttpResponse response, string key);
    CookieOptions GetDefaultCookieOptions();
}