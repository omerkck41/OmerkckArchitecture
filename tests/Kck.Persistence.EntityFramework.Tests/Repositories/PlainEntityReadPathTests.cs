using AwesomeAssertions;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Repositories;

/// <summary>
/// K1 (2026-10-31 health analysis, probe P4): an entity that is not <c>ISoftDeletable</c> (a plain
/// <c>Entity&lt;TId&gt;</c>) must be readable through the repository. <c>Query()</c> applied the soft-delete cast to
/// every T, which a relational provider cannot translate ("The LINQ expression ... could not be translated").
/// SQLite reproduces it; the EF InMemory provider does not, which is why the existing PlainEntity tests passed.
/// </summary>
public sealed class PlainEntityReadPathTests : IDisposable
{
    private readonly SqliteHarness<TestDbContext> _harness;
    private readonly TestDbContext _ctx;
    private readonly EfRepository<PlainEntity, Guid> _sut;

    public PlainEntityReadPathTests()
    {
        _harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        _ctx = _harness.Context;
        _sut = new EfRepository<PlainEntity, Guid>(_ctx);
    }

    private async Task<PlainEntity> Seed(string name)
    {
        var entity = new PlainEntity { Id = Guid.NewGuid(), Name = name };
        _ctx.PlainEntities.Add(entity);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();
        return entity;
    }

    [Fact]
    public async Task CountAsync_PlainEntity_CountsRows()
    {
        await Seed("a");
        await Seed("b");

        (await _sut.CountAsync()).Should().Be(2);
        (await _sut.CountAsync(predicate: e => e.Name == "a")).Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_PlainEntity_ReturnsRow()
    {
        var seeded = await Seed("found");

        var result = await _sut.GetByIdAsync(seeded.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("found");
    }

    [Fact]
    public async Task GetListAsync_PlainEntity_ReturnsPage()
    {
        await Seed("one");

        var page = await _sut.GetListAsync();

        page.Items.Should().ContainSingle().Which.Name.Should().Be("one");
    }

    [Fact]
    public async Task AnyAsync_PlainEntity_ReportsRows()
    {
        await Seed("exists");

        (await _sut.AnyAsync()).Should().BeTrue();
        (await _sut.AnyAsync(predicate: e => e.Name == "missing")).Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_PlainEntity_ReturnsMatchingRow()
    {
        await Seed("match");

        var result = await _sut.GetAsync(e => e.Name == "match");

        result.Should().NotBeNull();
        result!.Name.Should().Be("match");
    }

    [Fact]
    public async Task DeleteAsync_ById_PlainEntity_RemovesRow()
    {
        var seeded = await Seed("gone");

        await _sut.DeleteAsync(seeded.Id);
        await _ctx.SaveChangesAsync();

        (await _ctx.PlainEntities.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_ByPredicate_PlainEntity_RemovesRow()
    {
        await Seed("gone");

        await _sut.DeleteAsync(e => e.Name == "gone");
        await _ctx.SaveChangesAsync();

        (await _ctx.PlainEntities.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RevertSoftDeleteAsync_ById_PlainEntity_ReturnsTheRowUnchanged()
    {
        // A type without soft delete has nothing to restore: the row is found and returned as it is.
        var seeded = await Seed("kept");

        var result = await _sut.RevertSoftDeleteAsync(seeded.Id);

        result.Id.Should().Be(seeded.Id);
        _ctx.Entry(result).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task BulkUpdateAsync_PlainEntity_UpdatesMatchingRows()
    {
        // Same query path as the reads: the untranslatable cast reached ExecuteUpdate too.
        await Seed("old");

        await _sut.BulkUpdateAsync(e => e.Name == "old", [(e => e.Name, "new")]);

        (await _ctx.PlainEntities.CountAsync(e => e.Name == "new")).Should().Be(1);
    }

    [Fact]
    public async Task IncludeDeleted_PlainEntity_HasNoEffect()
    {
        var seeded = await Seed("plain");

        (await _sut.GetByIdAsync(seeded.Id, QueryOptions.WithDeleted)).Should().NotBeNull();
        (await _sut.CountAsync(QueryOptions.WithDeleted)).Should().Be(1);
    }

    public void Dispose() => _harness.Dispose();
}
