using Core.CrossCuttingConcerns.GlobalException.Attributes;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;


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

    private int GetStatusCode(Exception exception)
    {
        if (exception is CustomException customEx && customEx.ExplicitStatusCode.HasValue)
            return customEx.ExplicitStatusCode.Value;

        var attr = exception.GetType().GetCustomAttribute<HttpStatusCodeAttribute>();
        return attr?.StatusCode ?? StatusCodes.Status500InternalServerError;
    }


    private UnifiedApiErrorResponse CreateCustomErrorResponse(CustomException exception, string errorId, int statusCode)
    {
        var response = UnifiedApiErrorResponse.FromException(exception)
            .WithDetail(_env.IsDevelopment() ? exception.ToString() : exception.Message);
        return response with
        {
            StatusCode = statusCode,
            AdditionalData = new { ErrorId = errorId, exception.AdditionalData }
        };
    }

    private UnifiedApiErrorResponse CreateDefaultErrorResponse(Exception exception, string errorId, int statusCode)
    {
        return UnifiedApiErrorResponse.CreateInternalServerError(errorId, _env.IsDevelopment() ? exception.ToString() : null)
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