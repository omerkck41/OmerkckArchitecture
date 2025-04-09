using Core.Api.ApiControllerBase.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Core.Api.ApiControllerBase.Helpers;

public static class ApiResponseHelper
{
    /// <summary>
    /// Başarılı işlemler için standart yanıt oluşturur.
    /// </summary>
    public static ApiResponse<T> CreateSuccessResponse<T>(
        T data,
        string message = "",
        HttpContext? httpContext = null,
        object? resourceId = null)
    {
        if (httpContext == null || httpContext.Request == null || string.IsNullOrWhiteSpace(httpContext.Request.Method))
            throw new ArgumentNullException(nameof(httpContext), "HttpContext and Request Method cannot be null or empty.");

        var method = httpContext.Request.Method.ToUpperInvariant();
        int statusCode = method switch
        {
            "GET" => data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
            "POST" => resourceId != null ? StatusCodes.Status201Created : StatusCodes.Status200OK,
            "PUT" => StatusCodes.Status200OK,
            "PATCH" => StatusCodes.Status200OK,
            "DELETE" => StatusCodes.Status204NoContent,
            _ => StatusCodes.Status200OK
        };

        string? locationHeader = null;
        if (method == "POST" && resourceId != null)
        {
            // Request URL ve resourceId'nin birleşmesiyle Location header oluşturulur.
            locationHeader = $"{httpContext.Request.GetEncodedUrl().TrimEnd('/')}/{resourceId}";
        }

        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            AdditionalData = data,
            StatusCode = statusCode,
            Instance = locationHeader ?? string.Empty
        };
    }

    /// <summary>
    /// Başarısız işlemler için standart yanıt oluşturur.
    /// </summary>
    public static ApiResponse<T> CreateFailResponse<T>(string message, HttpContext? httpContext = null)
    {
        if (httpContext == null || httpContext.Request == null || string.IsNullOrWhiteSpace(httpContext.Request.Method))
            throw new ArgumentNullException(nameof(httpContext), "HttpContext and Request Method cannot be null or empty.");

        var method = httpContext.Request.Method.ToUpperInvariant();
        int statusCode = method switch
        {
            "GET" => StatusCodes.Status404NotFound,
            "POST" => StatusCodes.Status400BadRequest,
            "PUT" => StatusCodes.Status404NotFound,
            "PATCH" => StatusCodes.Status404NotFound,
            "DELETE" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            AdditionalData = default!,
            StatusCode = statusCode,
            Instance = string.Empty
        };
    }
}