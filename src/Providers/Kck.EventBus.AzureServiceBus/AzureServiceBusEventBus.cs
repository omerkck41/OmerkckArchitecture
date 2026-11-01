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
    private readonly bool _ownsClient;
    private ServiceBusClient? _client;
    private ServiceBusSender? _sender;

    /// <summary>
    /// Creates the bus with its own <see cref="ServiceBusClient"/>, built from
    /// <see cref="AzureServiceBusOptions.ConnectionString"/> on first use and disposed with the bus.
    /// </summary>
    public AzureServiceBusEventBus(
        AzureServiceBusOptions options,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusEventBus> logger)
        : this(options, serviceProvider, logger, client: null, ownsClient: true)
    {
    }

    /// <summary>
    /// Creates the bus on an existing <see cref="ServiceBusClient"/>, for example one registered in the container with
    /// a token credential (managed identity) or custom client options; <see cref="AzureServiceBusOptions.ConnectionString"/>
    /// is then not used. The client is not owned: <see cref="DisposeAsync"/> leaves it open.
    /// </summary>
    public AzureServiceBusEventBus(
        AzureServiceBusOptions options,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusEventBus> logger,
        ServiceBusClient client)
        : this(options, serviceProvider, logger, client ?? throw new ArgumentNullException(nameof(client)), ownsClient: false)
    {
    }

    internal AzureServiceBusEventBus(
        AzureServiceBusOptions options,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusEventBus> logger,
        ServiceBusClient? client,
        bool ownsClient)
    {
        _options = options;
        _logger = logger;
        _eventProcessor = new EventProcessor(serviceProvider, logger);
        _client = client;
        _ownsClient = ownsClient;
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken ct = default)
        where TEvent : IntegrationEvent
    {
        var eventName = typeof(TEvent).Name;

        await EnsureClientAsync(ct).ConfigureAwait(false);

        var body = JsonSerializer.SerializeToUtf8Bytes(eventData, JsonOptions);

        var message = new ServiceBusMessage(body)
        {
            MessageId = eventData.Id.ToString(),
            Subject = eventName,
            ContentType = "application/json",
            ApplicationProperties = { ["EventType"] = eventName }
        };

        await _sender!.SendMessageAsync(message, ct).ConfigureAwait(false);

        Log.Published(_logger, eventName, eventData.Id);
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

    // The sender is the readiness marker: it is created once, after the client exists (own or injected).
    private async Task EnsureClientAsync(CancellationToken ct)
    {
        if (Volatile.Read(ref _sender) is not null)
            return;

        await _clientLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (Volatile.Read(ref _sender) is not null)
                return;

            _client ??= CreateOwnClient();
            Volatile.Write(ref _sender, _client.CreateSender(_options.TopicName));

            Log.Connected(_logger, _options.TopicName);
        }
        finally
        {
            _clientLock.Release();
        }
    }

    private ServiceBusClient CreateOwnClient()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("Azure Service Bus connection string is not configured.");

        return new ServiceBusClient(
            _options.ConnectionString,
            new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = _options.RetryCount
                }
            });
    }

    /// <summary>
    /// Stops and disposes the processors and the sender, and the client when the bus created it; an injected client
    /// is left open for its owner.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        foreach (var processor in _processors)
        {
            await processor.StopProcessingAsync().ConfigureAwait(false);
            await processor.DisposeAsync().ConfigureAwait(false);
        }

        if (_sender is not null)
            await _sender.DisposeAsync().ConfigureAwait(false);
        if (_ownsClient && _client is not null)
            await _client.DisposeAsync().ConfigureAwait(false);
        _clientLock.Dispose();
    }
}
