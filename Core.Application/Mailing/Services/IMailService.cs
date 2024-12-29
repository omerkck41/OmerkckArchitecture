namespace Core.Application.Mailing.Services;

public interface IMailService
{
    Task SendEmailAsync(EmailMessage emailMessage);
}