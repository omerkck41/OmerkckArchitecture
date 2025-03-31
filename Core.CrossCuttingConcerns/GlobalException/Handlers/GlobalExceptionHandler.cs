using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TimeoutException = Core.CrossCuttingConcerns.GlobalException.Exceptions.TimeoutException;
using ValidationException = Core.CrossCuttingConcerns.GlobalException.Exceptions.ValidationException;


namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;
    private readonly JsonSerializerOptions _jsonOptions;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Benzersiz hata kimliği oluştur
        var errorId = Guid.NewGuid().ToString();

        // Hata türüne göre HTTP durum kodunu belirle
        int statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ConflictException => StatusCodes.Status409Conflict,
            ForbiddenException => StatusCodes.Status403Forbidden,
            BadRequestException => StatusCodes.Status400BadRequest,
            TimeoutException => StatusCodes.Status408RequestTimeout,
            InvalidOperationException ioe when ioe.Message.Contains("No authenticationScheme was specified") => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        // Loglama: Hata detaylarını errorId ile birlikte loglayın
        _logger.LogError(exception, "Error occurred. ErrorId: {ErrorId}", errorId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        // Kullanıcı dostu mesaj üretme
        string userFriendlyMessage = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Invalid request.",
            StatusCodes.Status401Unauthorized => "Please login to access this page.",
            StatusCodes.Status403Forbidden => "You are not authorized to take this action.",
            StatusCodes.Status404NotFound => "The requested resource was not found.",
            _ => "An unexpected error occurred. Please try again later."
        };

        // Detay: Geliştirme ortamındaysanız daha fazla detay sunabilirsiniz
        string detail = _env.IsDevelopment() ? exception.ToString() : exception.Message;

        var errorResponse = new UnifiedApiErrorResponse
        {
            Success = false,
            StatusCode = statusCode,
            Message = userFriendlyMessage,
            ErrorType = exception.GetType().Name,
            Detail = detail,
            AdditionalData = new { ErrorId = errorId } // Destek taleplerinde kullanılabilir
        };

        string jsonResponse = JsonSerializer.Serialize(errorResponse, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}