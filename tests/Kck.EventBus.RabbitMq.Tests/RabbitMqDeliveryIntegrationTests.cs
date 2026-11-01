using System.Text.Json;
using AwesomeAssertions;
using Kck.EventBus.Abstractions;
using Kck.EventBus.RabbitMq;
using Kck.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

/// <summary>
/// Delivery counting against a live broker: publisher-supplied headers, handler cancellations, messages without a
/// MessageId, per-service dead-letter exchanges and the dead-letter log entry.
/// </summary>
/// <remarks>Requires Docker. CI: ubuntu-only (Category=Integration filter).</remarks>
[Trait("Category", "Integration")]
public sealed class RabbitMqDeliveryIntegrationTests(RabbitMqContainerFixture rabbit)
    : IClassFixture<RabbitMqContainerFixture>
{
    private static readonly JsonSerializerOptions CamelCase = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [DockerFact]
    public async Task PoisonMessage_WithPublisherSuppliedDeliveryCountHeader_IsStillBoundedByMaxDeliveryCount()
    {
        var counter = new Counter();
        var options = rabbit.CreateOptions("header-count", o => o.MaxDeliveryCount = 3);
        await using var services = IntegrationTestHelpers.BuildServices(s => s
            .AddSingleton(counter)
            .AddTransient<FailingHandler<HeaderCountEvent>>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<HeaderCountEvent, FailingHandler<HeaderCountEvent>>();
        await bus.StartConsumingAsync();

        // A classic queue neither sets nor strips x-delivery-count: here it is just a header the publisher chose.
        await rabbit.PublishRawAsync(
            options.ExchangeName,
            nameof(HeaderCountEvent),
            JsonSerializer.SerializeToUtf8Bytes(new HeaderCountEvent("always-fails"), CamelCase),
            new Dictionary<string, object?> { ["x-delivery-count"] = 0L },
            Guid.NewGuid().ToString());

        var deadLettered = await IntegrationTestHelpers.WaitUntilAsync(
            async () => await rabbit.GetMessageCountAsync(options.EffectiveDeadLetterQueueName) == 1,
            TimeSpan.FromSeconds(15));
        deadLettered.Should().BeTrue("a publisher-supplied x-delivery-count header must not reset the delivery count");
        await Task.Delay(500);
        counter.Value.Should().Be(3);
    }

    [DockerFact]
    public async Task Handler_ThrowingTaskCanceledException_IsTreatedAsFailure_AndDoesNotStallTheConsumer()
    {
        var counter = new Counter();
        var options = rabbit.CreateOptions("handler-oce", o =>
        {
            o.PrefetchCount = 1;
            o.MaxDeliveryCount = 2;
        });
        await using var services = IntegrationTestHelpers.BuildServices(s => s
            .AddSingleton(counter)
            .AddTransient<TimingOutHandler>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<TimeoutEvent, TimingOutHandler>();
        await bus.StartConsumingAsync();

        await bus.PublishAsync(new TimeoutEvent("first"));
        await bus.PublishAsync(new TimeoutEvent("second"));

        var deadLettered = await IntegrationTestHelpers.WaitUntilAsync(
            async () => await rabbit.GetMessageCountAsync(options.EffectiveDeadLetterQueueName) == 2,
            TimeSpan.FromSeconds(15));
        deadLettered.Should().BeTrue("a handler's TaskCanceledException is a failure, not a consumer shutdown");
        await Task.Delay(500);
        counter.Value.Should().Be(4);
        (await rabbit.GetMessageCountAsync($"{options.QueuePrefix}.{nameof(TimeoutEvent)}")).Should().Be(0u);
    }

    [DockerFact]
    public async Task DeadLetterQueues_OfTwoServicesSharingTheExchange_DoNotReceiveEachOthersMessages()
    {
        var counterA = new Counter();
        var counterB = new Counter();
        var optionsA = rabbit.CreateOptions("shared-a", o =>
        {
            o.ExchangeName = "kck.test.shared-exchange";
            o.QueuePrefix = "svc-a";
            o.MaxDeliveryCount = 2;
        });
        var optionsB = rabbit.CreateOptions("shared-b", o =>
        {
            o.ExchangeName = "kck.test.shared-exchange";
            o.QueuePrefix = "svc-b";
        });
        await using var servicesA = IntegrationTestHelpers.BuildServices(s => s
            .AddSingleton(counterA)
            .AddTransient<FailingHandler<ServiceAEvent>>());
        await using var servicesB = IntegrationTestHelpers.BuildServices(s => s
            .AddSingleton(counterB)
            .AddTransient<SucceedingHandler<ServiceBEvent>>());
        await using var busA = new RabbitMqEventBus(optionsA, servicesA, NullLogger<RabbitMqEventBus>.Instance);
        await using var busB = new RabbitMqEventBus(optionsB, servicesB, NullLogger<RabbitMqEventBus>.Instance);
        busA.Subscribe<ServiceAEvent, FailingHandler<ServiceAEvent>>();
        busB.Subscribe<ServiceBEvent, SucceedingHandler<ServiceBEvent>>();
        await busA.StartConsumingAsync();
        await busB.StartConsumingAsync();

        await busA.PublishAsync(new ServiceAEvent("only-for-a"));

        var deadLettered = await IntegrationTestHelpers.WaitUntilAsync(
            async () => await rabbit.GetMessageCountAsync(optionsA.EffectiveDeadLetterQueueName) == 1,
            TimeSpan.FromSeconds(15));
        deadLettered.Should().BeTrue();
        await Task.Delay(500);
        (await rabbit.GetMessageCountAsync(optionsB.EffectiveDeadLetterQueueName)).Should().Be(
            0u, "service B's dead-letter queue must not receive service A's rejected message");
        counterB.Value.Should().Be(0);
    }

    [DockerFact]
    public async Task PoisonMessage_LogsDeadLetteredExactlyOnce()
    {
        var counter = new Counter();
        var logger = new CapturingLogger<RabbitMqEventBus>();
        var options = rabbit.CreateOptions("dead-letter-log", o => o.MaxDeliveryCount = 3);
        await using var services = IntegrationTestHelpers.BuildServices(s => s
            .AddSingleton(counter)
            .AddTransient<FailingHandler<LoggedPoisonEvent>>());
        await using var bus = new RabbitMqEventBus(options, services, logger);
        bus.Subscribe<LoggedPoisonEvent, FailingHandler<LoggedPoisonEvent>>();
        await bus.StartConsumingAsync();

        await bus.PublishAsync(new LoggedPoisonEvent("always-fails"));

        var deadLettered = await IntegrationTestHelpers.WaitUntilAsync(
            async () => await rabbit.GetMessageCountAsync(options.EffectiveDeadLetterQueueName) == 1,
            TimeSpan.FromSeconds(15));
        deadLettered.Should().BeTrue();
        await Task.Delay(500);
        var entry = logger.Entries
            .Where(e => e.Level == LogLevel.Warning && e.Message.Contains("dead-lettered", StringComparison.Ordinal))
            .Should().ContainSingle().Which;
        entry.Values.Should().Contain(new KeyValuePair<string, object?>("Attempts", 3));
        entry.Message.Should().Contain(options.EffectiveDeadLetterExchangeName);
    }

    [DockerFact]
    public async Task Message_WithoutMessageId_IsDeliveredAtMostTwice()
    {
        var counter = new Counter();
        var options = rabbit.CreateOptions("no-message-id", o => o.MaxDeliveryCount = 5);
        await using var services = IntegrationTestHelpers.BuildServices(s => s
            .AddSingleton(counter)
            .AddTransient<FailingHandler<AnonymousEvent>>());
        await using var bus = new RabbitMqEventBus(options, services, NullLogger<RabbitMqEventBus>.Instance);
        bus.Subscribe<AnonymousEvent, FailingHandler<AnonymousEvent>>();
        await bus.StartConsumingAsync();

        await rabbit.PublishRawAsync(
            options.ExchangeName,
            nameof(AnonymousEvent),
            JsonSerializer.SerializeToUtf8Bytes(new AnonymousEvent("always-fails"), CamelCase),
            headers: null,
            messageId: null);

        var deadLettered = await IntegrationTestHelpers.WaitUntilAsync(
            async () => await rabbit.GetMessageCountAsync(options.EffectiveDeadLetterQueueName) == 1,
            TimeSpan.FromSeconds(15));
        deadLettered.Should().BeTrue();
        await Task.Delay(500);
        counter.Value.Should().Be(2, "without a MessageId the broker's Redelivered flag counts as the last delivery");
    }
}

internal static class IntegrationTestHelpers
{
    public static ServiceProvider BuildServices(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection().AddLogging();
        configure(services);
        return services.BuildServiceProvider();
    }

    public static async Task<bool> WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
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

internal sealed class FailingHandler<TEvent>(Counter counter) : IEventHandler<TEvent>
    where TEvent : IntegrationEvent
{
    public async Task HandleAsync(TEvent @event, CancellationToken ct = default)
    {
        counter.Increment();
        await Task.Yield();
        throw new InvalidOperationException("Handler always fails.");
    }
}

internal sealed class SucceedingHandler<TEvent>(Counter counter) : IEventHandler<TEvent>
    where TEvent : IntegrationEvent
{
    public Task HandleAsync(TEvent @event, CancellationToken ct = default)
    {
        counter.Increment();
        return Task.CompletedTask;
    }
}

// Behaves like an HttpClient call that hits its timeout: the returned task ends with a TaskCanceledException.
internal sealed class TimingOutHandler(Counter counter) : IEventHandler<TimeoutEvent>
{
    public async Task HandleAsync(TimeoutEvent @event, CancellationToken ct = default)
    {
        counter.Increment();
        await Task.Yield();
        throw new TaskCanceledException("Simulated HttpClient timeout.");
    }
}

internal sealed record HeaderCountEvent(string Payload) : IntegrationEvent;

internal sealed record TimeoutEvent(string Payload) : IntegrationEvent;

internal sealed record ServiceAEvent(string Payload) : IntegrationEvent;

internal sealed record ServiceBEvent(string Payload) : IntegrationEvent;

internal sealed record LoggedPoisonEvent(string Payload) : IntegrationEvent;

internal sealed record AnonymousEvent(string Payload) : IntegrationEvent;
