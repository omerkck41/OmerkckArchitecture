using Core.StatusHandling.Extensions;
using Core.StatusHandling.Interfaces;
using Core.StatusHandling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Core.StatusHandling.Handlers;

public class UnauthorizedStatusCodeHandler : IStatusCodeHandler
{
    private readonly StatusCodeHandlingOptions _options;

    public UnauthorizedStatusCodeHandler(IOptions<StatusCodeHandlingOptions> options)
    {
        _options = options.Value;
    }

    // Bu handler 401 (Unauthorized) ve 403 (Forbidden) kodlarını işler
    public bool CanHandle(int statusCode) => statusCode == 401 || statusCode == 403;

    public async Task HandleAsync(HttpContext context)
    {
        string? redirectPath = null;
        string notificationMessage = "You are not authorized for this operation.";
        string notificationType = "warning";

        int statusCode = context.Response.StatusCode;

        if (statusCode == 401)
        {
            notificationMessage = "Please log in.";
            notificationType = "info";
            _options.RedirectPaths.TryGetValue(401, out redirectPath);
            redirectPath ??= "/Account/Login";
        }
        else if (statusCode == 403)
        {
            notificationMessage = "You are not authorized for this operation.";
            notificationType = "warning";
            _options.RedirectPaths.TryGetValue(403, out redirectPath);
            redirectPath ??= "/";
        }

        // İstek browser'dan mı? Yoksa JSON mu?
        var accept = context.Request.Headers["Accept"].ToString();

        if (accept.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            if (_options.EnableNotifications)
            {
                context.AppendNotification(notificationMessage, notificationType);
            }

            if (!string.IsNullOrEmpty(redirectPath))
            {
                context.Response.Redirect(redirectPath);
            }
            else
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(notificationMessage);
            }
        }
        else
        {
            // API istekleri için basic JSON response
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                message = notificationMessage
            }));
        }
    }
}