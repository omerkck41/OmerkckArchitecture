using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.MediatR.Behaviors;

internal static partial class Log
{
    // LoggingBehavior
    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    public static partial void HandlingRequest(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Long running request {RequestName} took {ElapsedMs}ms")]
    public static partial void LongRunningRequest(ILogger logger, string requestName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs}ms")]
    public static partial void HandledRequest(ILogger logger, string requestName, long elapsedMs);

    // CachingBehavior
    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for {CacheKey}")]
    public static partial void CacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache set for {CacheKey}")]
    public static partial void CacheSet(ILogger logger, string cacheKey);

    // TransactionBehavior
    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction started for {RequestName}")]
    public static partial void TransactionStarted(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction committed for {RequestName}")]
    public static partial void TransactionCommitted(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Transaction rolled back for {RequestName}")]
    public static partial void TransactionRolledBack(ILogger logger, string requestName);
}
