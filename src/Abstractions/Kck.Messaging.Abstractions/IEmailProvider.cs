namespace Kck.Messaging.Abstractions;

public interface IEmailProvider
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
