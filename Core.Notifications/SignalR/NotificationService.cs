using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Core.Notifications.SignalR;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendRealTimeNotificationAsync(string message)
    {
        _logger.LogInformation($"Broadcasting notification: {message}");
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
    }
}