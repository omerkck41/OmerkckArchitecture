using Microsoft.Extensions.Logging;

namespace Core.Application.Logging.Services;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public void LogInfo(string message, object? data = null)
    {
        _logger.LogInformation("{Message} - Data: {@Data}", message, data);
    }

    public void LogWarning(string message, object? data = null)
    {
        _logger.LogWarning("{Message} - Data: {@Data}", message, data);
    }

    public void LogError(string message, Exception exception, object? data = null)
    {
        _logger.LogError(exception, "{Message} - Data: {@Data}", message, data);
    }

    public void LogDebug(string message, object? data = null)
    {
        _logger.LogDebug("{Message} - Data: {@Data}", message, data);
    }
}