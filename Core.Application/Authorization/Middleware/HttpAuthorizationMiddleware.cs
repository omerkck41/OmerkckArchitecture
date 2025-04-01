using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.Application.Authorization.Middleware;

public class HttpAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public HttpAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Endpoint metadata bilgisini alıyoruz
        var endpoint = context.GetEndpoint();

        // AllowAnonymous kontrolü
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        // Authentication kontrolü
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Authentication required"
            }));

            return;
        }

        await _next(context);
    }
}