using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async Task HandleExceptionAsync(HttpContext context, System.Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = new ErrorResponse
            {
                Success = false,
                Message = "Validation errors occurred.",
                Errors = validationException.Errors,
                Type = nameof(ValidationException)
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}