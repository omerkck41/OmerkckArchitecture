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

        headers.Add("X-Content-Type-Options", "nosniff");
        headers.Add("X-Frame-Options", "DENY");
        headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self';");
        headers.Add("Referrer-Policy", "no-referrer");
        headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
}