using Kck.AspNetCore.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kck.AspNetCore.Controllers;

/// <summary>
/// Abstract base controller that provides standardised <see cref="ApiResponse{T}"/> helper methods
/// for returning success and failure results from API endpoints.
/// </summary>
public abstract class KckApiControllerBase : ControllerBase
{
    /// <summary>
    /// Returns a successful API response with the given data and an optional message.
    /// The HTTP status code defaults to 200 OK.
    /// </summary>
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

    /// <summary>
    /// Returns a failed API response with the given message, status code, optional error type and detail.
    /// </summary>
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
