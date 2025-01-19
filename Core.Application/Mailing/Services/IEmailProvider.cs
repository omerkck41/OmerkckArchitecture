namespace Core.Application.Mailing.Services;

public interface IEmailProvider
{
    Task SendAsync(EmailMessage emailMessage);
}