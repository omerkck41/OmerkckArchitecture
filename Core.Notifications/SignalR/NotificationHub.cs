using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Core.Notifications.SignalR;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public async Task SendNotification(string message)
    {
        _logger.LogInformation($"Real-time notification: {message}");
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
}