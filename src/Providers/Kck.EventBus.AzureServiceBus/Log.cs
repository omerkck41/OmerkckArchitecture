using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Kck.EventBus.AzureServiceBus;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Published {EventName} with Id {EventId}")]
    public static partial void Published(ILogger logger, string eventName, Guid eventId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error processing {EventName}")]
    public static partial void ProcessingError(ILogger logger, Exception exception, string eventName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Service Bus processor error on {EntityPath}: {ErrorSource}")]
    public static partial void ProcessorError(ILogger logger, Exception exception, string entityPath, ServiceBusErrorSource errorSource);

    [LoggerMessage(Level = LogLevel.Information, Message = "Started processing {EventName} on topic {TopicName}")]
    public static partial void StartedProcessing(ILogger logger, string eventName, string topicName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connected to Azure Service Bus topic {TopicName}")]
    public static partial void Connected(ILogger logger, string topicName);
}
