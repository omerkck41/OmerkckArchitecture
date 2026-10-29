using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Persistence.EntityFramework.Interceptors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Configuration;

// DbContext pooling: options are built once (singleton), so interceptors must not hold scoped services.
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class PooledContextTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly EventSink _sink = new();
    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_sink);
        services.AddKckPersistence(p => p
            .UseEntityFrameworkPool<TestDbContext>(o => o.UseSqlite(_connection), poolSize: 4)
            .AddAuditInterceptor()
            .AddDomainEventDispatch<ScopedDispatcher>());
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        await using var scope = _provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TestDbContext>().Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task UseEntityFrameworkPool_ReusesTheContextAcrossScopes()
    {
        TestDbContext first;
        await using (var scope = _provider.CreateAsyncScope())
            first = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        await using (var scope = _provider.CreateAsyncScope())
        {
            scope.ServiceProvider.GetRequiredService<TestDbContext>().Should().BeSameAs(first);
            scope.ServiceProvider.GetRequiredService<DbContext>().Should().BeSameAs(first);
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().NotBeNull();
        }
    }

    [Fact]
    public async Task PooledSaveChanges_DispatchesThroughAScopedDispatcher()
    {
        var domainEvent = new TestDomainEvent();
        await using (var scope = _provider.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
            entity.AddDomainEvent(domainEvent);
            ctx.TestEntities.Add(entity);
            await ctx.SaveChangesAsync();

            entity.CreatedDate.Should().NotBe(default, "the audit interceptor is wired into the pooled options");
        }

        _sink.Received.Should().Equal(domainEvent);
    }

    [Fact]
    public async Task PooledUnitOfWork_DispatchesAfterCommit()
    {
        var domainEvent = new TestDomainEvent();
        await using var scope = _provider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "B" };
        entity.AddDomainEvent(domainEvent);

        await uow.BeginTransactionAsync();
        ctx.TestEntities.Add(entity);
        await uow.SaveChangesAsync();
        _sink.Received.Should().BeEmpty("events wait for the commit");

        await uow.CommitAsync();

        _sink.Received.Should().Equal(domainEvent);
    }

    [Fact]
    public async Task PooledDispatcherFailure_PutsTheEventsBackOnTheEntity()
    {
        var domainEvent = new TestDomainEvent();
        _sink.FailNext = true;
        await using var scope = _provider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "C" };
        entity.AddDomainEvent(domainEvent);
        ctx.TestEntities.Add(entity);

        var save = () => ctx.SaveChangesAsync();

        await save.Should().ThrowAsync<InvalidOperationException>().WithMessage("dispatch failed");
        entity.DomainEvents.Should().Equal(domainEvent);
    }

    internal sealed class EventSink
    {
        public List<IDomainEvent> Received { get; } = [];
        public bool FailNext { get; set; }
    }

    // Scoped on purpose: under pooling it must come from a scope, never from the root provider.
    internal sealed class ScopedDispatcher(EventSink sink) : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            if (sink.FailNext)
            {
                sink.FailNext = false;
                throw new InvalidOperationException("dispatch failed");
            }

            sink.Received.AddRange(events);
            return Task.CompletedTask;
        }
    }
}
