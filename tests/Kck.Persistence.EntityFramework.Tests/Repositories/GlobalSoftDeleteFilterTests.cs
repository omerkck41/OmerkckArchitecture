using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Repositories;

// ApplyGlobalSoftDeleteQueryFilter + QueryOptions.IncludeDeleted: the repository must lift the global soft-delete
// filter too, and only that filter.
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class GlobalSoftDeleteFilterTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly SqliteConnection _isolatedConnection = new("DataSource=:memory:"); // a second, empty database
    private FilteredDbContext _ctx = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await _isolatedConnection.OpenAsync();
        _ctx = new FilteredDbContext(new DbContextOptionsBuilder<FilteredDbContext>().UseSqlite(_connection).Options);
        await _ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _connection.DisposeAsync();
        await _isolatedConnection.DisposeAsync();
    }

    private async Task<Guid> AddSoftDeletedAsync()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "deleted", IsDeleted = true };
        _ctx.TestEntities.Add(entity);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();
        return entity.Id;
    }

    [Fact]
    public async Task GetAsync_WithDeleted_GlobalFilterApplied_ReturnsSoftDeletedEntity()
    {
        var id = await AddSoftDeletedAsync();
        var repository = new EfRepository<TestEntity, Guid>(_ctx);

        var found = await repository.GetAsync(e => e.Id == id, QueryOptions.WithDeleted);

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_Default_GlobalFilterApplied_HidesSoftDeletedEntity()
    {
        var id = await AddSoftDeletedAsync();
        var repository = new EfRepository<TestEntity, Guid>(_ctx);

        var found = await repository.GetAsync(e => e.Id == id);

        found.Should().BeNull();
    }

    [Fact]
    public async Task CountAndList_WithDeleted_GlobalFilterApplied_IncludeSoftDeletedEntity()
    {
        await AddSoftDeletedAsync();
        _ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "live" });
        await _ctx.SaveChangesAsync();
        var repository = new EfRepository<TestEntity, Guid>(_ctx);

        (await repository.CountAsync(QueryOptions.WithDeleted)).Should().Be(2);
        (await repository.CountAsync()).Should().Be(1);
        (await repository.GetListAsync(QueryOptions.WithDeleted)).Count.Should().Be(2);
    }

    [Fact]
    public async Task RevertSoftDeleteAsync_GlobalFilterApplied_RestoresEntity()
    {
        var id = await AddSoftDeletedAsync();
        var repository = new EfRepository<TestEntity, Guid>(_ctx);

        await repository.RevertSoftDeleteAsync(id);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();

        (await repository.GetAsync(e => e.Id == id)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_WithDeleted_KeepsOtherNamedFilters()
    {
        await using var ctx = new TenantDbContext(
            new DbContextOptionsBuilder<TenantDbContext>().UseSqlite(_connection).Options);
        await ctx.Database.EnsureCreatedAsync();
        var otherTenant = new TestEntity { Id = Guid.NewGuid(), Name = TenantDbContext.OtherTenant, IsDeleted = true };
        var ownTenant = new TestEntity { Id = Guid.NewGuid(), Name = "own-tenant", IsDeleted = true };
        ctx.TestEntities.AddRange(otherTenant, ownTenant);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();
        var repository = new EfRepository<TestEntity, Guid>(ctx);

        (await repository.GetAsync(e => e.Id == otherTenant.Id, QueryOptions.WithDeleted))
            .Should().BeNull("IncludeDeleted lifts only the soft-delete filter, never a tenant filter");
        (await repository.GetAsync(e => e.Id == ownTenant.Id, QueryOptions.WithDeleted))
            .Should().NotBeNull("the soft-delete filter itself is lifted");
    }

    [Fact]
    public async Task WithDeleted_NoGlobalFilter_KeepsUnnamedTenantFilter()
    {
        await using var ctx = new UnnamedTenantWithoutGlobalFilterDbContext(
            new DbContextOptionsBuilder().UseSqlite(_isolatedConnection).Options);
        await ctx.Database.EnsureCreatedAsync();
        var other = new TenantOrder { Id = Guid.NewGuid(), Tenant = UnnamedTenantDbContextBase.OtherTenant, IsDeleted = true };
        var own = new TenantOrder { Id = Guid.NewGuid(), Tenant = "own", IsDeleted = true };
        ctx.Orders.AddRange(other, own);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();
        var repository = new EfRepository<TenantOrder, Guid>(ctx);

        (await repository.GetAsync(o => o.Id == other.Id, QueryOptions.WithDeleted)).Should().BeNull();
        (await repository.GetAsync(o => o.Id == own.Id, QueryOptions.WithDeleted)).Should().NotBeNull();
    }

    [Fact]
    public async Task WithDeleted_GlobalFilter_IncludedDependentKeepsItsUnnamedTenantFilter()
    {
        await using var ctx = new UnnamedTenantWithGlobalFilterDbContext(
            new DbContextOptionsBuilder().UseSqlite(_isolatedConnection).Options);
        await ctx.Database.EnsureCreatedAsync();
        var order = new TenantOrder { Id = Guid.NewGuid(), Tenant = "own", IsDeleted = true };
        order.Notes.Add(new TenantNote { Id = Guid.NewGuid(), Tenant = "own" });
        order.Notes.Add(new TenantNote { Id = Guid.NewGuid(), Tenant = UnnamedTenantDbContextBase.OtherTenant });
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();
        var repository = new EfRepository<TenantOrder, Guid>(ctx);

        var found = await repository.GetAsync(o => o.Id == order.Id, QueryOptions.WithDeleted, includes: [o => o.Notes]);

        found.Should().NotBeNull();
        found!.Notes.Should().ContainSingle().Which.Tenant.Should().Be("own");
    }

    public sealed class TenantOrder : FullEntity<Guid>
    {
        public string Tenant { get; set; } = string.Empty;

        public List<TenantNote> Notes { get; set; } = [];
    }

    public sealed class TenantNote : Entity<Guid>
    {
        public Guid TenantOrderId { get; set; }

        public string Tenant { get; set; } = string.Empty;
    }

    // Unnamed tenant filters: on the soft-deletable order without the global filter, and on a dependent that is not
    // soft-deletable (so it may stay unnamed) next to the global filter. One sealed type per variant: EF caches the
    // model per context type.
    private abstract class UnnamedTenantDbContextBase(DbContextOptions options, bool applyGlobalFilter) : DbContext(options)
    {
        public const string OtherTenant = "other";

        public DbSet<TenantOrder> Orders => Set<TenantOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantOrder>(b =>
            {
                b.Ignore(o => o.RowVersion);
                b.HasMany(o => o.Notes).WithOne().HasForeignKey(n => n.TenantOrderId);
                if (applyGlobalFilter)
                    b.HasQueryFilter("Tenant", o => o.Tenant != OtherTenant);
                else
                    b.HasQueryFilter(o => o.Tenant != OtherTenant);
            });
            modelBuilder.Entity<TenantNote>().HasQueryFilter(n => n.Tenant != OtherTenant);
            if (applyGlobalFilter)
                modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
        }
    }

    private sealed class UnnamedTenantWithoutGlobalFilterDbContext(DbContextOptions options)
        : UnnamedTenantDbContextBase(options, applyGlobalFilter: false);

    private sealed class UnnamedTenantWithGlobalFilterDbContext(DbContextOptions options)
        : UnnamedTenantDbContextBase(options, applyGlobalFilter: true);

    [Fact]
    public async Task ApplyGlobalSoftDeleteQueryFilter_UnnamedFilterOnSoftDeletable_ThrowsWithGuidance()
    {
        await using var ctx = new UnnamedFilterDbContext(
            new DbContextOptionsBuilder<UnnamedFilterDbContext>().UseSqlite(_connection).Options);

        var buildModel = () => ctx.Model;

        buildModel.Should().Throw<InvalidOperationException>()
            .WithMessage("*TestEntity*unnamed query filter*HasQueryFilter(\"Tenant\"*");
    }

    [Fact]
    public async Task UnnamedFilterAddedAfterGlobalFilter_ModelBuildThrows()
    {
        await using var ctx = new LateUnnamedFilterDbContext(new DbContextOptionsBuilder().UseSqlite(_isolatedConnection).Options);

        var buildModel = () => ctx.Model;

        buildModel.Should().Throw<InvalidOperationException>().WithMessage("*anonymous and named*");
    }

    [Fact]
    public async Task NamedNullFilter_OptsOneEntityOutOfTheGlobalFilter()
    {
        await using var ctx = new OptOutFilterDbContext(new DbContextOptionsBuilder().UseSqlite(_isolatedConnection).Options);
        await ctx.Database.EnsureCreatedAsync();
        ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "deleted", IsDeleted = true });
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        (await ctx.TestEntities.CountAsync()).Should().Be(1);
    }

    // Configures TestEntity after ApplyGlobalSoftDeleteQueryFilter: with an unnamed filter (EF rejects the mix) or by
    // clearing the named soft-delete filter for this one entity (the documented opt-out). One sealed type per variant.
    private abstract class LateFilterDbContextBase(DbContextOptions options, bool optOut) : DbContext(options)
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(b =>
            {
                b.HasKey(e => e.Id);
                b.Ignore(e => e.RowVersion);
            });
            modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
            if (optOut)
                modelBuilder.Entity<TestEntity>().HasQueryFilter(ModelBuilderExtensions.SoftDeleteQueryFilterName, null);
            else
                modelBuilder.Entity<TestEntity>().HasQueryFilter(e => e.Name != "x");
        }
    }

    private sealed class LateUnnamedFilterDbContext(DbContextOptions options) : LateFilterDbContextBase(options, optOut: false);

    private sealed class OptOutFilterDbContext(DbContextOptions options) : LateFilterDbContextBase(options, optOut: true);

    // Before 4.6 the soft-delete filter replaced this unnamed tenant filter without a trace.
    private sealed class UnnamedFilterDbContext(DbContextOptions<UnnamedFilterDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(b =>
            {
                b.HasKey(e => e.Id);
                b.Ignore(e => e.RowVersion);
                b.HasQueryFilter(e => e.Name != TenantDbContext.OtherTenant);
            });
            modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
        }
    }

    private sealed class FilteredDbContext(DbContextOptions<FilteredDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(b =>
            {
                b.HasKey(e => e.Id);
                b.Ignore(e => e.RowVersion);
            });
            modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
        }
    }

    // A second, named filter on the same entity (multi-tenancy) next to the global soft-delete filter.
    private sealed class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
    {
        public const string OtherTenant = "other-tenant";

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(b =>
            {
                b.HasKey(e => e.Id);
                b.Ignore(e => e.RowVersion);
                b.HasQueryFilter("Tenant", e => e.Name != OtherTenant);
            });
            modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
        }
    }
}
