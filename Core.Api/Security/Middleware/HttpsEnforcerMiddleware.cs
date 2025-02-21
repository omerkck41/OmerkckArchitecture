using Core.Api.Security.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Core.Api.Security.Middleware;

public class HttpsEnforcerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _enforceHttps;

    public HttpsEnforcerMiddleware(RequestDelegate next, IOptions<SecuritySettings> securitySettings)
    {
        _next = next;
        _enforceHttps = securitySettings.Value.EnforceHttps;
    }

    public async Task Invoke(HttpContext context)
    {
        if (_enforceHttps && !context.Request.IsHttps)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("HTTPS is required.");
            return;
        }

        await _next(context);
    }
}