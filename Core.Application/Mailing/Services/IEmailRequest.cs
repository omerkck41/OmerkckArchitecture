namespace Core.Application.Mailing.Services;

public interface IEmailRequest
{
    EmailMessage GetEmailMessage();
}