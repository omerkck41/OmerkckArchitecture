using AwesomeAssertions;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.UnitOfWork;

public sealed class EfUnitOfWorkTests : IDisposable
{
    private readonly SqliteHarness<TestDbContext> _harness;
    private readonly EfUnitOfWork _sut;

    public EfUnitOfWorkTests()
    {
        _harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        _sut = new EfUnitOfWork(_harness.Context, new StubRepositoryFactory());
    }

    [Fact]
    public async Task BeginTransactionAsync_CalledTwice_KeepsSingleTransaction()
    {
        await _sut.BeginTransactionAsync();
        var first = _harness.Context.Database.CurrentTransaction;

        // Second call must be a no-op (??=). A plain assignment would throw "already in transaction".
        await _sut.BeginTransactionAsync();

        _harness.Context.Database.CurrentTransaction.Should().BeSameAs(first);
    }

    [Fact]
    public async Task SaveChangesAsync_AutoCommitTrue_CommitsTransaction()
    {
        await _sut.BeginTransactionAsync();
        _harness.Context.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "x" });

        var affected = await _sut.SaveChangesAsync(autoCommitTransaction: true);

        affected.Should().Be(1);
        _harness.Context.Database.CurrentTransaction.Should().BeNull("auto-commit must complete the transaction");
    }

    [Fact]
    public async Task SaveChangesAsync_AutoCommitFalse_LeavesTransactionOpen()
    {
        await _sut.BeginTransactionAsync();
        _harness.Context.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "x" });

        await _sut.SaveChangesAsync(autoCommitTransaction: false);

        _harness.Context.Database.CurrentTransaction.Should().NotBeNull("without auto-commit the transaction stays open");
    }

    [Fact]
    public async Task CommitAsync_WithoutActiveTransaction_Throws()
    {
        var act = () => _sut.CommitAsync();
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No active transaction*");
    }

    [Fact]
    public async Task CommitAsync_WithActiveTransaction_Commits()
    {
        await _sut.BeginTransactionAsync();
        _harness.Context.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "x" });
        await _sut.SaveChangesAsync();

        await _sut.CommitAsync();

        _harness.Context.Database.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public async Task RollbackAsync_WithoutActiveTransaction_Throws()
    {
        var act = () => _sut.RollbackAsync();
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No active transaction*");
    }

    [Fact]
    public async Task RollbackAsync_WithActiveTransaction_DiscardsChanges()
    {
        await _sut.BeginTransactionAsync();
        _harness.Context.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "x" });
        await _sut.SaveChangesAsync();

        await _sut.RollbackAsync();

        _harness.Context.Database.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public void Repository_ReturnsCachedInstancePerType()
    {
        var first = _sut.Repository<TestEntity, Guid>();
        var second = _sut.Repository<TestEntity, Guid>();

        first.Should().BeAssignableTo<IRepository<TestEntity, Guid>>();
        second.Should().BeSameAs(first, "repositories are cached by type key");
    }

    [Fact]
    public async Task DisposeAsync_WithActiveTransaction_DisposesTransaction()
    {
        await _sut.BeginTransactionAsync();
        _harness.Context.Database.CurrentTransaction.Should().NotBeNull();

        await _sut.DisposeAsync();

        _harness.Context.Database.CurrentTransaction.Should().BeNull("dispose must release the active transaction");
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_IsIdempotent()
    {
        await _sut.BeginTransactionAsync();

        await _sut.DisposeAsync();
        var act = async () => await _sut.DisposeAsync();

        await act.Should().NotThrowAsync();
    }

    public void Dispose() => _harness.Dispose();
}
