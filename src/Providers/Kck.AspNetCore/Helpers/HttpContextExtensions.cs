using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Helpers;

public static class HttpContextExtensions
{
    public static string GetClientIpAddress(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
