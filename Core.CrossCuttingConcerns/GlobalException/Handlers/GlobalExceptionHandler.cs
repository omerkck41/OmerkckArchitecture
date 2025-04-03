using Core.CrossCuttingConcerns.GlobalException.Attributes;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.Text.Json;


namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;
    private readonly IStringLocalizer<GlobalExceptionHandler> _localizer;
    private readonly JsonSerializerOptions _jsonOptions;

    // Cache: Exception tipi -> Durum Kodu
    private static readonly ConcurrentDictionary<Type, int> _statusCodeCache = new();

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env, IStringLocalizer<GlobalExceptionHandler> localizer)
    {
        _logger = logger;
        _env = env;
        _localizer = localizer;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };
    }

    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorId = Guid.NewGuid().ToString();
        LogException(exception, errorId);

        int statusCode = GetStatusCode(exception);

        UnifiedApiErrorResponse response = exception switch
        {
            CustomException customEx => CreateCustomErrorResponse(customEx, errorId, statusCode),
            _ => CreateDefaultErrorResponse(exception, errorId, statusCode)
        };

        await WriteResponseAsync(context, response);
    }

    private void LogException(Exception exception, string errorId)
    {
        if (exception is CustomException)
            _logger.LogWarning(exception, "Handled exception. ErrorId: {ErrorId}", errorId);
        else
            _logger.LogError(exception, "Unhandled exception. ErrorId: {ErrorId}", errorId);
    }

    private static int GetStatusCode(Exception exception)
    {
        if (exception is CustomException customEx && customEx.ExplicitStatusCode.HasValue)
            return customEx.ExplicitStatusCode.Value;

        var exceptionType = exception.GetType();
        if (_statusCodeCache.TryGetValue(exceptionType, out int cachedStatusCode))
        {
            return cachedStatusCode;
        }

        var attr = exceptionType.GetCustomAttribute<HttpStatusCodeAttribute>();
        int statusCode = attr?.StatusCode ?? StatusCodes.Status500InternalServerError;
        _statusCodeCache.TryAdd(exceptionType, statusCode);
        return statusCode;
    }


    private UnifiedApiErrorResponse CreateCustomErrorResponse(CustomException exception, string errorId, int statusCode)
    {
        // Lokalizasyon kullanarak hata mesajını çeviriyoruz.
        string localizedMessage = _localizer[exception.Message];
        var response = UnifiedApiErrorResponse.FromException(exception) with
        {
            ErrorId = errorId,
            StatusCode = statusCode,
            Message = localizedMessage, // Lokalize edilmiş mesaj
            AdditionalData = exception.AdditionalData
        };
        return response;
    }
    private UnifiedApiErrorResponse CreateDefaultErrorResponse(Exception exception, string errorId, int statusCode)
    {
        // Genel hata mesajını lokalize edelim.
        string localizedMessage = _localizer["UnhandledError"];
        return UnifiedApiErrorResponse.CreateInternalServerError(errorId, _env.IsDevelopment() ? exception.ToString() : localizedMessage)
            with
        { StatusCode = statusCode };
    }
    private async Task WriteResponseAsync(HttpContext context, UnifiedApiErrorResponse response)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonOptions));
    }
}