using AwesomeAssertions;
using Kck.Persistence.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Configuration;

/// <summary>
/// SQLite (and SQL Server datetime2) return DateTime values with Kind = Unspecified, so a UTC audit date is serialized
/// without the "Z" suffix. UseUtcDateTimes marks every DateTime read from the database as UTC.
/// </summary>
public sealed class UtcDateTimeTests
{
    private sealed class UtcContext(DbContextOptions<UtcContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.UseUtcDateTimes();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<TestEntity>().Ignore(e => e.RowVersion);
    }

    [Fact]
    public async Task UseUtcDateTimes_ReadBack_IsUtc()
    {
        using var harness = new SqliteHarness<UtcContext>(o => new UtcContext(o));
        var created = new DateTime(2026, 10, 31, 9, 15, 0, DateTimeKind.Utc);
        var id = Guid.NewGuid();
        harness.Context.Entities.Add(new TestEntity { Id = id, CreatedDate = created, DeletedDate = created });
        await harness.Context.SaveChangesAsync();

        await using var read = harness.NewContext();
        var row = await read.Entities.SingleAsync(e => e.Id == id);

        row.CreatedDate.Should().Be(created);
        row.CreatedDate.Kind.Should().Be(DateTimeKind.Utc);
        row.DeletedDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task UseUtcDateTimes_LocalValue_IsStoredAsUtc()
    {
        using var harness = new SqliteHarness<UtcContext>(o => new UtcContext(o));
        var local = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Local);
        var id = Guid.NewGuid();
        harness.Context.Entities.Add(new TestEntity { Id = id, CreatedDate = local });
        await harness.Context.SaveChangesAsync();

        await using var read = harness.NewContext();
        var row = await read.Entities.SingleAsync(e => e.Id == id);

        row.CreatedDate.Should().Be(local.ToUniversalTime());
    }

    [Fact]
    public async Task WithoutUseUtcDateTimes_ReadBack_IsUnspecified()
    {
        // The problem the convention solves (control).
        using var harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        var id = Guid.NewGuid();
        harness.Context.TestEntities.Add(new TestEntity { Id = id, CreatedDate = DateTime.UtcNow });
        await harness.Context.SaveChangesAsync();

        await using var read = harness.NewContext();
        (await read.TestEntities.SingleAsync(e => e.Id == id)).CreatedDate.Kind.Should().Be(DateTimeKind.Unspecified);
    }
}
