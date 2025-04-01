using Microsoft.AspNetCore.Http;

namespace Core.Security.Headers;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            AddSecurityHeaders(context);
            await _next(context);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers.ContentSecurityPolicy = "default-src 'self'; script-src 'self'; style-src 'self';";
        headers["Referrer-Policy"] = "no-referrer";
        headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains";
    }
}