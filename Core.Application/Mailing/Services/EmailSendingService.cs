namespace Core.Application.Mailing.Services;

public class EmailSendingService : IMailService
{
    private readonly IEnumerable<IEmailProvider> _emailProviders;

    public EmailSendingService(IEnumerable<IEmailProvider> emailProviders)
    {
        _emailProviders = emailProviders;
    }

    public async Task SendEmailAsync(EmailMessage emailMessage)
    {
        foreach (var provider in _emailProviders)
        {
            try
            {
                await provider.SendAsync(emailMessage);
                return; // If successful, exit loop
            }
            catch (Exception)
            {
                // Log and continue to next provider
            }
        }

        throw new InvalidOperationException("All email providers failed to send the email.");
    }
}