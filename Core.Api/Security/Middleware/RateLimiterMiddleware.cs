using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace Core.Api.Security.Middleware;

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, (int RequestCount, DateTime StartTime)> RequestLogs = new();
    private const int MaxRequests = 100;
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

    public RateLimiterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ipAddress))
        {
            await _next(context);
            return;
        }

        if (!RequestLogs.TryGetValue(ipAddress, out var log) || DateTime.UtcNow - log.StartTime > TimeWindow)
        {
            RequestLogs[ipAddress] = (1, DateTime.UtcNow);
        }
        else if (log.RequestCount >= MaxRequests)
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