using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public async Task DeletedEntity_EventsAreDispatched()
    {
        var entity = new PlainEntity { Id = Guid.NewGuid(), Name = "A" };
        _ctx.PlainEntities.Add(entity);
        await _ctx.SaveChangesAsync();
        var deleted = new TestDomainEvent();
        entity.AddDomainEvent(deleted);

        _ctx.PlainEntities.Remove(entity);
        await _ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(deleted);
    }

    [Fact]
    public async Task PermanentRepositoryDelete_EventsAreDispatchedAndRowIsGone()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        _ctx.TestEntities.Add(entity);
        await _ctx.SaveChangesAsync();
        var deleted = new TestDomainEvent();
        entity.AddDomainEvent(deleted);

        await _uow.Repository<TestEntity, Guid>().DeleteAsync(entity, permanent: true);
        await _ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(deleted);
        (await _ctx.TestEntities.IgnoreQueryFilters().AsNoTracking().AnyAsync(e => e.Id == entity.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesFails_EventsStayOnTheEntityAndAreNotDispatched()
    {
        var id = Guid.NewGuid();
        await using (var other = new TestDbContext(
            new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options))
        {
            other.TestEntities.Add(new TestEntity { Id = id, Name = "existing" });
            await other.SaveChangesAsync();
        }

        var duplicate = new TestEntity { Id = id, Name = "duplicate" };
        var domainEvent = new TestDomainEvent();
        duplicate.AddDomainEvent(domainEvent);
        _ctx.TestEntities.Add(duplicate);

        var save = () => _ctx.SaveChangesAsync();

        await save.Should().ThrowAsync<DbUpdateException>();
        _dispatcher.CallCount.Should().Be(0);
        duplicate.DomainEvents.Should().Equal(domainEvent);
    }

    [Fact]
    public async Task LaterSavingInterceptorThrows_EventIsNotDispatchedByTheNextSave()
    {
        var throwing = new ThrowOnSavingInterceptor();
        await using var ctx = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(
                new DomainEventDispatchInterceptor(_dispatcher, NullLogger<DomainEventDispatchInterceptor>.Instance),
                throwing)
            .Options);
        var failed = new TestEntity { Id = Guid.NewGuid(), Name = "never saved" };
        var stale = new TestDomainEvent();
        failed.AddDomainEvent(stale);
        ctx.TestEntities.Add(failed);
        throwing.FailNext = true;

        var save = () => ctx.SaveChangesAsync();
        await save.Should().ThrowAsync<InvalidOperationException>().WithMessage("saving rejected");
        ctx.ChangeTracker.Clear();
        var kept = new TestEntity { Id = Guid.NewGuid(), Name = "saved" };
        var keptEvent = new TestDomainEvent();
        kept.AddDomainEvent(keptEvent);
        ctx.TestEntities.Add(kept);
        await ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(keptEvent);
        failed.DomainEvents.Should().Equal(stale);
    }

    [Fact]
    public async Task EventRaisedDuringTheSave_IsDispatchedByTheSameSave()
    {
        var raisedDuringSave = new TestDomainEvent();
        _ctx.ChangeTracker.StateChanged += (_, e) =>
        {
            if (e.NewState == EntityState.Unchanged && e.Entry.Entity is TestEntity saved)
                saved.AddDomainEvent(raisedDuringSave);
        };
        _ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "A" });

        await _ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(raisedDuringSave);
    }

    [Fact]
    public async Task CanceledSave_EventsStayOnTheEntityAndAreNotDispatched()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);
        _ctx.TestEntities.Add(entity);
        using var canceled = new CancellationTokenSource();
        await canceled.CancelAsync();

        var save = () => _ctx.SaveChangesAsync(canceled.Token);

        await save.Should().ThrowAsync<OperationCanceledException>();
        _dispatcher.CallCount.Should().Be(0);
        entity.DomainEvents.Should().Equal(domainEvent);
    }

    [Fact]
    public async Task CanceledWhileWriting_EventsGoBackOnTheEntityAndAreNotDispatched()
    {
        await using var ctx = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(
                new DomainEventDispatchInterceptor(_dispatcher, NullLogger<DomainEventDispatchInterceptor>.Instance),
                new CancelOnWriteInterceptor())
            .Options);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);
        ctx.TestEntities.Add(entity);

        var save = () => ctx.SaveChangesAsync();

        await save.Should().ThrowAsync<OperationCanceledException>();
        _dispatcher.CallCount.Should().Be(0);
        entity.DomainEvents.Should().Equal(domainEvent);
    }

    [Fact]
    public void SyncSaveChanges_DeletedEntity_EventsAreDispatched()
    {
        var entity = new PlainEntity { Id = Guid.NewGuid(), Name = "A" };
        _ctx.PlainEntities.Add(entity);
        _ctx.SaveChanges();
        var deleted = new TestDomainEvent();
        entity.AddDomainEvent(deleted);

        _ctx.PlainEntities.Remove(entity);
        _ctx.SaveChanges();

        _dispatcher.Received.Should().Equal(deleted);
        _ctx.PlainEntities.Any().Should().BeFalse();
    }

    [Fact]
    public async Task SyncSaveChanges_PermanentRepositoryDelete_EventsAreDispatched()
    {
        var entity = AddWithEvent(out var added);
        _ctx.SaveChanges();
        var deleted = new TestDomainEvent();
        entity.AddDomainEvent(deleted);

        await new EfRepository<TestEntity, Guid>(_ctx).DeleteAsync(entity, permanent: true);
        _ctx.SaveChanges();

        _dispatcher.Received.Should().Equal(added, deleted);
        _ctx.TestEntities.Any().Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrencyFailure_EventsStayOnTheEntityAndAreNotDispatched()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        _ctx.TestEntities.Add(entity);
        await _ctx.SaveChangesAsync();
        await using (var other = new TestDbContext(
            new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options))
        {
            await other.TestEntities.Where(e => e.Id == entity.Id).ExecuteDeleteAsync();
        }

        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);
        entity.Name = "B";
        var save = () => _ctx.SaveChangesAsync();

        await save.Should().ThrowAsync<DbUpdateConcurrencyException>();
        _dispatcher.CallCount.Should().Be(0);
        entity.DomainEvents.Should().Equal(domainEvent);
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

        // Order across entities follows EF's change tracker and is not part of the contract.
        _dispatcher.Received.Should().BeEquivalentTo([failed, next]);
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

    private sealed class ThrowOnSavingInterceptor : SaveChangesInterceptor
    {
        public bool FailNext { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!FailNext)
                return ValueTask.FromResult(result);

            FailNext = false;
            throw new InvalidOperationException("saving rejected");
        }
    }

    // Cancels the save after the interceptors' SavingChanges ran: EF reports it through SaveChangesCanceled.
    private sealed class CancelOnWriteInterceptor : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(
            System.Data.Common.DbCommand command,
            CommandEventData eventData,
            InterceptionResult<System.Data.Common.DbDataReader> result,
            CancellationToken cancellationToken = default) =>
            throw new OperationCanceledException("canceled while writing");

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            System.Data.Common.DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default) =>
            throw new OperationCanceledException("canceled while writing");
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
