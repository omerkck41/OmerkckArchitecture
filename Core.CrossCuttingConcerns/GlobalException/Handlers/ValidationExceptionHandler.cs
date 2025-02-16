using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(validationException.ToProblemDetails());
        }
    }
}