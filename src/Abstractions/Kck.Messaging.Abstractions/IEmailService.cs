namespace Kck.Messaging.Abstractions;

/// <summary>
/// High-level application-facing service for sending emails, abstracting provider selection, retry, and error handling from calling code.
/// </summary>
[Obsolete(
    "No package implements IEmailService; every provider registers IEmailProvider, which has the same SendAsync. " +
    "Removed in v5.0.",
    DiagnosticId = "KCK0600",
    UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
public interface IEmailService
{
    /// <summary>Sends the specified <paramref name="message"/> using the configured email provider.</summary>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
