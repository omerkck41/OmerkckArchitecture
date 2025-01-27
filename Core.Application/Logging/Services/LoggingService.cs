using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Core.Application.Logging.Services;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    private string SerializeData(object? data)
    {
        if (data == null)
            return string.Empty;

        try
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
        catch
        {
            return "Failed to serialize data.";
        }
    }

    public void LogInfo(string message, object? data = null, EventId? eventId = null)
    {
        _logger.LogInformation(eventId ?? default, "{Message} - Data: {Data}", message, SerializeData(data));
    }

    public void LogWarning(string message, object? data = null, EventId? eventId = null)
    {
        _logger.LogWarning(eventId ?? default, "{Message} - Data: {Data}", message, SerializeData(data));
    }

    public void LogError(string message, Exception exception, object? data = null, EventId? eventId = null)
    {
        _logger.LogError(eventId ?? default, exception, "{Message} - Data: {Data}", message, SerializeData(data));
    }

    public void LogDebug(string message, object? data = null, EventId? eventId = null)
    {
        _logger.LogDebug(eventId ?? default, "{Message} - Data: {Data}", message, SerializeData(data));
    }

    public void LogTrace(string message, object? data = null, EventId? eventId = null)
    {
        _logger.LogTrace(eventId ?? default, "{Message} - Data: {Data}", message, SerializeData(data));
    }
}