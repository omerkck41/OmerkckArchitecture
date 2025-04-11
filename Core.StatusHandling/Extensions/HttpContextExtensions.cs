using Microsoft.AspNetCore.Http;

namespace Core.StatusHandling.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Belirtilen mesaj ve bildirim tipi ile response'a cookie ekler.
    /// </summary>
    public static void AppendNotification(this HttpContext context, string message, string type)
    {
        context.Response.Cookies.Append("X-Notification-Message", message, new CookieOptions
        {
            Path = "/",
            HttpOnly = false,
            SameSite = SameSiteMode.Lax
        });
        context.Response.Cookies.Append("X-Notification-Type", type, new CookieOptions
        {
            Path = "/",
            HttpOnly = false,
            SameSite = SameSiteMode.Lax
        });
    }
}