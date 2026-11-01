using AwesomeAssertions;
using Kck.EventBus.Abstractions;
using Kck.EventBus.RabbitMq;
using Kck.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

/// <summary>
/// Connection loss against a live broker. A class of its own (own container): it closes every connection of the
/// broker and counts the connections, so no other test may share the broker.
/// </summary>
/// <remarks>Requires Docker. CI: ubuntu-only (Category=Integration filter).</remarks>
[Trait("Category", "Integration")]
public sealed class RabbitMqConnectionRecoveryIntegrationTests(RabbitMqContainerFixture rabbit)
    : IClassFixture<RabbitMqContainerFixture>
{
    [DockerFact]
    public async Task PublishAsync_DuringConnectionRecovery_WaitsForTheRecoveredConnection_AndOpensNoSecondConnection()
    {
        var counter = new Counter();
        var options = rabbit.CreateOptions("recovery", o =>
        {
            o.RetryCount = 5;
            o.RetryDelay = TimeSpan.FromSeconds(1);
        });
        await using var services = IntegrationTestHelpers.BuildServices(s => s
            .AddSingleton(counter)
            .AddTransient<SucceedingHandler<RecoveryEvent>>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<RecoveryEvent, SucceedingHandler<RecoveryEvent>>();
        await bus.StartConsumingAsync();

        await rabbit.CloseAllConnectionsAsync();
        (await IntegrationTestHelpers.WaitUntilAsync(() => Task.FromResult(!bus.IsConnected), TimeSpan.FromSeconds(5)))
            .Should().BeTrue("the broker closed the bus' connection");

        var publish = () => bus.PublishAsync(new RecoveryEvent("after-recovery"));

        await publish.Should().NotThrowAsync();
        (await IntegrationTestHelpers.WaitUntilAsync(() => Task.FromResult(counter.Value == 1), TimeSpan.FromSeconds(20)))
            .Should().BeTrue("the recovered consumer must receive the message");
        (await rabbit.CountConnectionsAsync()).Should().Be(
            2, "only the bus' recovered connection and the fixture's inspection connection may exist");
    }
}

internal sealed record RecoveryEvent(string Payload) : IntegrationEvent;
