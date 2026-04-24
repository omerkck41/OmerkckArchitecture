using Kck.Exceptions.AspNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kck.Exceptions.AspNetCore.Handlers;

/// <summary>
/// <see cref="ValidationException"/> tipini işleyen handler.
/// HTTP 422 Unprocessable Entity döner ve validasyon hatalarını errors alanına yazar.
/// </summary>
public sealed partial class ValidationExceptionHandler : IKckExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool CanHandle(Exception exception) => exception is ValidationException;

    /// <inheritdoc />
    public UnifiedApiErrorResponse Handle(Exception exception, HttpContext context)
    {
        var validationException = (ValidationException)exception;
        var traceId = context.TraceIdentifier;

        LogValidationException(_logger, exception, traceId);

        var errors = validationException.Errors
            .ToDictionary(
                e => e.Property,
                e => e.Errors.ToArray());

        return new UnifiedApiErrorResponse(
            type: "https://httpstatuses.com/422",
            title: "Validation Error",
            status: StatusCodes.Status422UnprocessableEntity,
            detail: "Validation failed for one or more fields.",
            instance: context.Request.Path,
            errors: errors,
            traceId: traceId);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Validation exception occurred. TraceId: {TraceId}")]
    private static partial void LogValidationException(ILogger logger, Exception exception, string traceId);
}
