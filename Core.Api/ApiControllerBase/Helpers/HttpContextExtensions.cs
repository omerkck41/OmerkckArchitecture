using Microsoft.AspNetCore.Http;

namespace Core.Api.ApiControllerBase.Helpers;

public static class HttpContextExtensions
{
    /// <summary>
    /// HttpContext üzerinden istemcinin IP adresini alır.
    /// </summary>
    public static string GetClientIpAddress(this HttpContext context)
    {
        return context?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}