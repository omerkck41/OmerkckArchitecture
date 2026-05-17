namespace Kck.Messaging.Abstractions;

/// <summary>
/// Low-level abstraction implemented by concrete email delivery providers (e.g. SMTP, SendGrid, Mailjet) to send a single message.
/// </summary>
public interface IEmailProvider
{
    /// <summary>Delivers the given <paramref name="message"/> through the underlying email transport.</summary>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
