using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errorId = Guid.NewGuid().ToString();
            _logger.LogWarning(exception, "Validation error occurred. ErrorId: {ErrorId}", errorId);

            var errorResponse = UnifiedApiErrorResponse.FromValidationException(
                validationException,
                errorId,
                "Validation failed for one or more fields.");

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, _jsonOptions));
        }
    }

    async Task IExceptionHandler.HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            await HandleExceptionAsync(context, validationException);
        }
    }
}