using Kck.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Kck.Messaging.SendGrid;

/// <summary>
/// Sends emails via the SendGrid Web API v3, supporting multiple recipients, Cc/Bcc, and file attachments.
/// </summary>
public sealed class SendGridEmailProvider : IEmailProvider
{
    private readonly ISendGridClient _client;

    /// <summary>
    /// Initializes the provider with a SendGrid client built from <see cref="SendGridOptions.ApiKey"/>. This is the
    /// constructor dependency injection uses for a plain type registration (the 4.x behaviour);
    /// <c>AddKckMessagingSendGrid</c> picks the registered-client constructor itself.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public SendGridEmailProvider(IOptionsMonitor<SendGridOptions> options)
        : this(new SendGridClient(options.CurrentValue.ApiKey))
    {
    }

    /// <summary>
    /// Initializes the provider with an existing SendGrid client, for example one registered in the container with
    /// its own <see cref="SendGridClientOptions"/> (proxy, host, EU data residency) or a test double.
    /// </summary>
    public SendGridEmailProvider(ISendGridClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <summary>
    /// Sends the given <paramref name="message"/> through SendGrid and throws <see cref="InvalidOperationException"/> if the API returns a non-success status.
    /// </summary>
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
            // Copy the whole stream: one the caller has just written is positioned at its end.
            if (att.Content.CanSeek)
                att.Content.Position = 0;

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
