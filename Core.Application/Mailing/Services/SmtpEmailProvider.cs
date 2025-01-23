using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class SmtpEmailProvider : IEmailProvider
{
    private readonly ISmtpClientSelector _smtpClientSelector;

    public SmtpEmailProvider(ISmtpClientSelector smtpClientSelector)
    {
        _smtpClientSelector = smtpClientSelector;
    }

    public async Task SendAsync(EmailMessage emailMessage)
    {
        var smtpClient = _smtpClientSelector.GetNextClient();
        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailMessage.From), // Sender name can be configurable
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

        await smtpClient.SendMailAsync(mailMessage);
    }
}