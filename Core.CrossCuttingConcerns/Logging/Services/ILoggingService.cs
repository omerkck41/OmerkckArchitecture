using Microsoft.Extensions.Logging;

namespace Core.CrossCuttingConcerns.Logging.Services;

public interface ILoggingService
{
    void LogInfo(string message, object? data = null, EventId? eventId = null);
    void LogWarning(string message, object? data = null, EventId? eventId = null);
    void LogError(string message, Exception exception, object? data = null, EventId? eventId = null);
    void LogDebug(string message, object? data = null, EventId? eventId = null);
    void LogTrace(string message, object? data = null, EventId? eventId = null);
}