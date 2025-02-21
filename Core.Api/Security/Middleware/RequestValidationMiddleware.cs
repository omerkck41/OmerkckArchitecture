using Microsoft.AspNetCore.Http;

namespace Core.Api.Security.Middleware;

public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    public RequestValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.ToString().Contains("SELECT") || context.Request.Path.ToString().Contains("DROP"))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid request detected.");
            return;
        }
        await _next(context);
    }
}