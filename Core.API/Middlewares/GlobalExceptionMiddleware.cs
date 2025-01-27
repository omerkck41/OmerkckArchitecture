using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var errorResponse = new
            {
                success = false,
                message = "An error occurred while processing your request.",
                detail = ex.Message // Hata detayını ekledik
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}