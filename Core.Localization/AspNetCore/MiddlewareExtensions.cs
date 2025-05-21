using Core.Localization.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Core.Localization.AspNetCore;

/// <summary>
/// ASP.NET Core için NewLocalization Middleware uzantıları
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// NewLocalization'ı kullanarak RequestLocalization middleware yapılandırması ekler
    /// </summary>
    public static IApplicationBuilder UseNewLocalization(this IApplicationBuilder app)
    {
        var localizationOptions = app.ApplicationServices
            .GetRequiredService<IOptions<LocalizationOptions>>()
            .Value;

        var requestLocalizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(localizationOptions.DefaultCulture),
            SupportedCultures = localizationOptions.SupportedCultures.ToList(),
            SupportedUICultures = localizationOptions.SupportedCultures.ToList()
        };

        // Kültür providers ayarları
        requestLocalizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
        {
            new QueryStringRequestCultureProvider(),
            new CookieRequestCultureProvider(),
            new AcceptLanguageHeaderRequestCultureProvider()
        };

        app.UseRequestLocalization(requestLocalizationOptions);

        return app;
    }

    /// <summary>
    /// URL path'te kültür bilgisine göre lokalizasyon için middleware ekler
    /// Örnek: /en-US/products, /tr-TR/products
    /// </summary>
    public static IApplicationBuilder UseRouteCultureMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<RouteCultureMiddleware>();
        return app;
    }
}

/// <summary>
/// URL path'te kültür bilgisine göre kültürü değiştiren middleware
/// </summary>
public class RouteCultureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<LocalizationOptions> _options;

    public RouteCultureMiddleware(RequestDelegate next, IOptions<LocalizationOptions> options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (path != null && path.Length > 1)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length > 0)
            {
                var potentialCulture = segments[0];

                try
                {
                    var culture = CultureInfo.GetCultureInfo(potentialCulture);

                    // Desteklenen bir kültür mü kontrol et
                    if (_options.Value.SupportedCultures.Any(c => c.Name == culture.Name))
                    {
                        // Kültürü ayarla
                        CultureInfo.CurrentCulture = culture;
                        CultureInfo.CurrentUICulture = culture;

                        // Path'ten kültür segmentini çıkart
                        var newPath = "/" + string.Join('/', segments.Skip(1));
                        context.Request.Path = new PathString(newPath);
                    }
                }
                catch (CultureNotFoundException)
                {
                    // Geçerli bir kültür değilse, path'i değiştirme
                }
            }
        }

        await _next(context);
    }
}
