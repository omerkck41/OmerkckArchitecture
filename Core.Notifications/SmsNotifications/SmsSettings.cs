namespace Core.Notifications.SmsNotifications;

public class SmsSettings
{
    public string TwilioAccountSid { get; set; }
    public string TwilioAuthToken { get; set; }
    public string FromPhoneNumber { get; set; }
}