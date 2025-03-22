using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using TimeoutException = Core.CrossCuttingConcerns.GlobalException.Exceptions.TimeoutException;
using ValidationException = Core.CrossCuttingConcerns.GlobalException.Exceptions.ValidationException;


namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async Task HandleExceptionAsync(HttpContext context, System.Exception exception)
    {
        // Hata tipi bazında status kodu belirleme
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

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResponse = new UnifiedApiErrorResponse
        {
            StatusCode = statusCode,
            Message = statusCode == StatusCodes.Status500InternalServerError ? "Unexpected error occurred" : "Error occurred",
            ErrorType = exception.GetType().Name,
            Detail = exception.Message
        };

        // Eğer exception, CustomException ise ve ek veri varsa bunu da ekle.
        if (exception is CustomException customException && customException.AdditionalData != null)
        {
            errorResponse.AdditionalData = customException.AdditionalData;
        }

        JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}