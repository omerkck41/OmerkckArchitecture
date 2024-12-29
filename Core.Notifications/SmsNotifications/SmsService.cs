using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Core.Notifications.SmsNotifications;

public class SmsService : ISmsService
{
    private readonly SmsSettings _settings;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
    {
        _settings = configuration.GetSection("SmsSettings").Get<SmsSettings>()
            ?? throw new InvalidOperationException("SMS settings are not configured.");
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            TwilioClient.Init(_settings.TwilioAccountSid, _settings.TwilioAuthToken);

            await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_settings.FromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            _logger.LogInformation($"SMS sent to {phoneNumber} successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send SMS to {phoneNumber}.");
            throw;
        }
    }
}