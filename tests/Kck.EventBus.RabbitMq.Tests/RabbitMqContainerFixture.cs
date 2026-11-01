using Kck.EventBus.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Testcontainers.RabbitMq;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

/// <summary>
/// One RabbitMQ container shared by every test of a class, plus a separate raw client connection used to
/// inspect queues (message counts, queue arguments) independently of the event bus under test.
/// </summary>
#pragma warning disable CA1001 // disposed through IAsyncLifetime.DisposeAsync
public sealed class RabbitMqContainerFixture : IAsyncLifetime
#pragma warning restore CA1001
{
    private const string BrokerUser = "kck_test";

    // Generated per run: the throw-away container needs a non-guest user ('guest' is loopback-only inside the
    // container), and no credential is written into the source.
    private readonly string _brokerSecret = Guid.NewGuid().ToString("N");

    private readonly RabbitMqContainer _container;
    private IConnection? _inspection;

    public RabbitMqContainerFixture()
    {
        // rabbitmq:3-management provides the management API used by the Testcontainers health check.
        _container = new RabbitMqBuilder("rabbitmq:3-management")
            .WithUsername(BrokerUser)
            .WithPassword(_brokerSecret)
            .Build();
    }

    public string Hostname => _container.Hostname;

    public int Port => _container.GetMappedPublicPort(5672);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var factory = new ConnectionFactory
        {
            HostName = Hostname,
            Port = Port,
            UserName = BrokerUser,
            Password = _brokerSecret
        };
        _inspection = await factory.CreateConnectionAsync();
    }

    public async Task DisposeAsync()
    {
        if (_inspection is not null)
            await _inspection.DisposeAsync();
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Options pointing at the container; exchange and queue names are unique per test so tests never share
    /// broker topology.
    /// </summary>
    public RabbitMqOptions CreateOptions(string testName, Action<RabbitMqOptions>? configure = null)
    {
        var options = new RabbitMqOptions
        {
            HostName = Hostname,
            Port = Port,
            UserName = BrokerUser,
            Password = _brokerSecret,
            ExchangeName = $"kck.test.{testName}",
            QueuePrefix = $"kck.{testName}",
            RetryCount = 3,
            RetryDelay = TimeSpan.FromMilliseconds(200)
        };
        configure?.Invoke(options);
        return options;
    }

    /// <summary>
    /// Ready message count of <paramref name="queue"/>, or <see langword="null"/> when the queue does not exist.
    /// </summary>
    public async Task<uint?> GetMessageCountAsync(string queue)
    {
        var inspection = await GetOpenInspectionAsync();
        var channel = await inspection.CreateChannelAsync();
        try
        {
            var ok = await channel.QueueDeclarePassiveAsync(queue);
            return ok.MessageCount;
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
        {
            return null;
        }
        finally
        {
            await channel.DisposeAsync();
        }
    }

    /// <summary>
    /// Declares a durable queue with the given arguments; throws when the queue exists with other arguments.
    /// </summary>
    public async Task DeclareQueueAsync(string queue, IDictionary<string, object?>? arguments)
    {
        var inspection = await GetOpenInspectionAsync();
        var channel = await inspection.CreateChannelAsync();
        try
        {
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
        }
        finally
        {
            await channel.DisposeAsync();
        }
    }

    /// <summary>
    /// Publishes a message through the inspection connection, bypassing the event bus, so a test controls the
    /// headers and the <c>MessageId</c> exactly.
    /// </summary>
    public async Task PublishRawAsync(
        string exchange,
        string routingKey,
        byte[] body,
        IDictionary<string, object?>? headers,
        string? messageId)
    {
        var inspection = await GetOpenInspectionAsync();
        var channel = await inspection.CreateChannelAsync();
        try
        {
            var props = new BasicProperties
            {
                MessageId = messageId,
                Headers = headers,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };
            await channel.BasicPublishAsync(exchange, routingKey, mandatory: false, basicProperties: props, body: body);
        }
        finally
        {
            await channel.DisposeAsync();
        }
    }

    /// <summary>
    /// Closes every AMQP connection of the broker (<c>rabbitmqctl close_all_connections</c>), the inspection
    /// connection included; clients with automatic recovery reconnect on their own.
    /// </summary>
    public async Task CloseAllConnectionsAsync()
    {
        await GetOpenInspectionAsync();
        var result = await _container.ExecAsync(["rabbitmqctl", "close_all_connections", "kck-test-forced-close"]);
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"rabbitmqctl close_all_connections failed: {result.Stderr}");
    }

    /// <summary>Number of AMQP connections the broker currently has (<c>rabbitmqctl list_connections</c>).</summary>
    public async Task<int> CountConnectionsAsync()
    {
        await GetOpenInspectionAsync();
        // -q drops the "Listing connections" banner but not the column header row.
        var result = await _container.ExecAsync(["rabbitmqctl", "list_connections", "name", "-q", "--no-table-headers"]);
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"rabbitmqctl list_connections failed: {result.Stderr}");

        return result.Stdout
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Length;
    }

    // close_all_connections also closes the inspection connection; it recovers automatically, so every helper waits
    // for it first.
    private async Task<IConnection> GetOpenInspectionAsync()
    {
        var inspection = _inspection ?? throw new InvalidOperationException("The fixture is not initialized.");
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(20);
        while (!inspection.IsOpen && DateTime.UtcNow < deadline)
            await Task.Delay(100);

        return inspection.IsOpen
            ? inspection
            : throw new InvalidOperationException("The inspection connection did not recover within 20 seconds.");
    }
}
