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

        emailMessage.To.ForEach(to => mailMessage.To.Add(to));
        emailMessage.Cc.ForEach(cc => mailMessage.CC.Add(cc));
        emailMessage.Bcc.ForEach(bcc => mailMessage.Bcc.Add(bcc));
        emailMessage.Attachments.ForEach(attachment => mailMessage.Attachments.Add(attachment));

        if (emailMessage.IsImportant)
            mailMessage.Priority = MailPriority.High;


        await _smtpClient.SendMailAsync(mailMessage);
    }
}