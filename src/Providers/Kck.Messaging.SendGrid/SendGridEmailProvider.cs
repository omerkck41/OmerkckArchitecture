using Kck.Messaging.Abstractions;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Kck.Messaging.SendGrid;

public sealed class SendGridEmailProvider(IOptionsMonitor<SendGridOptions> options) : IEmailProvider
{
    private readonly SendGridClient _client = new(options.CurrentValue.ApiKey);

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var from = new EmailAddress(message.From, message.FromName);
        var tos = message.Recipients
            .Where(r => r.Type == RecipientType.To)
            .Select(r => new EmailAddress(r.Email, r.Name))
            .ToList();

        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
            from, tos, message.Subject,
            message.IsHtml ? null : message.Body,
            message.IsHtml ? message.Body : null);

        foreach (var r in message.Recipients.Where(r => r.Type == RecipientType.Cc))
            msg.AddCc(new EmailAddress(r.Email, r.Name));
        foreach (var r in message.Recipients.Where(r => r.Type == RecipientType.Bcc))
            msg.AddBcc(new EmailAddress(r.Email, r.Name));

        foreach (var att in message.Attachments)
        {
            using var ms = new MemoryStream();
            await att.Content.CopyToAsync(ms, ct).ConfigureAwait(false);
            msg.AddAttachment(att.FileName, Convert.ToBase64String(ms.ToArray()), att.ContentType);
        }

        var response = await _client.SendEmailAsync(msg, ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"SendGrid returned {response.StatusCode}.");
    }
}
