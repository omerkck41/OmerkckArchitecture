using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Kck.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.EventBus.AzureServiceBus;

/// <summary>
/// Azure Service Bus implementation of <see cref="IEventBus"/> for distributed event publishing and subscribing.
/// </summary>
public sealed class AzureServiceBusEventBus : IEventBus, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AzureServiceBusOptions _options;
    private readonly ILogger<AzureServiceBusEventBus> _logger;
    private readonly EventProcessor _eventProcessor;
    private readonly ConcurrentDictionary<string, Type> _eventTypes = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<Type>> _handlers = new();
    private readonly SemaphoreSlim _clientLock = new(1, 1);
    private readonly List<ServiceBusProcessor> _processors = [];
    private ServiceBusClient? _client;
    private ServiceBusSender? _sender;

    public AzureServiceBusEventBus(
        AzureServiceBusOptions options,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusEventBus> logger)
    {
        _options = options;
        _logger = logger;
        _eventProcessor = new EventProcessor(serviceProvider, logger);
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IntegrationEvent
    {
        var eventName = typeof(TEvent).Name;

        await EnsureClientAsync(ct).ConfigureAwait(false);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event, JsonOptions);

        var message = new ServiceBusMessage(body)
        {
            MessageId = @event.Id.ToString(),
            Subject = eventName,
            ContentType = "application/json",
            ApplicationProperties = { ["EventType"] = eventName }
        };

        await _sender!.SendMessageAsync(message, ct).ConfigureAwait(false);

        Log.Published(_logger, eventName, @event.Id);
    }

    /// <inheritdoc />
    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        _eventTypes.TryAdd(eventName, typeof(TEvent));

        var handlerTypes = _handlers.GetOrAdd(eventName, _ => []);
        var handlerType = typeof(THandler);
        if (!handlerTypes.Contains(handlerType))
            handlerTypes.Add(handlerType);
    }

    internal async Task StartProcessingAsync(CancellationToken ct = default)
    {
        await EnsureClientAsync(ct).ConfigureAwait(false);

        foreach (var (eventName, _) in _eventTypes)
        {
            var processor = _client!.CreateProcessor(
                _options.TopicName,
                $"{_options.SubscriptionName}.{eventName}",
                new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = _options.MaxConcurrentCalls,
                    AutoCompleteMessages = false,
                    MaxAutoLockRenewalDuration = _options.MaxAutoLockRenewalDuration
                });

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    await _eventProcessor.ProcessAsync(eventName, args.Message.Body.ToArray(), _eventTypes, _handlers).ConfigureAwait(false);
                    await args.CompleteMessageAsync(args.Message, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Processor is shutting down; do not Abandon (which would use a cancelled token).
                    // Broker will redeliver the message via lock expiration.
                    throw;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Log.ProcessingError(_logger, ex, eventName);
                    await args.AbandonMessageAsync(args.Message, cancellationToken: ct).ConfigureAwait(false);
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                Log.ProcessorError(_logger, args.Exception, args.EntityPath, args.ErrorSource);
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(ct).ConfigureAwait(false);
            _processors.Add(processor);

            Log.StartedProcessing(_logger, eventName, _options.TopicName);
        }
    }

    private async Task EnsureClientAsync(CancellationToken ct)
    {
        if (_client is not null)
            return;

        await _clientLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_client is not null)
                return;

            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
                throw new InvalidOperationException("Azure Service Bus connection string is not configured.");

            _client = new ServiceBusClient(
                _options.ConnectionString,
                new ServiceBusClientOptions
                {
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        MaxRetries = _options.RetryCount
                    }
                });

            _sender = _client.CreateSender(_options.TopicName);

            Log.Connected(_logger, _options.TopicName);
        }
        finally
        {
            _clientLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var processor in _processors)
        {
            await processor.StopProcessingAsync().ConfigureAwait(false);
            await processor.DisposeAsync().ConfigureAwait(false);
        }

        if (_sender is not null)
            await _sender.DisposeAsync().ConfigureAwait(false);
        if (_client is not null)
            await _client.DisposeAsync().ConfigureAwait(false);
        _clientLock.Dispose();
    }
}
