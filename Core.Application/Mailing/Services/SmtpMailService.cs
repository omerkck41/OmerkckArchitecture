using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class SmtpMailService : IMailService
{
    private readonly SmtpClient _smtpClient;
    private readonly ILogger<SmtpMailService> _logger;

    public SmtpMailService(SmtpClient smtpClient, ILogger<SmtpMailService> logger)
    {
        _smtpClient = smtpClient;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailMessage emailMessage)
    {
        try
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

            _logger.LogInformation("Sending email to {Recipients}", string.Join(", ", emailMessage.To));
            await _smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email.");
            throw;
        }
    }
}