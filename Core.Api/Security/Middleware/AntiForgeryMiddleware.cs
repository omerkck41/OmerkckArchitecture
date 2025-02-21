using Microsoft.AspNetCore.Http;

namespace Core.Api.Security.Middleware;

public class AntiForgeryMiddleware
{
    private readonly RequestDelegate _next;
    public AntiForgeryMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
        {
            if (!context.Request.Headers.ContainsKey("X-CSRF-Token"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CSRF token is required.");
                return;
            }
        }
        await _next(context);
    }
}