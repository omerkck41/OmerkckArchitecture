using AwesomeAssertions;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Repositories;

// Issue #113: BulkUpdateAsync must work on providers that cannot map an object-typed parameter (SQLite).
public class BulkUpdateTests
{
    private static async Task<SqliteHarness<TestDbContext>> SeedAsync()
    {
        var harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        harness.Context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "a", Value = 1, DeletedDate = DateTime.UtcNow },
            new TestEntity { Id = Guid.NewGuid(), Name = "b", Value = 2, DeletedDate = DateTime.UtcNow });
        await harness.Context.SaveChangesAsync();
        harness.Context.ChangeTracker.Clear();
        return harness;
    }

    private static Task<List<TestEntity>> AllAsync(SqliteHarness<TestDbContext> harness)
    {
        harness.Context.ChangeTracker.Clear();
        return harness.Context.TestEntities.OrderBy(e => e.Name).ToListAsync();
    }

    [Fact]
    public async Task BulkUpdateAsync_ValueTypeProperty_UpdatesMatchingRowsOnly()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<TestEntity, Guid>(harness.Context);

        await repo.BulkUpdateAsync(e => e.Value == 1, [(e => e.Value, 99)]);

        var rows = await AllAsync(harness);
        rows.Select(r => r.Value).Should().Equal(99, 2);
    }

    [Fact]
    public async Task BulkUpdateAsync_MultipleProperties_UpdatesAllInOneStatement()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<TestEntity, Guid>(harness.Context);

        await repo.BulkUpdateAsync(e => e.Value == 2, [(e => e.Name, "z"), (e => e.Value, 42)]);

        var rows = await AllAsync(harness);
        rows.Should().ContainSingle(r => r.Name == "z" && r.Value == 42);
        rows.Should().ContainSingle(r => r.Name == "a" && r.Value == 1);
    }

    [Fact]
    public async Task BulkUpdateAsync_NullForNullableProperty_WritesNull()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<TestEntity, Guid>(harness.Context);

        await repo.BulkUpdateAsync(e => e.Value == 1, [(e => e.DeletedDate!, null)]);

        var rows = await AllAsync(harness);
        rows.Single(r => r.Name == "a").DeletedDate.Should().BeNull();
        rows.Single(r => r.Name == "b").DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task BulkUpdateAsync_ValueOfWrongType_ThrowsAndLeavesRowsUnchanged()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<TestEntity, Guid>(harness.Context);

        var act = () => repo.BulkUpdateAsync(e => e.Value == 1, [(e => e.Name, "z"), (e => e.Value, 99L)]);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Value*Int32*Int64*");
        var rows = await AllAsync(harness);
        rows.Select(r => (r.Name, r.Value)).Should().Equal(("a", 1), ("b", 2));
    }

    [Fact]
    public async Task BulkUpdateAsync_SoftDeletedRow_IsNotUpdated()
    {
        // Like the repository's reads: soft-deleted rows are left out, with or without the global soft-delete filter
        // (TestDbContext has none).
        using var harness = await SeedAsync();
        var deleted = await harness.Context.TestEntities.SingleAsync(e => e.Name == "b");
        deleted.IsDeleted = true;
        await harness.Context.SaveChangesAsync();
        var repo = new EfRepository<TestEntity, Guid>(harness.Context);

        await repo.BulkUpdateAsync(e => e.Value > 0, [(e => e.Value, 7)]);

        var rows = await AllAsync(harness);
        rows.Select(r => (r.Name, r.Value)).Should().Equal(("a", 7), ("b", 2));
    }

    [Fact]
    public async Task BulkUpdateAsync_NullForNonNullableValueType_Throws()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<TestEntity, Guid>(harness.Context);

        var act = () => repo.BulkUpdateAsync(e => e.Value == 1, [(e => e.Value, null)]);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Value*null*Int32*");
    }
}
