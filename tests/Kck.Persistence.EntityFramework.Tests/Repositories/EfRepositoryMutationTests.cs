using AwesomeAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Repositories;

/// <summary>
/// SQLite-backed tests covering EfRepository branches the InMemory suite cannot reach:
/// tracking behaviour, detached-entity state transitions, not-found guards, and range deletes.
/// </summary>
public sealed class EfRepositoryMutationTests : IDisposable
{
    private readonly SqliteHarness<TestDbContext> _harness;
    private readonly TestDbContext _ctx;
    private readonly EfRepository<TestEntity, Guid> _sut;

    public EfRepositoryMutationTests()
    {
        _harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        _ctx = _harness.Context;
        _sut = new EfRepository<TestEntity, Guid>(_ctx);
    }

    private async Task<TestEntity> Seed(string name = "A", int value = 1, bool save = true)
    {
        var e = new TestEntity { Id = Guid.NewGuid(), Name = name, Value = value };
        _ctx.TestEntities.Add(e);
        if (save)
        {
            await _ctx.SaveChangesAsync();
            _ctx.Entry(e).State = EntityState.Detached;
        }
        return e;
    }

    // --- Query tracking flag (L33) ---

    [Fact]
    public async Task Query_EnableTracking_TracksEntities()
    {
        await Seed("Tracked");

        var item = _sut.Query(enableTracking: true).Single();

        _ctx.Entry(item).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task Query_DefaultNoTracking_DoesNotTrack()
    {
        await Seed("Untracked");

        var item = _sut.Query().Single();

        _ctx.Entry(item).State.Should().Be(EntityState.Detached);
    }

    // --- AnyAsync / CountAsync predicate branches (L117, L141) ---

    [Fact]
    public async Task AnyAsync_NoPredicate_ReturnsTrueWhenRowsExist()
    {
        await Seed();

        (await _sut.AnyAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_WithNonMatchingPredicate_ReturnsFalse()
    {
        await Seed("Exists");

        (await _sut.AnyAsync(e => e.Name == "Missing")).Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_WithPredicate_CountsMatchingSubset()
    {
        await Seed("Keep", 5);
        await Seed("Skip", 1);

        (await _sut.CountAsync(e => e.Value == 5)).Should().Be(1);
    }

    // --- AddRangeAsync over a lazy (non-IList) enumerable (L158) ---

    [Fact]
    public async Task AddRangeAsync_LazyEnumerable_AddsAll()
    {
        IEnumerable<TestEntity> lazy = Enumerable.Range(0, 3)
            .Select(i => new TestEntity { Id = Guid.NewGuid(), Name = $"L{i}" });

        await _sut.AddRangeAsync(lazy.Where(_ => true));
        await _ctx.SaveChangesAsync();

        (await _ctx.TestEntities.CountAsync()).Should().Be(3);
    }

    // --- UpdateAsync / UpdatePartialAsync / UpdateRangeAsync (L166, L181, L206) ---

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var e = await Seed("Orig");
        e.Name = "Changed";

        await _sut.UpdateAsync(e);
        await _ctx.SaveChangesAsync();
        _ctx.Entry(e).State = EntityState.Detached;

        var reloaded = await _ctx.TestEntities.AsNoTracking().SingleAsync();
        reloaded.Name.Should().Be("Changed");
    }

    [Fact]
    public async Task UpdatePartialAsync_OnlyPersistsSpecifiedProperty()
    {
        var e = await Seed("Orig", 1);
        e.Name = "NewName";
        e.Value = 999;

        await _sut.UpdatePartialAsync(e, [x => x.Name]);
        await _ctx.SaveChangesAsync();
        _ctx.Entry(e).State = EntityState.Detached;

        var reloaded = await _ctx.TestEntities.AsNoTracking().SingleAsync();
        reloaded.Name.Should().Be("NewName");
        reloaded.Value.Should().Be(1, "only Name was marked modified");
    }

    [Fact]
    public async Task UpdateRangeAsync_PersistsAllChanges()
    {
        var a = await Seed("A", save: false);
        var b = await Seed("B", save: false);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();
        a.Name = "A2";
        b.Name = "B2";

        await _sut.UpdateRangeAsync([a, b]);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        var names = await _ctx.TestEntities.AsNoTracking().Select(e => e.Name).ToListAsync();
        names.Should().BeEquivalentTo(["A2", "B2"]);
    }

    // --- Detached soft delete relies on explicit state = Modified (L226) ---

    [Fact]
    public async Task DeleteAsync_SoftDelete_DetachedEntity_PersistsFlag()
    {
        var e = await Seed("Soft");

        await _sut.DeleteAsync(e, permanent: false);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        var reloaded = await _ctx.TestEntities.IgnoreQueryFilters().AsNoTracking().SingleAsync();
        reloaded.IsDeleted.Should().BeTrue();
        reloaded.DeletedDate.Should().NotBeNull();
    }

    // --- Non-soft-deletable entity takes the else (hard delete) branch (L230/L231, L325/L326) ---

    [Fact]
    public async Task DeleteAsync_PlainEntity_SoftRequested_RemovesPhysically()
    {
        var p = new PlainEntity { Id = Guid.NewGuid(), Name = "P" };
        _ctx.PlainEntities.Add(p);
        await _ctx.SaveChangesAsync();

        await _sut2().DeleteAsync(p, permanent: false);
        await _ctx.SaveChangesAsync();

        (await _ctx.PlainEntities.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteRangeAsync_PlainEntities_SoftRequested_RemovesPhysically()
    {
        var items = new[]
        {
            new PlainEntity { Id = Guid.NewGuid(), Name = "P1" },
            new PlainEntity { Id = Guid.NewGuid(), Name = "P2" },
        };
        _ctx.PlainEntities.AddRange(items);
        await _ctx.SaveChangesAsync();

        await _sut2().DeleteRangeAsync(items, permanent: false);
        await _ctx.SaveChangesAsync();

        (await _ctx.PlainEntities.CountAsync()).Should().Be(0);
    }

    // --- DeleteAsync by predicate / id not-found guards (L257-261, L272-276) ---

    [Fact]
    public async Task DeleteAsync_ByPredicate_NotFound_Throws()
    {
        var act = () => _sut.DeleteAsync(e => e.Name == "ghost");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task DeleteAsync_ByPredicate_Found_PermanentlyDeletes()
    {
        await Seed("target");

        await _sut.DeleteAsync(e => e.Name == "target", permanent: true);
        await _ctx.SaveChangesAsync();

        (await _ctx.TestEntities.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_ByPredicate_LocatesSoftDeletedRow()
    {
        // Soft-deleted row is only reachable because DeleteAsync queries with withDeleted: true.
        var e = new TestEntity { Id = Guid.NewGuid(), Name = "gone", IsDeleted = true, DeletedDate = DateTime.UtcNow };
        _ctx.TestEntities.Add(e);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        await _sut.DeleteAsync(x => x.Name == "gone", permanent: true);
        await _ctx.SaveChangesAsync();

        (await _ctx.TestEntities.IgnoreQueryFilters().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_ById_LocatesSoftDeletedRow()
    {
        var e = new TestEntity { Id = Guid.NewGuid(), Name = "gone", IsDeleted = true, DeletedDate = DateTime.UtcNow };
        _ctx.TestEntities.Add(e);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        await _sut.DeleteAsync(e.Id, permanent: true);
        await _ctx.SaveChangesAsync();

        (await _ctx.TestEntities.IgnoreQueryFilters().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_ById_NotFound_Throws()
    {
        var act = () => _sut.DeleteAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task DeleteAsync_ById_Found_PermanentlyDeletes()
    {
        var e = await Seed("byid");

        await _sut.DeleteAsync(e.Id, permanent: true);
        await _ctx.SaveChangesAsync();

        (await _ctx.TestEntities.CountAsync()).Should().Be(0);
    }

    // --- RevertSoftDeleteAsync by id (L284-288) ---

    [Fact]
    public async Task RevertSoftDeleteAsync_ById_NotFound_Throws()
    {
        var act = () => _sut.RevertSoftDeleteAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task RevertSoftDeleteAsync_ById_ClearsDeletedFlag()
    {
        var e = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Deleted",
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            DeletedBy = "admin",
        };
        _ctx.TestEntities.Add(e);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        await _sut.RevertSoftDeleteAsync(e.Id);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        var reloaded = await _ctx.TestEntities.IgnoreQueryFilters().AsNoTracking().SingleAsync();
        reloaded.IsDeleted.Should().BeFalse();
        reloaded.DeletedBy.Should().BeNull();
    }

    // --- DeleteRangeAsync empty / permanent / soft (L305-321) ---

    [Fact]
    public async Task DeleteRangeAsync_CancelledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => _sut.DeleteRangeAsync([], cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DeleteRangeAsync_EmptyList_IsNoOp()
    {
        await Seed("keep");

        await _sut.DeleteRangeAsync([]);
        await _ctx.SaveChangesAsync();

        (await _ctx.TestEntities.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DeleteRangeAsync_Permanent_RemovesAll()
    {
        var a = await Seed("A", save: false);
        var b = await Seed("B", save: false);
        await _ctx.SaveChangesAsync();

        await _sut.DeleteRangeAsync([a, b], permanent: true);
        await _ctx.SaveChangesAsync();

        (await _ctx.TestEntities.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteRangeAsync_Soft_FlagsAllAsDeleted()
    {
        var a = await Seed("A", save: false);
        var b = await Seed("B", save: false);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        await _sut.DeleteRangeAsync([a, b], permanent: false);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        var all = await _ctx.TestEntities.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        all.Should().OnlyContain(e => e.IsDeleted);
    }

    // --- GetListAsync / GetListByDynamicAsync (L80, L101) ---

    [Fact]
    public async Task GetListAsync_ReturnsPaginatedResult()
    {
        await Seed("A", save: false);
        await Seed("B", save: false);
        await _ctx.SaveChangesAsync();

        var page = await _sut.GetListAsync(size: 10);

        page.Count.Should().Be(2);
        page.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetListByDynamicAsync_WithWhitelist_ReturnsMatching()
    {
        await Seed("Match", save: false);
        await Seed("Other", save: false);
        await _ctx.SaveChangesAsync();

        var repo = new EfRepository<TestEntity, Guid>(_ctx, new TestWhitelist<TestEntity>("Name"));
        var query = new DynamicQuery
        {
            Filter = new Filter { Field = "Name", Operator = "eq", Value = "Match" },
        };

        var page = await repo.GetListByDynamicAsync(query);

        // HaveCount(1) (not just ContainSingle-by-name) proves the dynamic filter was applied —
        // without it both rows would return.
        page.Items.Should().HaveCount(1);
        page.Items.Should().ContainSingle(e => e.Name == "Match");
    }

    [Fact]
    public async Task GetListByDynamicAsync_NonWhitelistedExistingField_RejectedByGuard()
    {
        var repo = new EfRepository<TestEntity, Guid>(_ctx, new TestWhitelist<TestEntity>("Name"));
        // "Value" is a real, queryable property — only the whitelist guard rejects it.
        // (If the guard were skipped, ToDynamic would happily filter by it.)
        var query = new DynamicQuery
        {
            Filter = new Filter { Field = "Value", Operator = "eq", Value = 42 },
        };

        var act = () => repo.GetListByDynamicAsync(query);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Value*");
    }

    // EfRepository for the non-soft-deletable PlainEntity.
    private EfRepository<PlainEntity, Guid> _sut2() => new(_ctx);

    public void Dispose() => _harness.Dispose();
}
