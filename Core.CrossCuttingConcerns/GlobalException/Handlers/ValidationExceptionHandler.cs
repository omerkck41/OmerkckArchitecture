using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errorResponse = new UnifiedApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Validation error",
                ErrorType = nameof(ValidationException),
                Detail = "Validation failed for one or more fields.",
                AdditionalData = validationException.Errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, _jsonOptions));
        }
    }
}