using Core.Application.Mailing.Services;
using MediatR;

namespace Core.Application.Mailing.Behaviors;

public class EmailSendingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IMailService _mailService;

    public EmailSendingBehavior(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is EmailMessage emailMessage)
            await _mailService.SendEmailAsync(emailMessage);

        return response;
    }
}
