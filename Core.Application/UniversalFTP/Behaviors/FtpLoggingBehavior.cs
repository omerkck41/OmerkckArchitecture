using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.UniversalFTP.Behaviors;

public class FtpLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<FtpLoggingBehavior<TRequest, TResponse>> _logger;

    public FtpLoggingBehavior(ILogger<FtpLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling FTP request: {Request}", typeof(TRequest).Name);

        var response = await next();

        _logger.LogInformation("Handled FTP request: {Request}", typeof(TRequest).Name);

        return response;
    }
}