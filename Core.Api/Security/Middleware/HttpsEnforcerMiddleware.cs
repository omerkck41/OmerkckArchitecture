using Microsoft.AspNetCore.Http;

namespace Core.Api.Security.Middleware;

public class HttpsEnforcerMiddleware
{
    private readonly RequestDelegate _next;

    public HttpsEnforcerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.IsHttps)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("HTTPS is required.");
            return;
        }

        await _next(context);
    }
}