using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kck.AspNetCore.Responses;

public static class ApiResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response, HttpContext? httpContext = null)
    {
        var result = new ObjectResult(response) { StatusCode = response.StatusCode };

        if (!string.IsNullOrEmpty(response.Instance)
            && httpContext is not null
            && !httpContext.Response.Headers.ContainsKey("Location"))
        {
            httpContext.Response.Headers.Append("Location", response.Instance);
        }

        return result;
    }
}
