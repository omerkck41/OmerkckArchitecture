using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Mailing.Behaviors;

public class EmailLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<EmailLoggingBehavior<TRequest, TResponse>> _logger;

    public EmailLoggingBehavior(ILogger<EmailLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling email request: {Request}", request);

        try
        {
            var response = await next();
            _logger.LogInformation("Successfully handled email request. Response: {Response}", response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling the email request: {Request}", request);
            throw; // Hata yeniden fırlatılıyor, böylece diğer behavior'lar veya handler'lar da bu hatayı işleyebilir.
        }
    }
}