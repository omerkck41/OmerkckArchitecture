using Core.Api.Security.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Core.Api.Security.Middleware;

public class BruteForceProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Dictionary<string, (int AttemptCount, DateTime LastAttempt)> LoginAttempts = new();
    private readonly int _maxLoginAttempts;
    private readonly TimeSpan _lockoutTime;

    public BruteForceProtectionMiddleware(RequestDelegate next, IOptions<SecuritySettings> securitySettings)
    {
        _next = next;
        _maxLoginAttempts = securitySettings.Value.MaxLoginAttempts;
        _lockoutTime = TimeSpan.FromMinutes(securitySettings.Value.LockoutTime);
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.ToString().Contains("/login"))
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                if (LoginAttempts.TryGetValue(ipAddress, out var attemptData))
                {
                    if (attemptData.AttemptCount >= _maxLoginAttempts && DateTime.UtcNow - attemptData.LastAttempt < _lockoutTime)
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