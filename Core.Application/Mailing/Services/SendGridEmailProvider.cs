using Core.Application.Mailing.Models;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Core.Application.Mailing.Services;

public class SendGridEmailProvider : IEmailProvider
{
    private readonly SendGridClient _client;
    private readonly EmailSettings _emailSettings;

    public SendGridEmailProvider(EmailSettings emailSettings)
    {
        _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));

        var apiKey = _emailSettings.SendGridApiKey;
        _client = new SendGridClient(apiKey);
    }

    public async Task SendAsync(EmailMessage emailMessage)
    {
        var from = new EmailAddress(emailMessage.From, emailMessage.FromName);
        var subject = emailMessage.Subject;
        var to = emailMessage.Recipients
            .Where(r => r.Type == RecipientType.To)
            .Select(r => new EmailAddress(r.Email, r.Name))
            .ToList();

        var htmlContent = emailMessage.IsHtml ? emailMessage.Body : null;
        var plainTextContent = emailMessage.IsHtml ? null : emailMessage.Body;

        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, to, subject, plainTextContent, htmlContent);
        var response = await _client.SendEmailAsync(msg);

        if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
        {
            throw new CustomException("Failed to send email via SendGrid.");
        }
    }
}