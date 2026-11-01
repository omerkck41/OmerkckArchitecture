using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    // The standard problem members; AdditionalData keys with these names (any casing) are not written.
    private static readonly FrozenSet<string> ReservedMemberNames = new[]
    {
        "type", "title", "status", "detail", "instance", "errors", "traceId",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    // Same naming and null handling as the middleware that writes the body; a shallow depth limit, since this is an
    // error payload and a deep or cyclic graph should fail fast.
    private static readonly JsonSerializerOptions AdditionalDataJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = 16,
    };

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
                traceId: traceId)
            {
                Extensions = ToExtensions(customEx.AdditionalData, _logger, traceId),
            };
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

    [LoggerMessage(Level = LogLevel.Warning, Message = "AdditionalData could not be converted to problem extension members and is left out of the response ({ConversionError}). TraceId: {TraceId}")]
    private static partial void LogAdditionalDataNotWritten(ILogger logger, string conversionError, string traceId);

    /// <summary>
    /// Converts <see cref="CustomException.AdditionalData"/> to RFC 9457 extension members. A dictionary's pairs are
    /// copied (null keys skipped); any other object is serialized and its top-level properties become members with
    /// camelCase keys; a value that is not a JSON object goes under <c>additionalData</c>. Keys that collide with a
    /// standard member are skipped. Every value is converted here, so the response write cannot fail on it: data that
    /// cannot be serialized (a cycle, a <see cref="Type"/>, nesting deeper than 16) is left out as a whole and a warning
    /// naming the error type (not the data) is logged.
    /// </summary>
    [RequiresUnreferencedCode("Serializes AdditionalData of a runtime type with reflection-based System.Text.Json.")]
    [RequiresDynamicCode("Serializes AdditionalData of a runtime type with reflection-based System.Text.Json.")]
    private static Dictionary<string, object?>? ToExtensions(object? additionalData, ILogger logger, string traceId)
    {
        if (additionalData is null)
            return null;

        try
        {
            var extensions = new Dictionary<string, object?>(StringComparer.Ordinal);

            if (additionalData is IEnumerable<KeyValuePair<string, object?>> pairs)
            {
                foreach (var (key, value) in pairs)
                {
                    if (IsWritable(key) && !extensions.ContainsKey(key))
                        extensions[key] = value is null ? null : JsonSerializer.SerializeToElement(value, AdditionalDataJsonOptions);
                }
            }
            else
            {
                var element = JsonSerializer.SerializeToElement(additionalData, AdditionalDataJsonOptions);
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in element.EnumerateObject())
                    {
                        if (IsWritable(property.Name))
                            extensions.TryAdd(property.Name, property.Value);
                    }
                }
                else
                {
                    extensions.Add("additionalData", element);
                }
            }

            return extensions.Count == 0 ? null : extensions;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or ArgumentException or InvalidOperationException)
        {
            LogAdditionalDataNotWritten(logger, ex.GetType().Name, traceId);
            return null;
        }
    }

    private static bool IsWritable([NotNullWhen(true)] string? key) => key is not null && !ReservedMemberNames.Contains(key);

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
