using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Interceptors;

// SQLite, not InMemory: dispatch timing depends on real transactions. Disposed through IAsyncLifetime.
#pragma warning disable CA1001
public sealed class DomainEventDispatchSemanticsTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ToggleDispatcher _dispatcher = new();
    private TestDbContext _ctx = null!;
    private EfUnitOfWork _uow = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new DomainEventDispatchInterceptor(
                _dispatcher, NullLogger<DomainEventDispatchInterceptor>.Instance))
            .Options;
        _ctx = new TestDbContext(options);
        await _ctx.Database.EnsureCreatedAsync();
        _uow = new EfUnitOfWork(_ctx, new StubRepositoryFactory());
    }

    public async Task DisposeAsync()
    {
        await _uow.DisposeAsync();
        await _ctx.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private TestEntity AddWithEvent(out TestDomainEvent domainEvent)
    {
        domainEvent = new TestDomainEvent();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        entity.AddDomainEvent(domainEvent);
        _ctx.TestEntities.Add(entity);
        return entity;
    }

    [Fact]
    public async Task NoTransaction_DispatcherSucceeds_DispatchesOnceAndClears()
    {
        var entity = AddWithEvent(out var domainEvent);

        await _ctx.SaveChangesAsync();

        _dispatcher.CallCount.Should().Be(1);
        _dispatcher.Received.Should().Equal(domainEvent);
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task NoTransaction_DispatcherThrows_KeepsEventsAndNextSaveRetries()
    {
        var entity = AddWithEvent(out var domainEvent);
        _dispatcher.FailNext = true;

        var save = () => _ctx.SaveChangesAsync();

        await save.Should().ThrowAsync<InvalidOperationException>();
        (await _ctx.TestEntities.AsNoTracking().AnyAsync(e => e.Id == entity.Id)).Should().BeTrue();
        entity.DomainEvents.Should().Equal(domainEvent);

        await _ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(domainEvent);
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task ExplicitTransaction_SaveChanges_DoesNotDispatchBeforeCommit()
    {
        await _uow.BeginTransactionAsync();
        AddWithEvent(out _);

        await _uow.SaveChangesAsync();

        _dispatcher.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task ExplicitTransaction_Commit_DispatchesOnce()
    {
        await _uow.BeginTransactionAsync();
        AddWithEvent(out var domainEvent);
        await _uow.SaveChangesAsync();

        await _uow.CommitAsync();

        _dispatcher.CallCount.Should().Be(1);
        _dispatcher.Received.Should().Equal(domainEvent);
    }

    [Fact]
    public async Task ExplicitTransaction_Rollback_DiscardsEvents()
    {
        await _uow.BeginTransactionAsync();
        var entity = AddWithEvent(out _);
        await _uow.SaveChangesAsync();

        await _uow.RollbackAsync();
        await _ctx.SaveChangesAsync();

        _dispatcher.CallCount.Should().Be(0);
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task ExplicitTransaction_RolledBackEvents_AreNotDispatchedByALaterCommit()
    {
        await using (var rolledBack = await _ctx.Database.BeginTransactionAsync())
        {
            AddWithEvent(out _);
            await _ctx.SaveChangesAsync();
            await rolledBack.RollbackAsync();
        }

        await using var committed = await _ctx.Database.BeginTransactionAsync();
        AddWithEvent(out var kept);
        await _ctx.SaveChangesAsync();
        await committed.CommitAsync();
        await _ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(kept);
    }

    [Fact]
    public async Task DirectCommit_EventsWaitForTheNextSaveChanges()
    {
        await using var transaction = await _ctx.Database.BeginTransactionAsync();
        AddWithEvent(out var domainEvent);
        await _ctx.SaveChangesAsync();

        await transaction.CommitAsync();
        _dispatcher.CallCount.Should().Be(0);

        await _ctx.SaveChangesAsync();
        _dispatcher.Received.Should().Equal(domainEvent);
    }

    [Fact]
    public async Task ExplicitTransaction_AutoCommitSave_DispatchesAfterCommit()
    {
        await _uow.BeginTransactionAsync();
        AddWithEvent(out var domainEvent);

        await _uow.SaveChangesAsync(autoCommitTransaction: true);

        _dispatcher.Received.Should().Equal(domainEvent);
        _ctx.Database.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public async Task ExplicitTransaction_TwoSaves_CommitDispatchesAllOnce()
    {
        await _uow.BeginTransactionAsync();
        AddWithEvent(out var first);
        await _uow.SaveChangesAsync();
        AddWithEvent(out var second);
        await _uow.SaveChangesAsync();

        await _uow.CommitAsync();

        _dispatcher.CallCount.Should().Be(1);
        _dispatcher.Received.Should().Equal(first, second);
    }

    [Fact]
    public async Task Commit_HandlerSavesWithSameContext_HandlerEventIsDispatched()
    {
        TestDomainEvent? handlerEvent = null;
        _dispatcher.OnDispatch = async () =>
        {
            _dispatcher.OnDispatch = null;
            AddWithEvent(out var raised);
            handlerEvent = raised;
            await _ctx.SaveChangesAsync();
        };
        await _uow.BeginTransactionAsync();
        AddWithEvent(out var first);
        await _uow.SaveChangesAsync();

        await _uow.CommitAsync();

        _dispatcher.Received.Should().Equal(first, handlerEvent!);
        (await _ctx.TestEntities.AsNoTracking().CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Commit_DispatcherThrows_NextSaveRetriesTheEvents()
    {
        await _uow.BeginTransactionAsync();
        var entity = AddWithEvent(out var domainEvent);
        await _uow.SaveChangesAsync();
        _dispatcher.FailNext = true;

        var commit = () => _uow.CommitAsync();

        await commit.Should().ThrowAsync<InvalidOperationException>();
        (await _ctx.TestEntities.AsNoTracking().AnyAsync(e => e.Id == entity.Id)).Should().BeTrue();

        await _ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(domainEvent);
    }

    [Fact]
    public async Task Commit_DispatcherThrows_NextUnitOfWorkTransactionStillParksEvents()
    {
        await _uow.BeginTransactionAsync();
        AddWithEvent(out var failed);
        await _uow.SaveChangesAsync();
        _dispatcher.FailNext = true;
        var commit = () => _uow.CommitAsync();
        await commit.Should().ThrowAsync<InvalidOperationException>().WithMessage("handler failed");
        var callsAfterFailure = _dispatcher.CallCount;

        await _uow.BeginTransactionAsync();
        AddWithEvent(out var next);
        await _uow.SaveChangesAsync();

        _ctx.Database.CurrentTransaction.Should().NotBeNull();
        _dispatcher.CallCount.Should().Be(callsAfterFailure, "events saved in the new transaction wait for its commit");

        await _uow.CommitAsync();

        _dispatcher.Received.Should().Equal(failed, next);
    }

    [Fact]
    public async Task TransactionDisposedWithoutCommit_EventsAreNotDispatchedByALaterCommit()
    {
        await using (await _ctx.Database.BeginTransactionAsync())
        {
            AddWithEvent(out _);
            await _ctx.SaveChangesAsync();
        }

        await using var committed = await _ctx.Database.BeginTransactionAsync();
        AddWithEvent(out var kept);
        await _ctx.SaveChangesAsync();
        await committed.CommitAsync();
        await _ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(kept);
    }

    private sealed class ToggleDispatcher : IDomainEventDispatcher
    {
        public bool FailNext { get; set; }
        public Func<Task>? OnDispatch { get; set; }
        public List<IDomainEvent> Received { get; } = [];
        public int CallCount { get; private set; }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            CallCount++;
            if (FailNext)
            {
                FailNext = false;
                throw new InvalidOperationException("handler failed");
            }

            Received.AddRange(events);
            if (OnDispatch is { } onDispatch)
                await onDispatch();
        }
    }
}
