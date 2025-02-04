using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using TimeoutException = Core.CrossCuttingConcerns.GlobalException.Exceptions.TimeoutException;


namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public static class GlobalExceptionHandler
{
    public static async Task HandleExceptionAsync(HttpContext context, System.Exception exception)
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

        // IHostEnvironment Kullanımı
        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

        var response = new ErrorResponse
        {
            Success = false,
            Message = exception.Message,
            Detail = exception.InnerException?.Message,
            Type = exception.GetType().Name,
            Errors = exception is ValidationException validationException ? validationException.Errors : null,
            StackTrace = env.IsDevelopment() ? exception.StackTrace : null,
            ErrorCode = statusCode, // HTTP durum kodunu hata kodu olarak kullanabilirsiniz
            CorrelationId = context.TraceIdentifier, // İstek kimliği
            Timestamp = DateTime.UtcNow // Zaman damgası
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonSerializerOptions));
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}