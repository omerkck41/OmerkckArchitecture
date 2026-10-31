using System.Text.Json;
using Kck.Exceptions.AspNetCore.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kck.Exceptions.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core exception middleware.
/// Catches errors in the pipeline and builds an RFC 7807 compliant JSON response with the matching handler.
/// </summary>
public sealed partial class ExceptionMiddleware
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

    [LoggerMessage(Level = LogLevel.Error, Message = "Response has already started, cannot write error response. TraceId: {TraceId}")]
    private static partial void LogResponseAlreadyStarted(ILogger logger, Exception ex, string traceId);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        // Only a cancellation by the client of this request passes through; another one (an HttpClient timeout, a
        // canceled downstream call) is a failure of this request and gets a problem response.
        catch (Exception ex) when (ex is not OperationCanceledException || !context.RequestAborted.IsCancellationRequested)
        {
            // The handlers log the exception once, at a level that matches its status code (4xx Warning, 5xx Error).
            if (context.Response.HasStarted)
            {
                LogResponseAlreadyStarted(_logger, ex, context.TraceIdentifier);
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
