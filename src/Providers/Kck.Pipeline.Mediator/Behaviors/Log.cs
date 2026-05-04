using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.Mediator.Behaviors;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {MessageName}")]
    public static partial void HandlingRequest(ILogger logger, string messageName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Long running request {MessageName} took {ElapsedMs}ms")]
    public static partial void LongRunningRequest(ILogger logger, string messageName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {MessageName} in {ElapsedMs}ms")]
    public static partial void HandledRequest(ILogger logger, string messageName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for {CacheKey}")]
    public static partial void CacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache set for {CacheKey}")]
    public static partial void CacheSet(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction started for {MessageName}")]
    public static partial void TransactionStarted(ILogger logger, string messageName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction committed for {MessageName}")]
    public static partial void TransactionCommitted(ILogger logger, string messageName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Transaction rolled back for {MessageName}")]
    public static partial void TransactionRolledBack(ILogger logger, string messageName);
}
