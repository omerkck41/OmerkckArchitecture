using Core.Application.UniversalFTP.Helper;
using MediatR;

namespace Core.Application.UniversalFTP.Behaviors;

public class FtpRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        const int retryCount = 3;
        TimeSpan delay = TimeSpan.FromSeconds(2);

        return await RetryHelper.RetryAsync(async () =>
        {
            return await next();
        }, retryCount, delay);
    }
}