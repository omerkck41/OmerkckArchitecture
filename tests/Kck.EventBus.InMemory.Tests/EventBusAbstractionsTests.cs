using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using Kck.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Kck.EventBus.InMemory.Tests;

#pragma warning disable IL2026, IL3050
public sealed class EventBusAbstractionsTests
{
    public sealed record PingEvent : IntegrationEvent
    {
        public string Text { get; init; } = string.Empty;
    }

    public sealed class PingHandler : IEventHandler<PingEvent>
    {
        public List<string> Seen { get; } = [];

        public Task HandleAsync(PingEvent eventData, CancellationToken ct = default)
        {
            Seen.Add(eventData.Text);
            return Task.CompletedTask;
        }
    }

    public sealed class SecondPingHandler : IEventHandler<PingEvent>
    {
        public Task HandleAsync(PingEvent eventData, CancellationToken ct = default) => Task.CompletedTask;
    }

    public abstract class AbstractHandler : IEventHandler<PingEvent>
    {
        public Task HandleAsync(PingEvent eventData, CancellationToken ct = default) => Task.CompletedTask;
    }

    public sealed record OtherEvent : IntegrationEvent;

    public sealed class MultiHandler : IEventHandler<PingEvent>, IEventHandler<OtherEvent>
    {
        public Task HandleAsync(PingEvent eventData, CancellationToken ct = default) => Task.CompletedTask;
        public Task HandleAsync(OtherEvent eventData, CancellationToken ct = default) => Task.CompletedTask;
    }

    private static readonly JsonSerializerOptions CamelCase = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static byte[] Json(string text) =>
        JsonSerializer.SerializeToUtf8Bytes(new { text }, CamelCase);

    [Fact]
    public void IntegrationEvent_NewInstances_HaveUniqueIdsAndRecentTimestamp()
    {
        var before = DateTimeOffset.UtcNow;

        var a = new PingEvent();
        var b = new PingEvent();

        a.Id.Should().NotBe(Guid.Empty).And.NotBe(b.Id);
        a.OccurredOn.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task InvokeAsync_ValidHandler_InvokesHandleAsyncWithEvent()
    {
        var handler = new PingHandler();

        await EventHandlerInvoker.InvokeAsync(handler, new PingEvent { Text = "x" }, typeof(PingEvent));

        handler.Seen.Should().Equal("x");
    }

    [Fact]
    public async Task InvokeAsync_CalledTwiceForSameType_UsesCachedMethodAndStillWorks()
    {
        var handler = new PingHandler();

        await EventHandlerInvoker.InvokeAsync(handler, new PingEvent { Text = "1" }, typeof(PingEvent));
        await EventHandlerInvoker.InvokeAsync(handler, new PingEvent { Text = "2" }, typeof(PingEvent));

        handler.Seen.Should().Equal("1", "2");
    }

    [Fact]
    public async Task InvokeAsync_HandlerThrows_PropagatesTargetInvocationException()
    {
        var handler = Substitute.For<IEventHandler<PingEvent>>();
        handler.HandleAsync(Arg.Any<PingEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("bad")));

        var act = () => EventHandlerInvoker.InvokeAsync(handler, new PingEvent(), typeof(PingEvent));

        // Task faults are awaited, so the original exception surfaces.
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("bad");
    }

    [Fact]
    public async Task ProcessAsync_UnknownEventName_DoesNothing()
    {
        var handler = new PingHandler();
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        var sut = new EventProcessor(services, Substitute.For<ILogger>());

        await sut.ProcessAsync("Ping", Json("x"), new ConcurrentDictionary<string, Type>(),
            new ConcurrentDictionary<string, ConcurrentBag<Type>>());

        handler.Seen.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_EventTypeKnownButNoHandlers_DoesNothing()
    {
        var handler = new PingHandler();
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        var sut = new EventProcessor(services, Substitute.For<ILogger>());
        var types = new ConcurrentDictionary<string, Type> { ["Ping"] = typeof(PingEvent) };

        await sut.ProcessAsync("Ping", Json("x"), types, new ConcurrentDictionary<string, ConcurrentBag<Type>>());

        handler.Seen.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_NullPayload_DoesNothing()
    {
        var handler = new PingHandler();
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        var sut = new EventProcessor(services, Substitute.For<ILogger>());
        var types = new ConcurrentDictionary<string, Type> { ["Ping"] = typeof(PingEvent) };
        var handlers = new ConcurrentDictionary<string, ConcurrentBag<Type>> { ["Ping"] = [typeof(PingHandler)] };

        await sut.ProcessAsync("Ping", "null"u8.ToArray(), types, handlers);

        handler.Seen.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_EventTypeNotIntegrationEvent_LogsWarningAndSkipsHandlers()
    {
        var handler = new PingHandler();
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var sut = new EventProcessor(services, logger);
        var types = new ConcurrentDictionary<string, Type> { ["Plain"] = typeof(PlainDto) };
        var handlers = new ConcurrentDictionary<string, ConcurrentBag<Type>> { ["Plain"] = [typeof(PingHandler)] };

        await sut.ProcessAsync("Plain", Json("x"), types, handlers);

        handler.Seen.Should().BeEmpty();
        logger.ReceivedCalls().Should().Contain(c => c.GetMethodInfo().Name == nameof(ILogger.Log));
    }

    public sealed class PlainDto
    {
        public string Text { get; set; } = string.Empty;
    }

    [Fact]
    public async Task ProcessAsync_RegisteredHandler_DeserializesCamelCaseAndInvokes()
    {
        var handler = new PingHandler();
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        var sut = new EventProcessor(services, Substitute.For<ILogger>());
        var types = new ConcurrentDictionary<string, Type> { ["Ping"] = typeof(PingEvent) };
        var handlers = new ConcurrentDictionary<string, ConcurrentBag<Type>> { ["Ping"] = [typeof(PingHandler)] };

        await sut.ProcessAsync("Ping", Json("hello"), types, handlers);

        handler.Seen.Should().Equal("hello");
    }

    [Fact]
    public async Task ProcessAsync_MissingHandlerInDi_LogsAndStillRunsRemainingHandlers()
    {
        var handler = new PingHandler();
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var sut = new EventProcessor(services, logger);
        var types = new ConcurrentDictionary<string, Type> { ["Ping"] = typeof(PingEvent) };
        var bag = new ConcurrentBag<Type> { typeof(SecondPingHandler), typeof(PingHandler) };
        var handlers = new ConcurrentDictionary<string, ConcurrentBag<Type>> { ["Ping"] = bag };

        await sut.ProcessAsync("Ping", Json("still"), types, handlers);

        handler.Seen.Should().Equal("still");
        logger.ReceivedCalls().Should().Contain(c => c.GetMethodInfo().Name == nameof(ILogger.Log));
    }

    [Fact]
    public void AddKckEventBus_InvokesConfigureWithBuilderOverSameCollection()
    {
        var services = new ServiceCollection();
        KckEventBusBuilder? captured = null;

        var returned = services.AddKckEventBus(b => captured = b);

        returned.Should().BeSameAs(services);
        captured.Should().NotBeNull();
        captured!.Services.Should().BeSameAs(services);
    }

    [Fact]
    public void RegisterHandlersFromAssembly_ScansConcreteHandlersOnly()
    {
        var services = new ServiceCollection();

        var builder = new KckEventBusBuilder(services).RegisterHandlersFromAssembly(typeof(EventBusAbstractionsTests).Assembly);

        builder.Services.Should().BeSameAs(services);
        services.Should().Contain(d => d.ServiceType == typeof(IEventHandler<PingEvent>) && d.ImplementationType == typeof(PingHandler)
            && d.Lifetime == ServiceLifetime.Transient);
        services.Should().NotContain(d => d.ImplementationType == typeof(AbstractHandler));
    }

    [Fact]
    public void RegisterHandlersFromAssembly_HandlerForMultipleEvents_RegistersEachInterface()
    {
        var services = new ServiceCollection();

        new KckEventBusBuilder(services).RegisterHandlersFromAssembly(typeof(EventBusAbstractionsTests).Assembly);

        services.Should().Contain(d => d.ServiceType == typeof(IEventHandler<PingEvent>) && d.ImplementationType == typeof(MultiHandler));
        services.Should().Contain(d => d.ServiceType == typeof(IEventHandler<OtherEvent>) && d.ImplementationType == typeof(MultiHandler));
    }

    [Fact]
    public void RegisterHandlersFromAssembly_NoAssemblies_RegistersNothing()
    {
        var services = new ServiceCollection();

        new KckEventBusBuilder(services).RegisterHandlersFromAssembly();

        services.Should().BeEmpty();
    }
}
#pragma warning restore IL2026, IL3050

