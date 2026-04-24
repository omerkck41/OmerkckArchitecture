using System.Collections.Concurrent;
using System.Reflection;
using Kck.Exceptions.AspNetCore.Models;
using Kck.Exceptions.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kck.Exceptions.AspNetCore.Handlers;

/// <summary>
/// <see cref="CustomException"/> ve alt sınıflarını işleyen genel exception handler.
/// <see cref="HttpStatusCodeAttribute"/> veya <see cref="CustomException.ExplicitStatusCode"/> ile
/// HTTP durum kodu belirlenir. Bulunamazsa 500 döner.
/// </summary>
public sealed partial class GlobalExceptionHandler : IKckExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    private static readonly ConcurrentDictionary<Type, int> StatusCodeCache = new();

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool CanHandle(Exception exception) => exception is CustomException;

    /// <inheritdoc />
    public UnifiedApiErrorResponse Handle(Exception exception, HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var statusCode = ResolveStatusCode(exception);

        if (exception is CustomException customEx)
        {
            LogHandledCustomException(_logger, exception, traceId, customEx.ErrorType);

            return new UnifiedApiErrorResponse(
                type: $"https://httpstatuses.com/{statusCode}",
                title: customEx.ErrorType,
                status: statusCode,
                detail: customEx.Message,
                instance: context.Request.Path,
                errors: null,
                traceId: traceId);
        }

        // Fallback — bu dal normalde çağrılmaz (CanHandle kontrolü nedeniyle)
        LogUnhandledGlobalException(_logger, exception, traceId);

        return new UnifiedApiErrorResponse(
            type: "https://httpstatuses.com/500",
            title: "Internal Server Error",
            status: StatusCodes.Status500InternalServerError,
            detail: "An unexpected error occurred.",
            instance: context.Request.Path,
            errors: null,
            traceId: traceId);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Handled CustomException. TraceId: {TraceId}, Type: {ExceptionType}")]
    private static partial void LogHandledCustomException(ILogger logger, Exception exception, string traceId, string exceptionType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception routed to GlobalExceptionHandler. TraceId: {TraceId}")]
    private static partial void LogUnhandledGlobalException(ILogger logger, Exception exception, string traceId);

    private static int ResolveStatusCode(Exception exception)
    {
        // 1. ExplicitStatusCode varsa onu kullan
        if (exception is CustomException { ExplicitStatusCode: not null } customEx)
        {
            return customEx.ExplicitStatusCode.Value;
        }

        // 2. Attribute cache
        var exceptionType = exception.GetType();
        if (StatusCodeCache.TryGetValue(exceptionType, out var cached))
        {
            return cached;
        }

        // 3. HttpStatusCodeAttribute oku
        var attr = exceptionType.GetCustomAttribute<HttpStatusCodeAttribute>();
        var statusCode = attr?.StatusCode ?? StatusCodes.Status500InternalServerError;
        StatusCodeCache.TryAdd(exceptionType, statusCode);
        return statusCode;
    }
}
