using Core.Api.Security.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;

namespace Core.Api.Security.Middleware;

public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _allowedIps;

    public IpWhitelistMiddleware(RequestDelegate next, IOptions<SecuritySettings> securitySettings)
    {
        _next = next;
        _allowedIps = securitySettings.Value.AllowedIPs ?? new List<string>();
    }

    public async Task Invoke(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();

        // remoteIp null veya boş ise de erişim reddedilir.
        if (string.IsNullOrEmpty(remoteIp) || !_allowedIps.Contains(remoteIp))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Access denied. Not authorized IP address.");
            return;
        }
        await _next(context);
    }
}