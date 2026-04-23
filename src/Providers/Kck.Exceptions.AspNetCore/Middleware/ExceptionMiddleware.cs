using System.Text.Json;
using Kck.Exceptions.AspNetCore.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kck.Exceptions.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core exception middleware.
/// Pipeline'daki hataları yakalar, uygun handler ile RFC 7807 uyumlu JSON yanıtı oluşturur.
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ExceptionHandlerFactory _handlerFactory;
    private readonly ILogger<ExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ExceptionMiddleware(
        RequestDelegate next,
        ExceptionHandlerFactory handlerFactory,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _handlerFactory = handlerFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Exception caught by ExceptionMiddleware. TraceId: {TraceId}", context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot write error response. TraceId: {TraceId}", context.TraceIdentifier);
                throw;
            }

            var handler = _handlerFactory.GetHandler(ex);
            var errorResponse = handler.Handle(ex, context);

            context.Response.Clear();
            context.Response.StatusCode = errorResponse.Status;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(errorResponse, JsonOptions),
                context.RequestAborted);
        }
    }
}
