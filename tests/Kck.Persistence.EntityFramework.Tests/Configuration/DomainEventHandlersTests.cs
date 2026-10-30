using System.Reflection;
using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Persistence.EntityFramework.Interceptors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Configuration;

// ADR-0030: handlers registered with AddDomainEventHandlers receive the events the interceptor dispatches.
// The scan covers this whole test assembly, so every event type here is used by these tests only.
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class DomainEventHandlersTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private static readonly Assembly ThisAssembly = typeof(DomainEventHandlersTests).Assembly;
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly HandlerSink _sink = new();
    private ServiceProvider? _provider;

    public Task InitializeAsync() => _connection.OpenAsync();

    public async Task DisposeAsync()
    {
        if (_provider is not null)
            await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private async Task<ServiceProvider> BuildAsync(Action<KckPersistenceBuilder> configure)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_sink)
            .AddScoped<ScopeMarker>();
        services.AddKckPersistence(configure);
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        await using var scope = _provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TestDbContext>().Database.EnsureCreatedAsync();
        return _provider;
    }

    private Task<ServiceProvider> BuildWithHandlersAsync() =>
        BuildAsync(p => p
            .UseEntityFramework<TestDbContext>(o => o.UseSqlite(_connection))
            .AddDomainEventHandlers(ThisAssembly));

    private static async Task<TestEntity> SaveWithEventAsync(IServiceProvider scope, IDomainEvent domainEvent)
    {
        var ctx = scope.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        entity.AddDomainEvent(domainEvent);
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    [Fact]
    public async Task AddDomainEventHandlers_TwoHandlersForOneEvent_BothRun()
    {
        var provider = await BuildWithHandlersAsync();
        var domainEvent = new HandledEvent();

        await using (var scope = provider.CreateAsyncScope())
            await SaveWithEventAsync(scope.ServiceProvider, domainEvent);

        _sink.Calls.Should().BeEquivalentTo(
            [(nameof(FirstHandler), (IDomainEvent)domainEvent), (nameof(SecondHandler), domainEvent)]);
    }

    [Fact]
    public async Task AddDomainEventHandlers_HandlerForBaseEventType_NotInvoked()
    {
        var provider = await BuildWithHandlersAsync();
        var domainEvent = new DerivedEvent();

        await using (var scope = provider.CreateAsyncScope())
            await SaveWithEventAsync(scope.ServiceProvider, domainEvent);

        _sink.Calls.Should().Equal((nameof(DerivedEventHandler), (IDomainEvent)domainEvent));
    }

    [Fact]
    public async Task AddDomainEventHandlers_NoHandlerForEvent_DispatchSucceeds()
    {
        var provider = await BuildWithHandlersAsync();

        await using var scope = provider.CreateAsyncScope();
        var entity = await SaveWithEventAsync(scope.ServiceProvider, new UnhandledEvent());

        entity.DomainEvents.Should().BeEmpty();
        _sink.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task AddDomainEventHandlers_HandlerThrows_EventsGoBackOnEntity()
    {
        var provider = await BuildWithHandlersAsync();
        var domainEvent = new FailingEvent();
        _sink.FailNext = true;

        await using var scope = provider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        entity.AddDomainEvent(domainEvent);
        ctx.TestEntities.Add(entity);

        var save = () => ctx.SaveChangesAsync();

        await save.Should().ThrowAsync<InvalidOperationException>().WithMessage("handler failed");
        entity.DomainEvents.Should().Equal(domainEvent);

        await ctx.SaveChangesAsync();
        _sink.Calls.Should().Equal((nameof(FailingEventHandler), (IDomainEvent)domainEvent));
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task AddDomainEventHandlers_PooledContext_ResolvesHandlersFromNewScope()
    {
        var provider = await BuildAsync(p => p
            .UseEntityFrameworkPool<TestDbContext>(o => o.UseSqlite(_connection), poolSize: 4)
            .AddDomainEventHandlers(ThisAssembly));

        ScopeMarker requestMarker;
        await using (var scope = provider.CreateAsyncScope())
        {
            requestMarker = scope.ServiceProvider.GetRequiredService<ScopeMarker>();
            await SaveWithEventAsync(scope.ServiceProvider, new ScopedEvent());
        }

        _sink.Markers.Should().ContainSingle().Which.Should().NotBeSameAs(requestMarker);
    }

    [Fact]
    public async Task AddDomainEventHandlers_SameAssemblyTwice_HandlerRunsOnce()
    {
        var provider = await BuildAsync(p => p
            .UseEntityFramework<TestDbContext>(o => o.UseSqlite(_connection))
            .AddDomainEventHandlers(ThisAssembly)
            .AddDomainEventHandlers(ThisAssembly));
        var domainEvent = new DerivedEvent();

        await using (var scope = provider.CreateAsyncScope())
            await SaveWithEventAsync(scope.ServiceProvider, domainEvent);

        _sink.Calls.Should().ContainSingle();
    }

    [Fact]
    public void AddDomainEventHandlers_SkipsAbstractAndOpenGenericTypes()
    {
        var services = new ServiceCollection();

        services.AddKckPersistence(p => p.AddDomainEventHandlers(ThisAssembly));

        services.Where(d => d.ServiceType == typeof(IDomainEventHandler<HandledEvent>))
            .Select(d => d.ImplementationType)
            .Should().BeEquivalentTo([typeof(FirstHandler), typeof(SecondHandler)]);
        services.Should().NotContain(d => d.ImplementationType == typeof(OpenGenericHandler<>));
        services.Should().OnlyContain(d => d.ServiceType != typeof(IDomainEventHandler<HandledEvent>)
            || d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task AddDomainEventHandlers_SeveralEvents_DispatchedInRaisedOrder()
    {
        var provider = await BuildWithHandlersAsync();
        var first = new HandledEvent();
        var second = new DerivedEvent();

        await using (var scope = provider.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
            entity.AddDomainEvent(second);
            entity.AddDomainEvent(first);
            ctx.TestEntities.Add(entity);
            await ctx.SaveChangesAsync();
        }

        _sink.Calls.Select(c => c.Event).Should().Equal(second, first, first);
    }

    [Fact]
    public async Task AddDomainEventHandlers_HandlerThrows_LaterEventsNotDispatchedAndRowSaved()
    {
        var provider = await BuildWithHandlersAsync();
        var entityId = Guid.NewGuid();
        _sink.FailNext = true;

        await using (var scope = provider.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var entity = new TestEntity { Id = entityId, Name = "A" };
            entity.AddDomainEvent(new FailingEvent());
            entity.AddDomainEvent(new DerivedEvent());
            ctx.TestEntities.Add(entity);

            var save = () => ctx.SaveChangesAsync();

            await save.Should().ThrowAsync<InvalidOperationException>();
        }

        _sink.Calls.Should().BeEmpty();
        await using (var scope = provider.CreateAsyncScope())
            (await scope.ServiceProvider.GetRequiredService<TestDbContext>().TestEntities.AnyAsync(e => e.Id == entityId))
                .Should().BeTrue();
    }

    [Fact]
    public void AddDomainEventHandlers_SecondCallWithOtherAssembly_KeepsFirstCallsHandlers()
    {
        var services = new ServiceCollection();

        services.AddKckPersistence(p => p
            .AddDomainEventHandlers(ThisAssembly)
            .AddDomainEventHandlers(typeof(KckPersistenceBuilder).Assembly));

        services.Should().Contain(d => d.ImplementationType == typeof(FirstHandler));
        services.Where(d => d.ServiceType == typeof(IDomainEventDispatcher)).Should().ContainSingle();
    }

    [Fact]
    public void AddDomainEventHandlers_NullAssembly_ThrowsArgumentException()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKckPersistence(p => p.AddDomainEventHandlers(ThisAssembly, null!));

        act.Should().Throw<ArgumentException>().WithParameterName("assemblies");
    }

    [Fact]
    public void AddDomainEventHandlers_AfterFactoryRegisteredDispatcher_Throws()
    {
        var services = new ServiceCollection();
        services.AddScoped<IDomainEventDispatcher>(_ => new CapturingDispatcher());

        var act = () => services.AddKckPersistence(p => p.AddDomainEventHandlers(ThisAssembly));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddDomainEventHandlers_KeyedDispatcherRegistered_IsIgnored()
    {
        var services = new ServiceCollection();
        services.AddKeyedScoped<IDomainEventDispatcher, CapturingDispatcher>("other");

        var act = () => services.AddKckPersistence(p => p.AddDomainEventHandlers(ThisAssembly));

        act.Should().NotThrow();
    }

    [Fact]
    public void AddDomainEventHandlers_NoAssembly_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKckPersistence(p => p.AddDomainEventHandlers());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddDomainEventHandlers_AfterCustomDispatcher_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKckPersistence(p => p
            .AddDomainEventDispatch<CapturingDispatcher>()
            .AddDomainEventHandlers(ThisAssembly));

        act.Should().Throw<InvalidOperationException>().WithMessage("*CapturingDispatcher*");
    }

    [Fact]
    public void AddDomainEventDispatch_AfterHandlers_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKckPersistence(p => p
            .AddDomainEventHandlers(ThisAssembly)
            .AddDomainEventDispatch<CapturingDispatcher>());

        act.Should().Throw<InvalidOperationException>().WithMessage("*AddDomainEventHandlers*");
    }

    internal sealed class HandlerSink
    {
        private readonly List<(string Handler, IDomainEvent Event)> _calls = [];
        private readonly List<ScopeMarker> _markers = [];

        public bool FailNext { get; set; }
        public IReadOnlyList<(string Handler, IDomainEvent Event)> Calls => _calls;
        public IReadOnlyList<ScopeMarker> Markers => _markers;

        public void Record(string handler, IDomainEvent domainEvent) => _calls.Add((handler, domainEvent));
        public void Record(ScopeMarker marker) => _markers.Add(marker);
    }

    internal sealed class ScopeMarker;

    internal sealed record HandledEvent : TestEventBase;
    internal sealed record UnhandledEvent : TestEventBase;
    internal sealed record FailingEvent : TestEventBase;
    internal sealed record ScopedEvent : TestEventBase;
    internal record BaseEvent : TestEventBase;
    internal sealed record DerivedEvent : BaseEvent;

    internal abstract record TestEventBase : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    internal sealed class FirstHandler(HandlerSink sink) : IDomainEventHandler<HandledEvent>
    {
        public Task HandleAsync(HandledEvent domainEvent, CancellationToken cancellationToken)
        {
            sink.Record(nameof(FirstHandler), domainEvent);
            return Task.CompletedTask;
        }
    }

    internal sealed class SecondHandler(HandlerSink sink) : IDomainEventHandler<HandledEvent>
    {
        public Task HandleAsync(HandledEvent domainEvent, CancellationToken cancellationToken)
        {
            sink.Record(nameof(SecondHandler), domainEvent);
            return Task.CompletedTask;
        }
    }

    internal sealed class BaseEventHandler(HandlerSink sink) : IDomainEventHandler<BaseEvent>
    {
        public Task HandleAsync(BaseEvent domainEvent, CancellationToken cancellationToken)
        {
            sink.Record(nameof(BaseEventHandler), domainEvent);
            return Task.CompletedTask;
        }
    }

    internal sealed class DerivedEventHandler(HandlerSink sink) : IDomainEventHandler<DerivedEvent>
    {
        public Task HandleAsync(DerivedEvent domainEvent, CancellationToken cancellationToken)
        {
            sink.Record(nameof(DerivedEventHandler), domainEvent);
            return Task.CompletedTask;
        }
    }

    internal sealed class FailingEventHandler(HandlerSink sink) : IDomainEventHandler<FailingEvent>
    {
        public Task HandleAsync(FailingEvent domainEvent, CancellationToken cancellationToken)
        {
            if (sink.FailNext)
            {
                sink.FailNext = false;
                throw new InvalidOperationException("handler failed");
            }

            sink.Record(nameof(FailingEventHandler), domainEvent);
            return Task.CompletedTask;
        }
    }

    internal sealed class ScopedEventHandler(HandlerSink sink, ScopeMarker marker) : IDomainEventHandler<ScopedEvent>
    {
        public Task HandleAsync(ScopedEvent domainEvent, CancellationToken cancellationToken)
        {
            sink.Record(marker);
            return Task.CompletedTask;
        }
    }

    // Must not be registered: abstract, and open generic.
    internal abstract class AbstractHandler : IDomainEventHandler<HandledEvent>
    {
        public abstract Task HandleAsync(HandledEvent domainEvent, CancellationToken cancellationToken);
    }

    internal sealed class OpenGenericHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        public Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
