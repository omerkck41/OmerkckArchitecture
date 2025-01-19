using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class SmtpMailService : IMailService
{
    private readonly SmtpClient _smtpClient;

    public SmtpMailService(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public async Task SendEmailAsync(EmailMessage emailMessage)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailMessage.From),
            Subject = emailMessage.Subject,
            Body = emailMessage.Body,
            IsBodyHtml = emailMessage.IsHtml
        };

        // Recipients'i uygun şekilde filtreleme
        emailMessage.Recipients
            .Where(r => r.Type == RecipientType.To)
            .ToList()
            .ForEach(r => mailMessage.To.Add(new MailAddress(r.Email, r.Name)));

        emailMessage.Recipients
            .Where(r => r.Type == RecipientType.Cc)
            .ToList()
            .ForEach(r => mailMessage.CC.Add(new MailAddress(r.Email, r.Name)));

        emailMessage.Recipients
            .Where(r => r.Type == RecipientType.Bcc)
            .ToList()
            .ForEach(r => mailMessage.Bcc.Add(new MailAddress(r.Email, r.Name)));

        emailMessage.Attachments.ForEach(attachment => mailMessage.Attachments.Add(attachment));

        if (emailMessage.IsImportant)
            mailMessage.Priority = MailPriority.High;

        await _smtpClient.SendMailAsync(mailMessage);
    }
}