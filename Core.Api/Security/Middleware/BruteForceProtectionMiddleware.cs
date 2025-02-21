using Microsoft.AspNetCore.Http;

namespace Core.Api.Security.Middleware;

public class BruteForceProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Dictionary<string, (int AttemptCount, DateTime LastAttempt)> LoginAttempts = new();
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutTime = TimeSpan.FromMinutes(5);

    public BruteForceProtectionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.ToString().Contains("/login"))
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                if (LoginAttempts.TryGetValue(ipAddress, out var attemptData))
                {
                    if (attemptData.AttemptCount >= MaxAttempts && DateTime.UtcNow - attemptData.LastAttempt < LockoutTime)
                    {
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        await context.Response.WriteAsync("Too many failed login attempts. Try again later.");
                        return;
                    }
                    LoginAttempts[ipAddress] = (attemptData.AttemptCount + 1, DateTime.UtcNow);
                }
                else
                {
                    LoginAttempts[ipAddress] = (1, DateTime.UtcNow);
                }
            }
        }
        await _next(context);
    }
}