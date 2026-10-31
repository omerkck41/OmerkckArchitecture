using AwesomeAssertions;
using Kck.EventBus.Abstractions;
using Kck.EventBus.RabbitMq;
using Kck.EventBus.RabbitMq.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client.Exceptions;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

/// <summary>
/// Reliability behaviour against a live broker: bounded redelivery + dead-lettering, publish channel
/// re-creation, consumer prefetch, inequivalent queue arguments and the shared-connection health check.
/// </summary>
/// <remarks>Requires Docker. CI: ubuntu-only (Category=Integration filter).</remarks>
[Trait("Category", "Integration")]
public sealed class RabbitMqReliabilityIntegrationTests(RabbitMqContainerFixture rabbit)
    : IClassFixture<RabbitMqContainerFixture>
{
    [Fact]
    public async Task PoisonMessage_WithDeadLetterEnabled_IsDeliveredMaxDeliveryCountTimes_ThenLandsInDeadLetterQueue()
    {
        var counter = new Counter();
        var options = rabbit.CreateOptions("poison-dlx", o => o.MaxDeliveryCount = 3);
        await using var services = BuildServices(s => s.AddSingleton(counter).AddTransient<PoisonHandler>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<PoisonEvent, PoisonHandler>();
        await bus.StartConsumingAsync();

        await bus.PublishAsync(new PoisonEvent("always-fails"));

        var deadLettered = await WaitUntilAsync(
            async () => await rabbit.GetMessageCountAsync(options.EffectiveDeadLetterQueueName) == 1,
            TimeSpan.FromSeconds(15));
        deadLettered.Should().BeTrue("the message must reach the dead-letter queue after MaxDeliveryCount failures");
        await Task.Delay(500);
        counter.Value.Should().Be(3);
        (await rabbit.GetMessageCountAsync($"{options.QueuePrefix}.{nameof(PoisonEvent)}")).Should().Be(0u);
    }

    [Fact]
    public async Task PoisonMessage_WithDeadLetterDisabled_IsRejectedAfterMaxDeliveryCount_AndQueueHasNoArguments()
    {
        var counter = new Counter();
        var options = rabbit.CreateOptions("poison-nodlx", o =>
        {
            o.DeadLetterEnabled = false;
            o.MaxDeliveryCount = 2;
        });
        var queue = $"{options.QueuePrefix}.{nameof(PoisonNoDlxEvent)}";
        await using var services = BuildServices(s => s.AddSingleton(counter).AddTransient<PoisonNoDlxHandler>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<PoisonNoDlxEvent, PoisonNoDlxHandler>();
        await bus.StartConsumingAsync();

        await bus.PublishAsync(new PoisonNoDlxEvent("always-fails"));

        var reached = await WaitUntilAsync(() => Task.FromResult(counter.Value >= 2), TimeSpan.FromSeconds(15));
        reached.Should().BeTrue();
        await Task.Delay(500);
        counter.Value.Should().Be(2);
        (await rabbit.GetMessageCountAsync(queue)).Should().Be(0u);
        var redeclareWithoutArguments = () => rabbit.DeclareQueueAsync(queue, arguments: null);
        await redeclareWithoutArguments.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_AfterBrokerClosedPublishChannel_OpensNewChannelAndDelivers()
    {
        var counter = new Counter();
        var options = rabbit.CreateOptions("reopen");
        await using var services = BuildServices(s => s.AddSingleton(counter).AddTransient<ReopenHandler>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<ReopenEvent, ReopenHandler>();
        await bus.StartConsumingAsync();
        await bus.PublishAsync(new ReopenEvent("first"));
        (await WaitUntilAsync(() => Task.FromResult(counter.Value == 1), TimeSpan.FromSeconds(10))).Should().BeTrue();

        var channel = bus.PublishChannel!;
        var breakChannel = async () => await channel.ExchangeDeclarePassiveAsync("kck.test.does-not-exist");
        await breakChannel.Should().ThrowAsync<OperationInterruptedException>();
        (await WaitUntilAsync(() => Task.FromResult(!channel.IsOpen), TimeSpan.FromSeconds(5))).Should().BeTrue();

        var publishAgain = () => bus.PublishAsync(new ReopenEvent("second"));
        await publishAgain.Should().NotThrowAsync();
        (await WaitUntilAsync(() => Task.FromResult(counter.Value == 2), TimeSpan.FromSeconds(10))).Should().BeTrue();
    }

    [Fact]
    public async Task PrefetchCount_One_LeavesOtherMessagesReadyInQueue()
    {
        var counter = new Counter();
        var gate = new Gate();
        var options = rabbit.CreateOptions("prefetch", o => o.PrefetchCount = 1);
        await using var services = BuildServices(s => s
            .AddSingleton(counter)
            .AddSingleton(gate)
            .AddTransient<PrefetchHandler>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<PrefetchEvent, PrefetchHandler>();
        await bus.StartConsumingAsync();
        var queue = $"{options.QueuePrefix}.{nameof(PrefetchEvent)}";

        try
        {
            for (var i = 0; i < 4; i++)
                await bus.PublishAsync(new PrefetchEvent($"m{i}"));

            var threeReady = await WaitUntilAsync(
                async () => await rabbit.GetMessageCountAsync(queue) == 3,
                TimeSpan.FromSeconds(10));
            threeReady.Should().BeTrue("with PrefetchCount = 1 only one message may be in flight");
        }
        finally
        {
            gate.Release();
        }

        (await WaitUntilAsync(() => Task.FromResult(counter.Value == 4), TimeSpan.FromSeconds(10))).Should().BeTrue();
    }

    [Fact]
    public async Task StartConsumingAsync_WhenQueueAlreadyExistsWithoutDeadLetterArgument_ThrowsInvalidOperationExceptionWithRemediation()
    {
        var options = rabbit.CreateOptions("inequivalent");
        var queue = $"{options.QueuePrefix}.{nameof(InequivalentEvent)}";
        await rabbit.DeclareQueueAsync(queue, arguments: null);
        await using var services = BuildServices(s => s.AddTransient<InequivalentHandler>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<InequivalentEvent, InequivalentHandler>();

        var start = () => bus.StartConsumingAsync();

        var thrown = await start.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Message.Should().Contain(queue)
            .And.Contain("PRECONDITION_FAILED")
            .And.Contain("DeadLetterEnabled");
    }

    [Fact]
    public async Task HealthCheck_BeforeStart_IsUnhealthy_AfterStart_IsHealthy()
    {
        var options = rabbit.CreateOptions("health");
        await using var services = BuildServices(s => s
            .AddSingleton(options)
            .AddSingleton<RabbitMqEventBus>());
        var bus = services.GetRequiredService<RabbitMqEventBus>();
        var check = ActivatorUtilities.CreateInstance<RabbitMqHealthCheck>(services);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("rabbitmq-eventbus", check, HealthStatus.Unhealthy, ["ready"])
        };

        var before = await check.CheckHealthAsync(context);
        await bus.StartConsumingAsync();
        var after = await check.CheckHealthAsync(context);

        before.Status.Should().Be(HealthStatus.Unhealthy);
        after.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task HealthCheck_WhenConsumeChannelClosedByBroker_IsUnhealthy()
    {
        var options = rabbit.CreateOptions("health-channel");
        await using var services = BuildServices(s => s
            .AddSingleton(options)
            .AddSingleton<RabbitMqEventBus>()
            .AddTransient<HealthChannelHandler>());
        var bus = services.GetRequiredService<RabbitMqEventBus>();
        bus.Subscribe<HealthChannelEvent, HealthChannelHandler>();
        var check = ActivatorUtilities.CreateInstance<RabbitMqHealthCheck>(services);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("rabbitmq-eventbus", check, HealthStatus.Unhealthy, ["ready"])
        };
        await bus.StartConsumingAsync();
        (await check.CheckHealthAsync(context)).Status.Should().Be(HealthStatus.Healthy);

        // A channel-level error (404 on a passive declare) closes the consume channel; the client does not reopen it.
        var channel = bus.ConsumeChannel!;
        var breakChannel = async () => await channel.ExchangeDeclarePassiveAsync("kck.test.missing");
        await breakChannel.Should().ThrowAsync<OperationInterruptedException>();
        (await WaitUntilAsync(() => Task.FromResult(!channel.IsOpen), TimeSpan.FromSeconds(5))).Should().BeTrue();

        var result = await check.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("consumer channel");
    }

    private static ServiceProvider BuildServices(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection().AddLogging();
        configure(services);
        return services.BuildServiceProvider();
    }

    private static async Task<bool> WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
                return true;
            await Task.Delay(100);
        }

        return await condition();
    }
}

internal sealed class Counter
{
    private int _value;

    public int Value => Volatile.Read(ref _value);

    public int Increment() => Interlocked.Increment(ref _value);
}

internal sealed class Gate
{
    private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task WaitAsync() => _released.Task;

    public void Release() => _released.TrySetResult();
}

internal sealed record PoisonEvent(string Payload) : IntegrationEvent;

internal sealed class PoisonHandler(Counter counter) : IEventHandler<PoisonEvent>
{
    public Task HandleAsync(PoisonEvent @event, CancellationToken ct = default)
    {
        counter.Increment();
        throw new InvalidOperationException("Handler always fails.");
    }
}

internal sealed record PoisonNoDlxEvent(string Payload) : IntegrationEvent;

internal sealed class PoisonNoDlxHandler(Counter counter) : IEventHandler<PoisonNoDlxEvent>
{
    public Task HandleAsync(PoisonNoDlxEvent @event, CancellationToken ct = default)
    {
        counter.Increment();
        throw new InvalidOperationException("Handler always fails.");
    }
}

internal sealed record ReopenEvent(string Payload) : IntegrationEvent;

internal sealed class ReopenHandler(Counter counter) : IEventHandler<ReopenEvent>
{
    public Task HandleAsync(ReopenEvent @event, CancellationToken ct = default)
    {
        counter.Increment();
        return Task.CompletedTask;
    }
}

internal sealed record PrefetchEvent(string Payload) : IntegrationEvent;

internal sealed class PrefetchHandler(Counter counter, Gate gate) : IEventHandler<PrefetchEvent>
{
    public async Task HandleAsync(PrefetchEvent @event, CancellationToken ct = default)
    {
        await gate.WaitAsync();
        counter.Increment();
    }
}

internal sealed record HealthChannelEvent(string Payload) : IntegrationEvent;

internal sealed class HealthChannelHandler : IEventHandler<HealthChannelEvent>
{
    public Task HandleAsync(HealthChannelEvent @event, CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed record InequivalentEvent(string Payload) : IntegrationEvent;

internal sealed class InequivalentHandler : IEventHandler<InequivalentEvent>
{
    public Task HandleAsync(InequivalentEvent @event, CancellationToken ct = default) => Task.CompletedTask;
}
