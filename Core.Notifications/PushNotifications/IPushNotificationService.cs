namespace Core.Notifications.PushNotifications;

public interface IPushNotificationService
{
    Task SendPushNotificationAsync(string deviceToken, string title, string message);
}