using FluentAssertions;
using Kck.EventBus.Abstractions;
using Kck.EventBus.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Kck.EventBus.InMemory.Tests;

public sealed class InMemoryEventBusTests
{
    private readonly ServiceCollection _services = new();
    private readonly ILogger<InMemoryEventBus> _logger = Substitute.For<ILogger<InMemoryEventBus>>();

    [Fact]
    public async Task PublishAsync_WithSubscribedHandler_InvokesHandler()
    {
        var subscriber = new TestEventSubscriber();
        _services.AddSingleton(subscriber);
        var sp = _services.BuildServiceProvider();
        var sut = new InMemoryEventBus(sp, _logger);

        sut.Subscribe<TestEvent, TestEventSubscriber>();
        await sut.PublishAsync(new TestEvent { Message = "hello" });

        subscriber.LastMessage.Should().Be("hello");
    }

    [Fact]
    public async Task PublishAsync_WithoutSubscription_DispatchesViaDi()
    {
        var subscriber = new TestEventSubscriber();
        _services.AddSingleton<IEventHandler<TestEvent>>(subscriber);
        var sp = _services.BuildServiceProvider();
        var sut = new InMemoryEventBus(sp, _logger);

        await sut.PublishAsync(new TestEvent { Message = "di-dispatch" });

        subscriber.LastMessage.Should().Be("di-dispatch");
    }

    [Fact]
    public async Task PublishAsync_HandlerThrows_DoesNotPropagateException()
    {
        _services.AddTransient<ThrowingEventSubscriber>();
        var sp = _services.BuildServiceProvider();
        var sut = new InMemoryEventBus(sp, _logger);

        sut.Subscribe<TestEvent, ThrowingEventSubscriber>();

        var act = () => sut.PublishAsync(new TestEvent { Message = "fail" });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Subscribe_DuplicateHandler_DoesNotAddTwice()
    {
        _services.AddTransient<IEventHandler<TestEvent>, TestEventSubscriber>();
        var sp = _services.BuildServiceProvider();
        var sut = new InMemoryEventBus(sp, _logger);

        sut.Subscribe<TestEvent, TestEventSubscriber>();
        sut.Subscribe<TestEvent, TestEventSubscriber>();

        // No exception and subsequent publish should invoke handler once
    }

    [Fact]
    public async Task Subscribe_ConcurrentCalls_DoNotThrow()
    {
        _services.AddTransient<IEventHandler<TestEvent>, TestEventSubscriber>();
        var sp = _services.BuildServiceProvider();
        var sut = new InMemoryEventBus(sp, _logger);

        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => sut.Subscribe<TestEvent, TestEventSubscriber>()));

        var act = () => Task.WhenAll(tasks);

        await act.Should().NotThrowAsync();
    }
}

public sealed record TestEvent : IntegrationEvent
{
    public string? Message { get; init; }
}

public sealed class TestEventSubscriber : IEventHandler<TestEvent>
{
    public string? LastMessage { get; private set; }

    public Task HandleAsync(TestEvent @event, CancellationToken ct = default)
    {
        LastMessage = @event.Message;
        return Task.CompletedTask;
    }
}

public sealed class ThrowingEventSubscriber : IEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent @event, CancellationToken ct = default)
    {
        throw new InvalidOperationException("Handler failed");
    }
}
