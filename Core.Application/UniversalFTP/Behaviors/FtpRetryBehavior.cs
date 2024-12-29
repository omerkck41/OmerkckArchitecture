using Core.Application.UniversalFTP.Helper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.UniversalFTP.Behaviors;

public class FtpRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<FtpRetryBehavior<TRequest, TResponse>> _logger;

    public FtpRetryBehavior(ILogger<FtpRetryBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        const int retryCount = 3;
        TimeSpan delay = TimeSpan.FromSeconds(2);

        return await RetryHelper.RetryAsync(async () =>
        {
            _logger.LogInformation("Retry attempt for request: {Request}", typeof(TRequest).Name);
            return await next();
        }, retryCount, delay);
    }
}