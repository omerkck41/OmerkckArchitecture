using Core.Api.ApiControllerBase.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.ApiControllerBase.Controllers;

/// <summary>
/// Tüm API controller'lar için ortak özellikler ve yardımcı metotlar içerir.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ApiSuccess<T>(T data, string message = "", int statusCode = 200)
    {
        var response = new ApiResponse<T>
        {
            Success = true,
            Message = message,
            AdditionalData = data,
            StatusCode = statusCode,
            Instance = HttpContext.Request.Path
        };

        return new ObjectResult(response) { StatusCode = statusCode };
    }

    protected IActionResult ApiFail(string message, int statusCode = 400, string? errorType = null, string? detail = null)
    {
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            AdditionalData = null,
            Instance = HttpContext.Request.Path,
            ErrorType = errorType,
            Detail = detail,
            ErrorId = HttpContext.TraceIdentifier
        };

        return new ObjectResult(response) { StatusCode = statusCode };
    }
}