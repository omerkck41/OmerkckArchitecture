namespace Core.Application.Logging.Services;

public interface ILoggingService
{
    void LogInfo(string message, object? data = null);
    void LogWarning(string message, object? data = null);
    void LogError(string message, Exception exception, object? data = null);
    void LogDebug(string message, object? data = null);
}