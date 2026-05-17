using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Helpers;

/// <summary>
/// Extension methods for <see cref="HttpContext"/> that expose commonly needed request metadata.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Returns the remote IP address of the current HTTP connection, or "unknown" when unavailable.
    /// </summary>
    public static string GetClientIpAddress(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
