using System.Data.Common;
using AwesomeAssertions;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.UnitOfWork;

// Commit and rollback failures are injected with a transaction interceptor on a real SQLite transaction.
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class EfUnitOfWorkFailureTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly FailingTransactionInterceptor _failures = new();
    private TestDbContext _ctx = null!;
    private EfUnitOfWork _sut = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        ThrowingDisposeTransactionFactory.FailNextDispose = false;
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(_failures)
            .ReplaceService<IRelationalTransactionFactory, ThrowingDisposeTransactionFactory>()
            .Options;
        _ctx = new TestDbContext(options);
        await _ctx.Database.EnsureCreatedAsync();
        _sut = new EfUnitOfWork(_ctx, new StubRepositoryFactory());
    }

    public async Task DisposeAsync()
    {
        await _sut.DisposeAsync();
        await _ctx.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private async Task<int> CountPersistedAsync()
    {
        await using var check = new TestDbContext(
            new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options);
        return await check.TestEntities.CountAsync();
    }

    [Fact]
    public async Task CommitAsync_DatabaseCommitFails_ThrowsReleasesAndPersistsNothing()
    {
        await _sut.BeginTransactionAsync();
        _ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "lost" });
        await _sut.SaveChangesAsync();
        _failures.FailNextCommit = true;

        var commit = () => _sut.CommitAsync();

        await commit.Should().ThrowAsync<InvalidOperationException>().WithMessage("commit failed");
        _ctx.Database.CurrentTransaction.Should().BeNull();
        (await CountPersistedAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_AfterFailedCommit_NextTransactionCommits()
    {
        await _sut.BeginTransactionAsync();
        _ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "lost" });
        await _sut.SaveChangesAsync();
        _failures.FailNextCommit = true;
        var failed = () => _sut.CommitAsync();
        await failed.Should().ThrowAsync<InvalidOperationException>();
        _ctx.ChangeTracker.Clear();

        await _sut.BeginTransactionAsync();
        _ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "kept" });
        await _sut.SaveChangesAsync();
        await _sut.CommitAsync();

        (await CountPersistedAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RollbackAsync_AfterFailedCommit_ThrowsNoActiveTransaction()
    {
        await _sut.BeginTransactionAsync();
        _failures.FailNextCommit = true;
        var failed = () => _sut.CommitAsync();
        await failed.Should().ThrowAsync<InvalidOperationException>().WithMessage("commit failed");

        var rollback = () => _sut.RollbackAsync();

        await rollback.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No active transaction*");
    }

    [Fact]
    public async Task CommitAsync_DatabaseCommitFails_RollsBackTheTransaction()
    {
        await _sut.BeginTransactionAsync();
        _failures.FailNextCommit = true;

        var commit = () => _sut.CommitAsync();

        await commit.Should().ThrowAsync<InvalidOperationException>();
        _failures.RolledBack.Should().Be(1);
    }

    [Fact]
    public async Task CommitAsync_CommitAndRollbackBothFail_SurfacesTheCommitError()
    {
        await _sut.BeginTransactionAsync();
        _failures.FailNextCommit = true;
        _failures.FailNextRollback = true;

        var commit = () => _sut.CommitAsync();

        await commit.Should().ThrowAsync<InvalidOperationException>().WithMessage("commit failed");
        _ctx.Database.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public async Task CommitAsync_CanceledWhileCommitting_StillCommits()
    {
        await _sut.BeginTransactionAsync();
        _ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "kept" });
        await _sut.SaveChangesAsync();
        using var aborted = new CancellationTokenSource();
        _failures.OnCommitting = aborted.Cancel;

        await _sut.CommitAsync(aborted.Token);

        _ctx.Database.CurrentTransaction.Should().BeNull();
        (await CountPersistedAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CommitAsync_CanceledBeforeTheCommit_ThrowsReleasesAndPersistsNothing()
    {
        await _sut.BeginTransactionAsync();
        _ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "lost" });
        await _sut.SaveChangesAsync();
        using var aborted = new CancellationTokenSource();
        await aborted.CancelAsync();

        var commit = () => _sut.CommitAsync(aborted.Token);

        await commit.Should().ThrowAsync<OperationCanceledException>();
        _ctx.Database.CurrentTransaction.Should().BeNull();
        (await CountPersistedAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_CommitFailsAndDisposeFails_SurfacesTheCommitError()
    {
        await _sut.BeginTransactionAsync();
        _failures.FailNextCommit = true;
        ThrowingDisposeTransactionFactory.FailNextDispose = true;

        var commit = () => _sut.CommitAsync();

        await commit.Should().ThrowAsync<InvalidOperationException>().WithMessage("commit failed");
        _ctx.Database.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public async Task RollbackAsync_RollbackFails_ThrowsAndStillReleases()
    {
        await _sut.BeginTransactionAsync();
        _failures.FailNextRollback = true;

        var rollback = () => _sut.RollbackAsync();

        await rollback.Should().ThrowAsync<InvalidOperationException>().WithMessage("rollback failed");
        _ctx.Database.CurrentTransaction.Should().BeNull();
        await _sut.BeginTransactionAsync();
        _ctx.Database.CurrentTransaction.Should().NotBeNull();
    }

    private sealed class ThrowingDisposeTransactionFactory(RelationalTransactionFactoryDependencies dependencies)
        : RelationalTransactionFactory(dependencies)
    {
        // Tests in one class run one at a time; InitializeAsync resets it.
        public static bool FailNextDispose { get; set; }

        public override RelationalTransaction Create(
            IRelationalConnection connection,
            DbTransaction transaction,
            Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned) =>
            new ThrowingDisposeTransaction(
                connection, transaction, transactionId, logger, transactionOwned, Dependencies.SqlGenerationHelper);
    }

    private sealed class ThrowingDisposeTransaction(
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
        bool transactionOwned,
        ISqlGenerationHelper sqlGenerationHelper)
        : RelationalTransaction(connection, transaction, transactionId, logger, transactionOwned, sqlGenerationHelper)
    {
        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (!ThrowingDisposeTransactionFactory.FailNextDispose)
                return;

            ThrowingDisposeTransactionFactory.FailNextDispose = false;
            throw new InvalidOperationException("dispose failed");
        }
    }

    private sealed class FailingTransactionInterceptor : DbTransactionInterceptor
    {
        public bool FailNextCommit { get; set; }
        public bool FailNextRollback { get; set; }
        public Action? OnCommitting { get; set; }
        public int RolledBack { get; private set; }

        public override Task TransactionRolledBackAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            RolledBack++;
            return Task.CompletedTask;
        }

        public override ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            OnCommitting?.Invoke();
            if (!FailNextCommit)
                return ValueTask.FromResult(result);

            FailNextCommit = false;
            throw new InvalidOperationException("commit failed");
        }

        public override ValueTask<InterceptionResult> TransactionRollingBackAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (!FailNextRollback)
                return ValueTask.FromResult(result);

            FailNextRollback = false;
            throw new InvalidOperationException("rollback failed");
        }
    }
}
