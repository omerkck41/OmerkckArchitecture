using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Core.Api.Security.Middleware;

public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _allowedIps;

    public IpWhitelistMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _allowedIps = configuration.GetSection("AllowedIPs").Get<List<string>>() ?? new();
    }

    public async Task Invoke(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        if (!_allowedIps.Contains(remoteIp))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Erişim reddedildi. Yetkili IP adresi değil.");
            return;
        }
        await _next(context);
    }
}