using Kck.AspNetCore.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kck.AspNetCore.Controllers;

public abstract class KckApiControllerBase : ControllerBase
{
    protected IActionResult ApiSuccess<T>(T data, string message = "", int statusCode = 200)
    {
        var response = new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
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
            Instance = HttpContext.Request.Path,
            ErrorType = errorType,
            Detail = detail,
            TraceId = HttpContext.TraceIdentifier
        };

        return new ObjectResult(response) { StatusCode = statusCode };
    }
}
