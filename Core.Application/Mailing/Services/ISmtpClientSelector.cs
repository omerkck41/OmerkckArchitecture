using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public interface ISmtpClientSelector
{
    SmtpClient GetNextClient();
    void RegisterSend(SmtpClient smtpClient);
}