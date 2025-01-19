using Microsoft.Extensions.Logging;

namespace Core.Application.Mailing.Services;

public class SendGridEmailProvider : IEmailProvider
{
    private readonly ILogger<SendGridEmailProvider> _logger;

    public SendGridEmailProvider(ILogger<SendGridEmailProvider> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage emailMessage)
    {
        // SendGrid API implementation goes here
        _logger.LogInformation("Sending email via SendGrid...");
        return Task.CompletedTask;
    }
}