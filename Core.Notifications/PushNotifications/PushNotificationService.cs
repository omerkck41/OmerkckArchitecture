using Microsoft.Extensions.Logging;

namespace Core.Notifications.PushNotifications;

public class PushNotificationService : IPushNotificationService
{
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendPushNotificationAsync(string deviceToken, string title, string message)
    {
        _logger.LogInformation($"Push notification sent to device [{deviceToken}] with title: {title}.");
        // Placeholder for actual push notification implementation
        return Task.CompletedTask;
    }
}