using AwesomeAssertions;
using Kck.Persistence.EntityFramework.Interceptors;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Interceptors;

public class AuditInterceptorTests
{
    private static TestDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditInterceptor())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task SavingChangesAsync_OnAdded_SetsCreatedDate()
    {
        using var ctx = NewContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A", CreatedDate = default };
        ctx.TestEntities.Add(entity);

        await ctx.SaveChangesAsync();

        entity.CreatedDate.Should().NotBe(default);
    }

    [Fact]
    public async Task SavingChangesAsync_OnModified_SetsModifiedDate()
    {
        using var ctx = NewContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        entity.ModifiedDate.Should().BeNull();
        entity.Name = "B";
        await ctx.SaveChangesAsync();

        entity.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public void SavingChanges_Sync_OnAdded_SetsCreatedDate()
    {
        using var ctx = NewContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A", CreatedDate = default };
        ctx.TestEntities.Add(entity);

        ctx.SaveChanges();

        entity.CreatedDate.Should().NotBe(default);
    }

    [Fact]
    public async Task SavingChanges_RemoveAlreadySoftDeleted_KeepsOriginalDeletedDate()
    {
        using var ctx = NewContext();
        var deletedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Gone", IsDeleted = true, DeletedDate = deletedAt };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        ctx.TestEntities.Remove(entity);
        await ctx.SaveChangesAsync();

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedDate.Should().Be(deletedAt);
        entity.ModifiedDate.Should().BeNull("a repeated delete is not a modification");
    }

    [Fact]
    public async Task SavingChanges_IsDeletedSetBeforeRemove_StillStampsDeletedDate()
    {
        // "Already deleted" means stored as deleted, not flagged in memory just before the delete.
        using var ctx = NewContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Live" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        entity.IsDeleted = true;
        ctx.TestEntities.Remove(entity);
        await ctx.SaveChangesAsync();

        entity.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task SavingChanges_OnDeletedSoftDeletable_ConvertsToSoftDelete()
    {
        using var ctx = NewContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        ctx.TestEntities.Remove(entity);
        await ctx.SaveChangesAsync();

        // Hard delete was rewritten to a soft delete — row survives, flagged deleted.
        var stored = await ctx.TestEntities
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(e => e.Id == entity.Id);
        stored.Should().NotBeNull();
        stored!.IsDeleted.Should().BeTrue();
        stored.DeletedDate.Should().NotBeNull();
    }
}
