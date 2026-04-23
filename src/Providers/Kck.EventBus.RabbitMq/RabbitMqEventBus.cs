using System.Collections.Concurrent;
using System.Text.Json;
using Kck.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Kck.EventBus.RabbitMq;

/// <summary>
/// RabbitMQ implementation of <see cref="IEventBus"/> for distributed event publishing and subscribing.
/// </summary>
public sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly EventProcessor _eventProcessor;
    private readonly ConcurrentDictionary<string, Type> _eventTypes = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<Type>> _handlers = new();
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _publishChannel;
    private IChannel? _consumeChannel;

    public RabbitMqEventBus(
        RabbitMqOptions options,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqEventBus> logger)
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

        await EnsureConnectionAsync(ct).ConfigureAwait(false);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event, JsonOptions);

        var props = new BasicProperties
        {
            MessageId = @event.Id.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _publishChannel!.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: eventName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct).ConfigureAwait(false);

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

    internal async Task StartConsumingAsync(CancellationToken ct = default)
    {
        await EnsureConnectionAsync(ct).ConfigureAwait(false);

        foreach (var (eventName, _) in _eventTypes)
        {
            var queueName = $"{_options.QueuePrefix}.{eventName}";

            await _consumeChannel!.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct).ConfigureAwait(false);

            await _consumeChannel.QueueBindAsync(
                queue: queueName,
                exchange: _options.ExchangeName,
                routingKey: eventName,
                cancellationToken: ct).ConfigureAwait(false);

            var consumer = new AsyncEventingBasicConsumer(_consumeChannel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    await _eventProcessor.ProcessAsync(eventName, ea.Body.ToArray(), _eventTypes, _handlers).ConfigureAwait(false);
                    await _consumeChannel.BasicAckAsync(ea.DeliveryTag, multiple: false).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Consumer is shutting down; do not Ack/Nack. Broker will redeliver unacked message after channel close.
                    throw;
                }
                catch (Exception ex)
                {
                    Log.ProcessingError(_logger, ex, eventName);

                    var requeue = GetDeathCount(ea.BasicProperties) < _options.MaxDeliveryCount;
                    await _consumeChannel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: requeue)
                        .ConfigureAwait(false);

                    if (!requeue)
                        Log.MessageDeadLettered(_logger, ea.BasicProperties.MessageId);
                }
            };

            await _consumeChannel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: ct).ConfigureAwait(false);

            Log.Subscribed(_logger, eventName, queueName);
        }
    }

    private async Task EnsureConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true })
            return;

        await _connectionLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_connection is { IsOpen: true })
                return;

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            Exception? lastException = null;
            for (var i = 0; i < _options.RetryCount; i++)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync(ct).ConfigureAwait(false);
                    _publishChannel = await _connection.CreateChannelAsync(cancellationToken: ct).ConfigureAwait(false);
                    _consumeChannel = await _connection.CreateChannelAsync(cancellationToken: ct).ConfigureAwait(false);

                    await _publishChannel.ExchangeDeclareAsync(
                        exchange: _options.ExchangeName,
                        type: _options.ExchangeType,
                        durable: true,
                        autoDelete: false,
                        cancellationToken: ct).ConfigureAwait(false);

                    Log.Connected(_logger, _options.HostName, _options.Port);
                    return;
                }
                catch (Exception ex) when (i < _options.RetryCount - 1)
                {
                    lastException = ex;
                    var delay = RetryHelper.CalculateDelay(i, _options.RetryDelay, _options.MaxRetryDelay);
                    Log.ConnectionRetry(_logger, ex, i + 1, _options.RetryCount, delay.TotalSeconds);
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw new InvalidOperationException(
                $"Failed to connect to RabbitMQ at {_options.HostName}:{_options.Port} after {_options.RetryCount} attempts.",
                lastException);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private static long GetDeathCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is null || !properties.Headers.TryGetValue("x-death", out var deathObj))
            return 0;

        if (deathObj is List<object> deathList && deathList.Count > 0 &&
            deathList[0] is Dictionary<string, object> deathEntry &&
            deathEntry.TryGetValue("count", out var countObj) &&
            countObj is long count)
            return count;

        return 0;
    }

    public async ValueTask DisposeAsync()
    {
        if (_publishChannel is not null)
            await _publishChannel.CloseAsync().ConfigureAwait(false);
        if (_consumeChannel is not null)
            await _consumeChannel.CloseAsync().ConfigureAwait(false);
        if (_connection is not null)
            await _connection.CloseAsync().ConfigureAwait(false);
        _connectionLock.Dispose();
    }
}
