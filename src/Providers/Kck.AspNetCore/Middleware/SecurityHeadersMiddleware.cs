using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Kck.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core middleware that appends security-related HTTP response headers
/// (X-Frame-Options, CSP, HSTS, etc.) to every outgoing response.
/// </summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    /// <summary>
    /// Adds security headers to the HTTP response and invokes the next middleware in the pipeline.
    /// HSTS is only added when the request is HTTPS and the application is running in Production.
    /// </summary>
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
