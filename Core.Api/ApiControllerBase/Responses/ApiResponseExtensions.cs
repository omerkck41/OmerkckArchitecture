using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.ApiControllerBase.Responses;

public static class ApiResponseExtensions
{
    /// <summary>
    /// ApiResponse nesnesini ObjectResult'a dönüştürür ve gerekirse Location header ekler.
    /// </summary>
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response, HttpContext? httpContext = null)
    {
        var result = new ObjectResult(response)
        {
            StatusCode = response.StatusCode
        };

        if (!string.IsNullOrEmpty(response.Instance) && httpContext is not null)
        {
            if (!httpContext.Response.Headers.ContainsKey("Location"))
            {
                httpContext.Response.Headers.Append("Location", response.Instance);
            }
        }

        return result;
    }
}