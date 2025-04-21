using Core.Localization.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Core.Localization.Extensions;

public class HttpContextCultureProvider : ICultureProvider
{
    private readonly IHttpContextAccessor _httpContext;

    public HttpContextCultureProvider(IHttpContextAccessor httpContext)
        => _httpContext = httpContext;

    public string GetRequestCulture()
    {
        var ctx = _httpContext.HttpContext!;
        // 1) QueryString “?culture=tr-TR”
        if (ctx.Request.Query.TryGetValue("culture", out var qs))
            return qs!;
        // 2) Cookie “.AspNetCore.Culture”
        var cookie = ctx.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
        if (cookie is not null)
        {
            var parsed = CookieRequestCultureProvider.ParseCookieValue(cookie);
            return parsed.Cultures.First().Value;
        }
        // 3) Accept‑Language başlığı
        var header = ctx.Request.Headers["Accept-Language"].ToString();
        return header.Split(',').FirstOrDefault() ?? "tr-TR";
    }
}
