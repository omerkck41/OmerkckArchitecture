using AwesomeAssertions;
using Kck.Persistence.Abstractions.Auditing;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

// IAuditUserProvider: resolved per save from the service provider that created the context (SQLite, real DI wiring).
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class AuditInterceptorUserTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private ServiceProvider? _provider;

    public Task InitializeAsync() => _connection.OpenAsync();

    public async Task DisposeAsync()
    {
        if (_provider is not null)
            await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task SavingChanges_WithAuditUser_OnAdded_SetsCreatedByOnly()
    {
        await BuildAsync(withUserProvider: true);
        await using var scope = Scope("alice");
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };

        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        entity.CreatedBy.Should().Be("alice");
        entity.ModifiedBy.Should().BeNull();
    }

    [Fact]
    public async Task SavingChanges_WithAuditUser_OnModified_SetsModifiedBy()
    {
        await BuildAsync(withUserProvider: true);
        await using var scope = Scope("alice");
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        entity.Name = "B";
        await ctx.SaveChangesAsync();

        entity.ModifiedBy.Should().Be("alice");
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task SavingChanges_WithAuditUser_OnRemove_SetsDeletedBy()
    {
        await BuildAsync(withUserProvider: true);
        await using var scope = Scope("alice");
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        ctx.TestEntities.Remove(entity);
        await ctx.SaveChangesAsync();

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedBy.Should().Be("alice");
    }

    [Fact]
    public async Task RepositoryDelete_WithAuditUser_SetsDeletedBy()
    {
        await BuildAsync(withUserProvider: true);
        await using var scope = Scope("alice");
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        await new EfRepository<TestEntity, Guid>(ctx).DeleteAsync(entity);
        await ctx.SaveChangesAsync();

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedBy.Should().Be("alice");
        // Same as DbContext.Remove: a soft delete is not a modification.
        entity.ModifiedBy.Should().BeNull();
        entity.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public async Task RepositoryDeleteRange_WithAuditUser_SetsDeletedByWithoutModifiedStamp()
    {
        await BuildAsync(withUserProvider: true);
        await using var scope = Scope("alice");
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        await new EfRepository<TestEntity, Guid>(ctx).DeleteRangeAsync([entity]);
        await ctx.SaveChangesAsync();

        entity.DeletedBy.Should().Be("alice");
        entity.ModifiedBy.Should().BeNull();
        entity.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public async Task RepositoryDeleteThenRevert_SameSave_StampsModified()
    {
        // A revert is a modification again, even before the delete was saved.
        await BuildAsync(withUserProvider: true);
        await using var scope = Scope("alice");
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();
        var repository = new EfRepository<TestEntity, Guid>(ctx);

        await repository.DeleteAsync(entity);
        await repository.RevertSoftDeleteAsync(entity);
        await ctx.SaveChangesAsync();

        entity.IsDeleted.Should().BeFalse();
        entity.ModifiedBy.Should().Be("alice");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Delete_AlreadySoftDeleted_KeepsOriginalDeletedBy(bool throughRepository)
    {
        await BuildAsync(withUserProvider: true);
        await using var scope = Scope("alice");
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var deletedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Gone", IsDeleted = true, DeletedDate = deletedAt, DeletedBy = "x" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        if (throughRepository)
            await new EfRepository<TestEntity, Guid>(ctx).DeleteAsync(entity);
        else
            ctx.TestEntities.Remove(entity);
        await ctx.SaveChangesAsync();

        entity.DeletedBy.Should().Be("x");
        entity.DeletedDate.Should().Be(deletedAt);
    }

    [Fact]
    public async Task SavingChanges_ScopedAuditUser_EachScopeRecordsItsOwnUser()
    {
        // ValidateScopes is on: resolving the scoped provider from the root would throw.
        await BuildAsync(withUserProvider: true);
        var first = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        var second = new TestEntity { Id = Guid.NewGuid(), Name = "B" };

        await using (var scope = Scope("alice"))
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            ctx.TestEntities.Add(first);
            await ctx.SaveChangesAsync();
        }

        await using (var scope = Scope("bob"))
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            ctx.TestEntities.Add(second);
            await ctx.SaveChangesAsync();
        }

        first.CreatedBy.Should().Be("alice");
        second.CreatedBy.Should().Be("bob");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SavingChanges_AuditUserWithoutUser_LeavesUserFieldsUnchanged(string? user)
    {
        await BuildAsync(withUserProvider: true);
        await using var scope = Scope(user);
        await AssertUserFieldsUnchangedAsync(scope.ServiceProvider.GetRequiredService<TestDbContext>());
    }

    [Fact]
    public async Task SavingChanges_NoAuditUserProvider_LeavesUserFieldsUnchanged()
    {
        await BuildAsync(withUserProvider: false);
        await using var scope = _provider!.CreateAsyncScope();
        await AssertUserFieldsUnchangedAsync(scope.ServiceProvider.GetRequiredService<TestDbContext>());
    }

    private static async Task AssertUserFieldsUnchangedAsync(TestDbContext ctx)
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A", CreatedBy = "seed" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();
        entity.Name = "B";
        await ctx.SaveChangesAsync();
        ctx.TestEntities.Remove(entity);
        await ctx.SaveChangesAsync();

        entity.CreatedBy.Should().Be("seed");
        entity.ModifiedBy.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
        entity.CreatedDate.Should().NotBe(default);
        entity.ModifiedDate.Should().NotBeNull();
        entity.DeletedDate.Should().NotBeNull();
    }

    private async Task BuildAsync(bool withUserProvider)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddKckPersistence(p => p
            .UseEntityFramework<TestDbContext>(o => o.UseSqlite(_connection))
            .AddAuditInterceptor());
        if (withUserProvider)
        {
            services.AddScoped<RequestUser>();
            services.AddScoped<IAuditUserProvider>(sp => sp.GetRequiredService<RequestUser>());
        }

        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        await using var scope = _provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TestDbContext>().Database.EnsureCreatedAsync();
    }

    // A request scope whose signed-in user is the given one.
    private AsyncServiceScope Scope(string? user)
    {
        var scope = _provider!.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<RequestUser>().CurrentUser = user;
        return scope;
    }

    private sealed class RequestUser : IAuditUserProvider
    {
        public string? CurrentUser { get; set; }
    }
}
