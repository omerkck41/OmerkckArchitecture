using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.ApiControllerBase.Responses;

public static class ApiResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response, HttpContext httpContext = null)
    {
        var result = new ObjectResult(response)
        {
            StatusCode = response.StatusCode
        };

        if (!string.IsNullOrEmpty(response.LocationHeader) && httpContext != null)
        {
            httpContext.Response.Headers.Add("Location", response.LocationHeader);
        }

        return result;
    }
}