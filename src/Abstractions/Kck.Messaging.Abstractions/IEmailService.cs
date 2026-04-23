namespace Kck.Messaging.Abstractions;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
