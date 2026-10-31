using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Kck.Exceptions.AspNetCore.Models;
using Kck.Exceptions.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kck.Exceptions.AspNetCore.Handlers;

/// <summary>
/// General exception handler for <see cref="CustomException"/> and its subclasses.
/// <see cref="HttpStatusCodeAttribute"/> or <see cref="CustomException.ExplicitStatusCode"/>
/// the HTTP status code is determined. Returns 500 if none is found.
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
            if (statusCode >= StatusCodes.Status500InternalServerError)
                LogCustomServerError(_logger, exception, traceId, customEx.ErrorType);
            else
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

        // Fallback — normally never reached (because of the CanHandle check)
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

    [LoggerMessage(Level = LogLevel.Error, Message = "CustomException mapped to a server error. TraceId: {TraceId}, Type: {ExceptionType}")]
    private static partial void LogCustomServerError(ILogger logger, Exception exception, string traceId, string exceptionType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception routed to GlobalExceptionHandler. TraceId: {TraceId}")]
    private static partial void LogUnhandledGlobalException(ILogger logger, Exception exception, string traceId);

    [RequiresUnreferencedCode("Reads HttpStatusCodeAttribute from runtime exception types via reflection.")]
    private static int ResolveStatusCode(Exception exception)
    {
        // 1. Use ExplicitStatusCode if present
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

        // 3. Read HttpStatusCodeAttribute
        var attr = exceptionType.GetCustomAttribute<HttpStatusCodeAttribute>();
        var statusCode = attr?.StatusCode ?? StatusCodes.Status500InternalServerError;
        StatusCodeCache.TryAdd(exceptionType, statusCode);
        return statusCode;
    }
}
