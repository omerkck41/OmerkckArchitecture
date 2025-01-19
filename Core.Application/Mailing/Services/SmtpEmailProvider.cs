using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class SmtpEmailProvider : IEmailProvider
{
    private readonly ISmtpClientSelector _smtpClientSelector;
    private readonly ILogger<SmtpEmailProvider> _logger;

    public SmtpEmailProvider(ISmtpClientSelector smtpClientSelector, ILogger<SmtpEmailProvider> logger)
    {
        _smtpClientSelector = smtpClientSelector;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage emailMessage)
    {
        try
        {
            var smtpClient = _smtpClientSelector.GetNextClient();
            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailMessage.From),
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsBodyHtml = emailMessage.IsHtml
            };

            foreach (var recipient in emailMessage.Recipients)
            {
                switch (recipient.Type)
                {
                    case RecipientType.To:
                        mailMessage.To.Add(new MailAddress(recipient.Email, recipient.Name));
                        break;
                    case RecipientType.Cc:
                        mailMessage.CC.Add(new MailAddress(recipient.Email, recipient.Name));
                        break;
                    case RecipientType.Bcc:
                        mailMessage.Bcc.Add(new MailAddress(recipient.Email, recipient.Name));
                        break;
                }
            }

            emailMessage.Attachments.ForEach(attachment => mailMessage.Attachments.Add(attachment));

            if (emailMessage.IsImportant)
                mailMessage.Priority = MailPriority.High;

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {Recipients}",
                string.Join(", ", emailMessage.Recipients.Select(r => r.Email)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email.");
            throw;
        }
    }
}