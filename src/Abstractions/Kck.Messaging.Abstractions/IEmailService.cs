namespace Kck.Messaging.Abstractions;

/// <summary>
/// High-level application-facing service for sending emails, abstracting provider selection, retry, and error handling from calling code.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends the specified <paramref name="message"/> using the configured email provider.</summary>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
