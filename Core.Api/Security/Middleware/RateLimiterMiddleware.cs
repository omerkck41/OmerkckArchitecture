using Core.Api.Security.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Core.Api.Security.Middleware;

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, (int RequestCount, DateTime StartTime)> RequestLogs = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;

    public RateLimiterMiddleware(RequestDelegate next, IOptions<SecuritySettings> securitySettings)
    {
        _next = next;
        _maxRequests = securitySettings.Value.RateLimit;
        _timeWindow = TimeSpan.FromMinutes(1);
    }

    public async Task Invoke(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ipAddress))
        {
            await _next(context);
            return;
        }

        if (!RequestLogs.TryGetValue(ipAddress, out var log) || DateTime.UtcNow - log.StartTime > _timeWindow)
        {
            RequestLogs[ipAddress] = (1, DateTime.UtcNow);
        }
        else if (log.RequestCount >= _maxRequests)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }
        else
        {
            RequestLogs[ipAddress] = (log.RequestCount + 1, log.StartTime);
        }

        await _next(context);
    }
}