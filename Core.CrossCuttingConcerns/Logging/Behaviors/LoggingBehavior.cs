using Core.CrossCuttingConcerns.Logging.Services;
using MediatR;
using System.Diagnostics;

namespace Core.CrossCuttingConcerns.Logging.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ILoggableRequest
{
    private readonly ILoggingService _loggingService;

    public LoggingBehavior(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        var stopwatch = Stopwatch.StartNew();
        _loggingService.LogInfo($"Handling {requestName}", request);

        try
        {
            var response = await next();
            stopwatch.Stop();

            _loggingService.LogInfo($"Handled {requestName} in {stopwatch.ElapsedMilliseconds}ms", response);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _loggingService.LogError($"Error occurred while handling {requestName}. Elapsed Time: {stopwatch.ElapsedMilliseconds}ms", ex, request);
            throw;
        }
    }
}