using Core.Application.Mailing.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Mailing.Behaviors;

public class EmailSendingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IMailService _mailService;
    private readonly ILogger<EmailLoggingBehavior<TRequest, TResponse>> _logger;

    public EmailSendingBehavior(IMailService mailService, ILogger<EmailLoggingBehavior<TRequest, TResponse>> logger)
    {
        _mailService = mailService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        // Eğer request IEmailRequest implemente ediyorsa e-posta gönder
        if (request is IEmailRequest emailRequest)
        {
            var emailMessage = emailRequest.GetEmailMessage();

            try
            {
                _logger.LogInformation("Sending email to: {Recipients}", string.Join(", ", emailMessage.Recipients.Select(r => r.Email)));
                await _mailService.SendEmailAsync(emailMessage);
                _logger.LogInformation("Email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to: {Recipients}", string.Join(", ", emailMessage.Recipients.Select(r => r.Email)));
                throw; // Hata yeniden fırlatılıyor
            }
        }

        return response;
    }
}