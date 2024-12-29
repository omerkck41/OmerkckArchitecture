namespace Core.Notifications.SmsNotifications;

public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
}