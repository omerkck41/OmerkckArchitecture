namespace Core.Notifications.SignalR;

public interface INotificationService
{
    Task SendRealTimeNotificationAsync(string message);
}
