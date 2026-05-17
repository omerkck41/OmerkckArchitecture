using FluentAssertions;
using Kck.EventBus.Abstractions;
using Kck.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.RabbitMq;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

/// <summary>
/// LS-FAZ-4.5: RabbitMQ container integration tests.
/// Tests DI setup and handler registration via a live broker container.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter).
/// PublishAsync requires a fully-ready AMQP broker; Subscribe validates
/// in-process handler registration without a network call.
/// </remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001
public sealed class RabbitMqEventBusIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private const string RabbitUser = "kck_test";
    private const string RabbitPass = "kck_test_pw";

    // rabbitmq:3-management provides the management API used by the Testcontainers health check.
    // 'guest' is loopback-only inside the container; explicit credentials avoid auth rejection.
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:3-management")
        .WithUsername(RabbitUser)
        .WithPassword(RabbitPass)
        .Build();

    private RabbitMqEventBus _bus = default!;

    public async Task InitializeAsync()
    {
        await _rabbit.StartAsync();

        var options = new RabbitMqOptions
        {
            HostName = _rabbit.Hostname,
            Port = _rabbit.GetMappedPublicPort(5672),
            UserName = RabbitUser,
            Password = RabbitPass,
            ExchangeName = "kck.test"
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
    public void Subscribe_RegistersHandlerWithoutException()
    {
        var act = () => _bus.Subscribe<RabbitIntegrationTestEvent, RabbitIntegrationTestEventHandler>();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task PublishAsync_WithSubscribedHandler_DoesNotThrow()
    {
        _bus.Subscribe<RabbitIntegrationTestEvent, RabbitIntegrationTestEventHandler>();
        var @event = new RabbitIntegrationTestEvent("publish-integration-test");

        var act = async () => await _bus.PublishAsync(@event);

        await act.Should().NotThrowAsync();
    }
}

internal sealed record RabbitIntegrationTestEvent(string Payload) : IntegrationEvent;

internal sealed class RabbitIntegrationTestEventHandler : IEventHandler<RabbitIntegrationTestEvent>
{
    public Task HandleAsync(RabbitIntegrationTestEvent @event, CancellationToken ct = default)
        => Task.CompletedTask;
}
