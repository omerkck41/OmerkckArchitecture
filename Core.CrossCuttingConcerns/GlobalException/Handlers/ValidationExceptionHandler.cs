using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            // Errors'u düzgün bir object formatına dönüştür
            var formattedErrors = validationException.Errors
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object)kvp.Value
                );

            var response = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation error",
                Detail = validationException.ToString(),
                Extensions = { ["errors"] = formattedErrors }
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}