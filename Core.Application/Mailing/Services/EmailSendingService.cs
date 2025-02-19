using Core.Application.Mailing.Models;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Application.Mailing.Services;

public class EmailSendingService : IMailService
{
    private readonly IEnumerable<IEmailProvider> _emailProviders;
    private readonly EmailSettings _emailSettings;

    public EmailSendingService(IEnumerable<IEmailProvider> emailProviders, EmailSettings emailSettings)
    {
        _emailProviders = emailProviders;
        _emailSettings = emailSettings;
    }

    public async Task SendEmailAsync(EmailMessage emailMessage)
    {
        // Öncelikli sağlayıcıyı bul
        var preferredProvider = _emailProviders.FirstOrDefault(p => p.GetType().Name.StartsWith(_emailSettings.PreferredProvider));

        if (preferredProvider != null)
        {
            try
            {
                await preferredProvider.SendAsync(emailMessage);
                return; // Öncelikli sağlayıcı başarılı olduğunda işlemi sonlandır
            }
            catch (Exception)
            {
                // Öncelikli sağlayıcı başarısız olursa diğer sağlayıcıları dene
            }
        }

        // Öncelikli sağlayıcı yoksa veya başarısız olduysa diğer sağlayıcıları dene
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

        throw new CustomException("All email providers failed to send the email.");
    }
}