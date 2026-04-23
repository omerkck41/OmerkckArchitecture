using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Kck.AspNetCore.Middleware;

internal sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.XFrameOptions = "DENY";
        headers.XXSSProtection = "0";
        headers.XContentTypeOptions = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Content-Security-Policy"] = "default-src 'self'";

        if (context.Request.IsHttps && env.IsProduction())
        {
            headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains; preload";
        }

        await next(context);
    }
}
