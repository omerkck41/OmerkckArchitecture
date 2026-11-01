using System.Collections.Concurrent;
using System.Text.Json;
using Kck.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Kck.EventBus.RabbitMq;

/// <summary>
/// RabbitMQ implementation of <see cref="IEventBus"/> for distributed event publishing and subscribing.
/// </summary>
public sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private const string DeadLetterExchangeArgument = "x-dead-letter-exchange";
    private const ushort PreconditionFailed = 406;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly EventProcessor _eventProcessor;
    private readonly DeliveryAttemptTracker _attempts = new(TimeProvider.System);
    private readonly ConcurrentDictionary<string, Type> _eventTypes = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<Type>> _handlers = new();
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _publishChannel;
    private IChannel? _consumeChannel;
    private CancellationToken _stoppingToken;
    private int _disposed;

    internal bool IsConnected => _connection is { IsOpen: true };

    /// <summary>
    /// Ready to consume: the connection is open and, when events are subscribed, the consumer channel is open
    /// (it is <see langword="null"/> until <see cref="StartConsumingAsync"/> ran).
    /// </summary>
    internal bool IsReady => _connection is { IsOpen: true } && (_eventTypes.IsEmpty || _consumeChannel is { IsOpen: true });

    internal IChannel? PublishChannel => _publishChannel;

    internal IChannel? ConsumeChannel => _consumeChannel;

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
    public async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken ct = default)
        where TEvent : IntegrationEvent
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        var eventName = typeof(TEvent).Name;

        var channel = await GetPublishChannelAsync(ct).ConfigureAwait(false);

        var body = JsonSerializer.SerializeToUtf8Bytes(eventData, JsonOptions);

        var props = new BasicProperties
        {
            MessageId = eventData.Id.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: eventName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct).ConfigureAwait(false);

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

    /// <summary>
    /// Connects, declares the topology and starts one consumer per subscribed event on a dedicated channel.
    /// </summary>
    /// <remarks>
    /// The client's automatic recovery restores the channel and its consumers after a connection loss. A channel
    /// closed by a channel-level error is not recovered by the client; that shutdown is logged and the health check
    /// reports Unhealthy. <paramref name="ct"/> is the host's stopping token: a handler's
    /// <see cref="OperationCanceledException"/> leaves the message unacknowledged only while it is canceled.
    /// </remarks>
    internal async Task StartConsumingAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        _stoppingToken = ct;
        await EnsureConnectionAsync(ct).ConfigureAwait(false);

        var channel = await _connection!.CreateChannelAsync(cancellationToken: ct).ConfigureAwait(false);
        _consumeChannel = channel;
        channel.ChannelShutdownAsync += OnConsumeChannelShutdownAsync;

        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: _options.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: ct).ConfigureAwait(false);

        Dictionary<string, object?>? queueArguments = null;
        if (_options.DeadLetterEnabled)
        {
            await DeclareDeadLetterTopologyAsync(channel, ct).ConfigureAwait(false);
            queueArguments = new Dictionary<string, object?>
            {
                [DeadLetterExchangeArgument] = _options.EffectiveDeadLetterExchangeName
            };
        }

        if (_options.PrefetchCount > 0)
        {
            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: _options.PrefetchCount,
                global: false,
                cancellationToken: ct).ConfigureAwait(false);
        }

        foreach (var (eventName, _) in _eventTypes)
        {
            var queueName = $"{_options.QueuePrefix}.{eventName}";

            await DeclareConsumerQueueAsync(channel, queueName, queueArguments, ct).ConfigureAwait(false);

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: _options.ExchangeName,
                routingKey: eventName,
                cancellationToken: ct).ConfigureAwait(false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, ea) => HandleDeliveryAsync(channel, eventName, ea);

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: ct).ConfigureAwait(false);

            Log.Subscribed(_logger, eventName, queueName);
        }
    }

    private async Task HandleDeliveryAsync(IChannel channel, string eventName, BasicDeliverEventArgs ea)
    {
        var messageId = ea.BasicProperties.MessageId;

        try
        {
            await _eventProcessor.ProcessAsync(eventName, ea.Body.ToArray(), _eventTypes, _handlers).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_stoppingToken.IsCancellationRequested)
        {
            // Consumer is shutting down; do not Ack/Nack. Broker will redeliver unacked message after channel close.
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException
                                and not StackOverflowException)
        {
            // Any other OperationCanceledException (for example an HttpClient timeout) is a handler failure.
            Log.ProcessingError(_logger, ex, eventName);

            // MaxDeliveryCount counts every delivery including the first: Max = 3 means 3 handler invocations.
            var attempt = ResolveDeliveryAttempt(messageId, ea.Redelivered, _options.MaxDeliveryCount, _attempts);
            var requeue = attempt < _options.MaxDeliveryCount;
            // Deliberately not cancellable: the broker must learn the outcome of this delivery even while the host
            // is stopping, otherwise the message stays unacknowledged until the channel closes.
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: requeue, cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);

            if (!requeue)
            {
                if (!string.IsNullOrEmpty(messageId))
                    _attempts.Forget(messageId);

                if (_options.DeadLetterEnabled)
                    Log.MessageDeadLettered(_logger, messageId, attempt, _options.EffectiveDeadLetterExchangeName);
                else
                    Log.MessageRejectedWithoutDeadLetter(_logger, messageId, attempt);
            }

            return;
        }

        // Outside the try: a failed ack (for example on a closed channel) is not a handler failure. Not cancellable for
        // the same reason as the nack above: a handled message must be acknowledged even during shutdown.
        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(messageId))
            _attempts.Forget(messageId);
    }

    /// <summary>
    /// Attempt number of a failed delivery, the first delivery being 1.
    /// </summary>
    /// <remarks>
    /// A requeued nack adds no x-death header and a classic queue does not count redeliveries (any
    /// <c>x-delivery-count</c> header on it was set by the publisher), so a message with a <c>MessageId</c> is counted
    /// in process. Without one there is nothing to count on: the broker's Redelivered flag marks the last delivery,
    /// so such a message is delivered at most twice.
    /// </remarks>
    internal static int ResolveDeliveryAttempt(
        string? messageId,
        bool redelivered,
        int maxDeliveryCount,
        DeliveryAttemptTracker tracker)
    {
        if (!string.IsNullOrEmpty(messageId))
            return tracker.Record(messageId);

        return redelivered ? maxDeliveryCount : 1;
    }

    private async Task DeclareDeadLetterTopologyAsync(IChannel channel, CancellationToken ct)
    {
        var exchange = _options.EffectiveDeadLetterExchangeName;
        var queue = _options.EffectiveDeadLetterQueueName;

        await channel.ExchangeDeclareAsync(
            exchange: exchange,
            type: "fanout",
            durable: true,
            autoDelete: false,
            cancellationToken: ct).ConfigureAwait(false);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct).ConfigureAwait(false);

        await channel.QueueBindAsync(
            queue: queue,
            exchange: exchange,
            routingKey: string.Empty,
            cancellationToken: ct).ConfigureAwait(false);

        Log.DeadLetterTopologyDeclared(_logger, exchange, queue);
    }

    private async Task DeclareConsumerQueueAsync(
        IChannel channel,
        string queueName,
        Dictionary<string, object?>? arguments,
        CancellationToken ct)
    {
        try
        {
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments,
                cancellationToken: ct).ConfigureAwait(false);
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == PreconditionFailed)
        {
            throw new InvalidOperationException(DescribeInequivalentQueue(queueName), ex);
        }
    }

    private string DescribeInequivalentQueue(string queueName)
    {
        var declared = _options.DeadLetterEnabled
            ? $"Kck 5.0 declares consumer queues with 'x-dead-letter-exchange' = '{_options.EffectiveDeadLetterExchangeName}'. "
              + "Remedies, safest first: (1) use a new QueuePrefix and keep the old consumers until the old queue is empty, or "
              + "(2) set RabbitMqOptions.DeadLetterEnabled = false and apply a dead-letter exchange through a broker policy, or "
              + "(3) delete the queue so it is re-declared (messages in it, and messages published before the restart, are lost). "
              + "See docs/migrations/v4-to-v5.md."
            : "RabbitMqOptions.DeadLetterEnabled is false, so the consumer queue is declared without arguments. "
              + "Remedies, safest first: (1) use a new QueuePrefix and keep the old consumers until the old queue is empty, or "
              + "(2) set RabbitMqOptions.DeadLetterEnabled = true if the queue was declared with Kck's dead-letter exchange, or "
              + "(3) delete the queue so it is re-declared (messages in it, and messages published before the restart, are lost).";

        return $"RabbitMQ queue '{queueName}' already exists with different arguments (406 PRECONDITION_FAILED). {declared}";
    }

    private Task OnConsumeChannelShutdownAsync(object sender, ShutdownEventArgs args)
    {
        // Closing the bus closes the channel too; only an unexpected shutdown is worth a warning.
        if (args.Initiator != ShutdownInitiator.Application)
            Log.ConsumeChannelShutdown(_logger, DescribeShutdown(args));

        return Task.CompletedTask;
    }

    private async Task<IChannel> GetPublishChannelAsync(CancellationToken ct)
    {
        if (_publishChannel is { IsOpen: true } open)
            return open;

        await _connectionLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_publishChannel is { IsOpen: true } current)
                return current;

            var connection = await EnsureConnectionCoreAsync(ct).ConfigureAwait(false);

            // Automatic recovery reopens the channel together with the connection.
            if (_publishChannel is { IsOpen: true } recovered)
                return recovered;

            // A channel-level error (e.g. 404 on a passive declare, 406) closes the channel and the client does not
            // reopen it: replace it with a new one.
            var previous = _publishChannel;
            var previousCloseReason = previous is null ? null : DescribeShutdown(previous.CloseReason);
            if (previous is not null)
                await previous.DisposeAsync().ConfigureAwait(false);

            var channel = await connection.CreateChannelAsync(cancellationToken: ct).ConfigureAwait(false);
            try
            {
                await channel.ExchangeDeclareAsync(
                    exchange: _options.ExchangeName,
                    type: _options.ExchangeType,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: ct).ConfigureAwait(false);
            }
            catch
            {
                await channel.DisposeAsync().ConfigureAwait(false);
                throw;
            }

            _publishChannel = channel;
            if (previousCloseReason is not null)
                Log.PublishChannelReopened(_logger, previousCloseReason);

            return channel;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task EnsureConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true })
            return;

        await _connectionLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await EnsureConnectionCoreAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    // Caller holds _connectionLock.
    private async Task<IConnection> EnsureConnectionCoreAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true } existing)
            return existing;

        // A connection that exists but is closed is being recovered by the client: opening a second one would leave
        // two connections (and the consumers on the recovered one) once recovery succeeds.
        if (_connection is { } recovering)
            return await WaitForRecoveryAsync(recovering, ct).ConfigureAwait(false);

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true
        };

        Exception? lastException = null;
        for (var i = 0; i < _options.RetryCount; i++)
        {
            try
            {
                var connection = await factory.CreateConnectionAsync(ct).ConfigureAwait(false);
                _connection = connection;

                Log.Connected(_logger, _options.HostName, _options.Port);
                return connection;
            }
            catch (Exception ex) when (i < _options.RetryCount - 1)
            {
                lastException = ex;
                var delay = RetryHelper.CalculateDelay(i, _options.RetryDelay, _options.MaxRetryDelay);
                Log.ConnectionRetry(_logger, ex, i + 1, _options.RetryCount, delay.TotalSeconds);
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException
                                    and not OutOfMemoryException
                                    and not StackOverflowException)
            {
                lastException = ex;
            }
        }

        throw new InvalidOperationException(
            $"Failed to connect to RabbitMQ at {_options.HostName}:{_options.Port} after {_options.RetryCount} attempts.",
            lastException);
    }

    // Caller holds _connectionLock. Waits on the connection retry schedule; the client keeps recovering on its own.
    private async Task<IConnection> WaitForRecoveryAsync(IConnection connection, CancellationToken ct)
    {
        for (var i = 0; i < _options.RetryCount; i++)
        {
            var delay = RetryHelper.CalculateDelay(i, _options.RetryDelay, _options.MaxRetryDelay);
            Log.WaitingForConnectionRecovery(_logger, i + 1, _options.RetryCount, delay.TotalSeconds);
            await Task.Delay(delay, ct).ConfigureAwait(false);

            if (connection.IsOpen)
                return connection;
        }

        throw new InvalidOperationException(
            $"RabbitMQ connection to {_options.HostName}:{_options.Port} is down and did not recover within "
            + $"{_options.RetryCount} attempts; the client keeps recovering in the background.");
    }

    private static string DescribeShutdown(ShutdownEventArgs? reason) =>
        reason is null ? "unknown reason" : $"{reason.ReplyCode} {reason.ReplyText}";

    /// <summary>
    /// Disposes the channels and the connection; later <see cref="PublishAsync{TEvent}"/> calls throw
    /// <see cref="ObjectDisposedException"/>.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await DisposeQuietlyAsync(_publishChannel).ConfigureAwait(false);
        await DisposeQuietlyAsync(_consumeChannel).ConfigureAwait(false);
        await DisposeQuietlyAsync(_connection).ConfigureAwait(false);
        _connectionLock.Dispose();
    }

    private static async ValueTask DisposeQuietlyAsync(IAsyncDisposable? resource)
    {
        if (resource is null)
            return;

        try
        {
            await resource.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is AlreadyClosedException
                                   or OperationInterruptedException
                                   or ObjectDisposedException)
        {
            // Already closed (by the broker, a channel-level error or a failed recovery): nothing left to release.
        }
    }
}
