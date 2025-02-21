using Core.Api.Security.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;

namespace Core.Api.Security.Middleware;

public class AntiForgeryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _enableCsrfProtection;
    private readonly List<string> _excludedEndpoints;

    public AntiForgeryMiddleware(RequestDelegate next, IOptions<SecuritySettings> securitySettings)
    {
        _next = next;
        _enableCsrfProtection = securitySettings.Value.EnableCsrfProtection;
        _excludedEndpoints = securitySettings.Value.CsrfExcludedEndpoints ?? new List<string>();
    }

    public async Task Invoke(HttpContext context)
    {
        // Eğer CSRF koruması kapalıysa veya bu endpoint muaf tutulmuşsa CSRF kontrolü yapmadan devam et
        if (!_enableCsrfProtection || _excludedEndpoints.Contains(context.Request.Path.Value))
        {
            await _next(context);
            return;
        }

        // Controller'daki metotta [IgnoreCsrf] attribute'u varsa, CSRF kontrolü yapmadan devam et
        var endpoint = context.GetEndpoint();
        var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (controllerActionDescriptor?.MethodInfo.GetCustomAttributes(typeof(IgnoreCsrfAttribute), false).Length > 0)
        {
            await _next(context);
            return;
        }

        // POST, PUT, DELETE gibi isteklerde CSRF token kontrolü yap
        if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "DELETE")
        {
            // Cookie'den CSRF token'ı oku
            var csrfTokenFromCookie = context.Request.Cookies["X-CSRF-TOKEN"];

            // Eğer Header'dan gelmiyorsa, sadece Cookie'den kontrol et
            if (string.IsNullOrEmpty(csrfTokenFromCookie))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CSRF token is required.");
                return;
            }
        }

        await _next(context);
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class IgnoreCsrfAttribute : Attribute
{
}