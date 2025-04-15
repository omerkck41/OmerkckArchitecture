using Microsoft.AspNetCore.Http;

namespace Core.Localization.Extensions;

/// <summary>
/// HttpContext için uzantı metotları
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// HttpContext'ten dil tercihini alır
    /// </summary>
    /// <param name="context">HTTP bağlamı</param>
    /// <param name="supportedCultures">Desteklenen kültürler</param>
    /// <returns>Dil tercihi</returns>
    public static string GetPreferredLanguage(this HttpContext context, IEnumerable<string> supportedCultures)
    {
        // Cookie'den tercih edilen dili al
        var cookieLanguage = context.Request.Cookies["Localization.Culture"];
        if (!string.IsNullOrEmpty(cookieLanguage) && supportedCultures.Contains(cookieLanguage))
        {
            return cookieLanguage;
        }

        // Accept-Language header'dan tercih edilen dili al
        var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            var languages = acceptLanguage.Split(',')
                .Select(lang => lang.Split(';').First().Trim())
                .ToList();

            foreach (var language in languages)
            {
                // Tam eşleşme (tr-TR)
                if (supportedCultures.Contains(language))
                {
                    return language;
                }

                // Kısmi eşleşme (tr-TR -> tr)
                var shortCode = language.Split('-').First();
                var match = supportedCultures.FirstOrDefault(c => c.StartsWith($"{shortCode}-", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(match))
                {
                    return match;
                }
            }
        }

        // Varsayılan ilk dili döndür
        return supportedCultures.First();
    }

    /// <summary>
    /// Dil tercihini cookie olarak saklar
    /// </summary>
    /// <param name="context">HTTP bağlamı</param>
    /// <param name="culture">Kültür kodu</param>
    /// <param name="expireDays">Geçerlilik süresi (gün)</param>
    public static void SetLanguageCookie(this HttpContext context, string culture, int expireDays = 365)
    {
        context.Response.Cookies.Append("Localization.Culture", culture, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(expireDays),
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });
    }
}