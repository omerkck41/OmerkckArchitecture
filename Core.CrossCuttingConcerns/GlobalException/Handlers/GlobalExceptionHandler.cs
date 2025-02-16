using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeoutException = Core.CrossCuttingConcerns.GlobalException.Exceptions.TimeoutException;
using ValidationException = Core.CrossCuttingConcerns.GlobalException.Exceptions.ValidationException;


namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async Task HandleExceptionAsync(HttpContext context, System.Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ConflictException => StatusCodes.Status409Conflict,
            ForbiddenException => StatusCodes.Status403Forbidden,
            BadRequestException => StatusCodes.Status400BadRequest,
            TimeoutException => StatusCodes.Status408RequestTimeout,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Detail = exception.Message,
            Extensions = { ["errorType"] = exception.GetType().Name }
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}