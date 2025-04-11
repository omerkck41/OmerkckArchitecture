using Core.StatusHandling.Extensions;
using Core.StatusHandling.Interfaces;
using Core.StatusHandling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.StatusHandling.Handlers;

public class TooManyRequestsHandler : IStatusCodeHandler
{
    private readonly StatusCodeHandlingOptions _options;
    private readonly ILogger<TooManyRequestsHandler> _logger;

    public TooManyRequestsHandler(IOptions<StatusCodeHandlingOptions> options, ILogger<TooManyRequestsHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool CanHandle(int statusCode) => statusCode == 429;

    public async Task HandleAsync(HttpContext context)
    {
        _logger.LogWarning("429 Too Many Requests: The user sent too many requests. URL: {Url}", context.Request.Path);

        string redirectPath = null;
        if (_options.RedirectPaths.TryGetValue(429, out redirectPath) && !string.IsNullOrEmpty(redirectPath))
        {
            context.Response.Redirect(redirectPath);
        }
        else
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Too many requests have been sent. Please try again after a while.");
        }

        if (_options.EnableNotifications)
        {
            context.AppendNotification("Your request limit has been exceeded. Wait a while and try again.", "warning");
        }
    }
}