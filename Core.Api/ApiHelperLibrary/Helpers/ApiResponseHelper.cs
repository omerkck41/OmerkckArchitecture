using Core.Api.ApiHelperLibrary.Responses;
using Microsoft.AspNetCore.Http;

namespace Core.Api.ApiHelperLibrary.Helpers;

public static class ApiResponseHelper
{
    public static ApiResponse<T> CreateSuccessResponse<T>(T data, string message = "", HttpContext httpContext = null, object resourceId = null)
    {
        var httpMethod = new HttpMethod(httpContext?.Request.Method);
        int statusCode = httpMethod switch
        {
            _ when httpMethod == HttpMethod.Get && data != null => 200,
            _ when httpMethod == HttpMethod.Get && data == null => 404,
            _ when httpMethod == HttpMethod.Post => 201,
            _ when httpMethod == HttpMethod.Put => 200,
            _ when httpMethod == HttpMethod.Delete => 204,
            _ => 200
        };

        string locationHeader = null;
        if (httpMethod == HttpMethod.Post && httpContext != null && resourceId != null)
        {
            locationHeader = LocationHelper.CreateLocationHeader(httpContext.Request, resourceId);
        }

        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            LocationHeader = locationHeader
        };
    }

    public static ApiResponse<T> CreateFailResponse<T>(string message, HttpContext httpContext = null)
    {
        var httpMethod = new HttpMethod(httpContext?.Request.Method);
        int statusCode = httpMethod switch
        {
            _ when httpMethod == HttpMethod.Get => 404,
            _ when httpMethod == HttpMethod.Post => 400,
            _ when httpMethod == HttpMethod.Put => 404,
            _ when httpMethod == HttpMethod.Delete => 404,
            _ => 400
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            StatusCode = statusCode
        };
    }
}