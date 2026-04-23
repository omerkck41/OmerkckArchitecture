using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Kck.AspNetCore.Responses;

public static class ApiResponseHelper
{
    public static ApiResponse<T> CreateSuccess<T>(
        T data,
        HttpContext httpContext,
        string message = "",
        object? resourceId = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var method = httpContext.Request.Method.ToUpperInvariant();
        int statusCode = method switch
        {
            "GET" => data is not null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
            "POST" => resourceId is not null ? StatusCodes.Status201Created : StatusCodes.Status200OK,
            "DELETE" => StatusCodes.Status204NoContent,
            _ => StatusCodes.Status200OK
        };

        string? location = null;
        if (method == "POST" && resourceId is not null)
        {
            location = $"{httpContext.Request.GetEncodedUrl().TrimEnd('/')}/{resourceId}";
        }

        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Instance = location
        };
    }

    public static ApiResponse<T> CreateFail<T>(
        string message,
        HttpContext httpContext,
        string? errorType = null,
        string? detail = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var method = httpContext.Request.Method.ToUpperInvariant();
        int statusCode = method switch
        {
            "GET" => StatusCodes.Status404NotFound,
            "POST" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            ErrorType = errorType,
            Detail = detail,
            TraceId = httpContext.TraceIdentifier
        };
    }
}
