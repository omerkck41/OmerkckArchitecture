using Core.Api.ApiControllerBase.Responses;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Core.Api.ApiControllerBase.Helpers;

public static class ApiResponseHelper
{
    public static ApiResponse<T> CreateSuccessResponse<T>(T data, string message = "", HttpContext httpContext = null, object resourceId = null)
    {
        if (httpContext == null || string.IsNullOrWhiteSpace(httpContext.Request?.Method))
            throw new CustomException(nameof(httpContext), "HttpContext or Request Method cannot be empty.");

        var method = httpContext.Request.Method.ToUpperInvariant();
        int statusCode = method switch
        {
            "GET" => data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
            "POST" =>
                    statusCode = resourceId != null
                    ? StatusCodes.Status201Created
                    : StatusCodes.Status200OK,

            "PUT" => StatusCodes.Status200OK,
            "PATCH" => StatusCodes.Status200OK,
            "DELETE" => StatusCodes.Status204NoContent,
            _ => StatusCodes.Status200OK
        };

        string locationHeader = null;
        if (method == "POST" && resourceId != null)
        {
            // .NET'in hazır extension metodunu kullanarak URL oluşturma
            locationHeader = $"{httpContext.Request.GetEncodedUrl().TrimEnd('/')}/{resourceId}";
        }

        return new ApiResponse<T>(true, message, data, statusCode, LocationHeader: locationHeader ?? string.Empty);
    }

    public static ApiResponse<T> CreateFailResponse<T>(string message, HttpContext httpContext = null)
    {
        if (httpContext == null || string.IsNullOrWhiteSpace(httpContext.Request?.Method))
            throw new CustomException(nameof(httpContext), "HttpContext or Request Method cannot be empty.");

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

        return new ApiResponse<T>(false, message, default, statusCode, string.Empty);
    }
}