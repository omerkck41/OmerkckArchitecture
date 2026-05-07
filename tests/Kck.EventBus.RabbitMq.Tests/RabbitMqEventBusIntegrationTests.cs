using FluentAssertions;
using Kck.EventBus.Abstractions;
using Kck.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.RabbitMq;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

/// <summary>
/// LS-FAZ-4.5: Real RabbitMQ container integration tests.
/// Verifies that the event bus can connect, declare topology, and publish
/// without errors against a real broker.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter).
/// RabbitMqContainer default credentials: guest / guest.
/// </remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001
public sealed class RabbitMqEventBusIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .Build();

    private RabbitMqEventBus _bus = default!;

    public async Task InitializeAsync()
    {
        await _rabbit.StartAsync();

        var options = new RabbitMqOptions
        {
            HostName = _rabbit.Hostname,
            Port = _rabbit.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest",
            ExchangeName = "kck.test",
            RetryCount = 2,
            RetryDelay = TimeSpan.FromMilliseconds(200)
        };

        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        _bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
    }

    public async Task DisposeAsync()
    {
        await _bus.DisposeAsync();
        await _rabbit.DisposeAsync();
    }

    [Fact]
    public async Task PublishAsync_ConnectsAndPublishesWithoutException()
    {
        var @event = new RabbitIntegrationTestEvent("order-42");

        var act = async () => await _bus.PublishAsync(@event);

        await act.Should().NotThrowAsync("a real broker is running");
    }

    [Fact]
    public void Subscribe_RegistersHandlerWithoutException()
    {
        var act = () => _bus.Subscribe<RabbitIntegrationTestEvent, RabbitIntegrationTestEventHandler>();

        act.Should().NotThrow();
    }
}

internal sealed record RabbitIntegrationTestEvent(string Payload) : IntegrationEvent;

internal sealed class RabbitIntegrationTestEventHandler : IEventHandler<RabbitIntegrationTestEvent>
{
    public Task HandleAsync(RabbitIntegrationTestEvent @event, CancellationToken ct = default)
        => Task.CompletedTask;
}
