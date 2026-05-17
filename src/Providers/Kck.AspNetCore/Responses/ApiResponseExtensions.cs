using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kck.AspNetCore.Responses;

/// <summary>
/// Extension methods that convert an <see cref="ApiResponse{T}"/> into an
/// <see cref="IActionResult"/> suitable for returning from MVC or Minimal API endpoints.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Wraps <paramref name="response"/> in an <see cref="ObjectResult"/> with the correct HTTP
    /// status code and, when a resource instance is present, appends a Location header.
    /// </summary>
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
