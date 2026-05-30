using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.Security;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Tests;

// E1 (v3.0): FullEntity replaces old Entity as the "batteries-included" base class.
public class TestEntity : FullEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

// Not soft-deletable — exercises the hard-delete / EnsureAttached branches in EfRepository.
public class PlainEntity : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    public DbSet<PlainEntity> PlainEntities => Set<PlainEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).HasMaxLength(100);
            // RowVersion (Timestamp) is not supported by InMemory provider — ignore it
            b.Ignore(e => e.RowVersion);
        });
        modelBuilder.Entity<PlainEntity>(b => b.HasKey(e => e.Id));
    }
}

// --- Cascade soft-delete model (SoftDeleteHelper) ---

public class CascadeParent : FullEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public List<CascadeChild> Children { get; set; } = [];
    public CascadeSingle? Reference { get; set; }
    public Guid? ReferenceId { get; set; }
}

public class CascadeChild : FullEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public Guid ParentId { get; set; }
}

public class CascadeSingle : FullEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
}

public class CascadeDbContext(DbContextOptions<CascadeDbContext> options) : DbContext(options)
{
    public DbSet<CascadeParent> Parents => Set<CascadeParent>();
    public DbSet<CascadeChild> Children => Set<CascadeChild>();
    public DbSet<CascadeSingle> Singles => Set<CascadeSingle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CascadeParent>(b =>
        {
            b.Ignore(e => e.RowVersion);
            b.HasMany(e => e.Children).WithOne().HasForeignKey(c => c.ParentId);
            b.HasOne(e => e.Reference).WithMany().HasForeignKey(e => e.ReferenceId);
        });
        modelBuilder.Entity<CascadeChild>(b => b.Ignore(e => e.RowVersion));
        modelBuilder.Entity<CascadeSingle>(b => b.Ignore(e => e.RowVersion));
    }
}

// --- Global soft-delete query filter model (ModelBuilderExtensions) ---

public class GlobalFilterDbContext(DbContextOptions<GlobalFilterDbContext> options) : DbContext(options)
{
    public DbSet<TestEntity> Items => Set<TestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(b =>
        {
            b.Ignore(e => e.RowVersion);
            b.Property(e => e.Name).HasMaxLength(100);
        });
        modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
    }
}

/// <summary>
/// SQLite in-memory database harness. Unlike the EF InMemory provider, SQLite supports
/// transactions, ExecuteUpdate, and relational query filters — required for mutation
/// coverage of EfUnitOfWork, BulkOperationHelper, and the global query filter.
/// </summary>
public sealed class SqliteHarness<TContext> : IDisposable
    where TContext : DbContext
{
    private readonly SqliteConnection _connection;
    private readonly Func<DbContextOptions<TContext>, TContext> _factory;
    public TContext Context { get; }

    public SqliteHarness(Func<DbContextOptions<TContext>, TContext> factory)
    {
        _factory = factory;
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlite(_connection)
            .Options;

        Context = factory(options);
        Context.Database.EnsureCreated();
    }

    /// <summary>Creates an additional context over the same in-memory database (for detached-entity scenarios).</summary>
    public TContext NewContext()
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlite(_connection)
            .Options;
        return _factory(options);
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}

// Resolves IFilterPropertyWhitelist<T> from a fixed allow-list — used to assert
// the dynamic-query path passes the whitelist guard.
internal sealed class TestWhitelist<T>(params string[] allowed) : IFilterPropertyWhitelist<T>
{
    public IReadOnlySet<string> AllowedProperties { get; } =
        new HashSet<string>(allowed, StringComparer.OrdinalIgnoreCase);

    public bool IsAllowed(string propertyName) => AllowedProperties.Contains(propertyName);
}

// Constructs a plain EfRepository<T,TId> for EfUnitOfWork tests (no DI container needed).
internal sealed class StubRepositoryFactory : IEfRepositoryFactory
{
    public IRepository<T, TId> Create<T, TId>(DbContext context) where T : Entity<TId>
        => new EfRepository<T, TId>(context);
}

internal sealed record TestDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Captures dispatched domain events so tests can assert the interceptor fired.
internal sealed class CapturingDispatcher : Kck.Persistence.EntityFramework.Interceptors.IDomainEventDispatcher
{
    public List<IDomainEvent> Received { get; } = [];
    public int CallCount { get; private set; }

    public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        CallCount++;
        Received.AddRange(events);
        return Task.CompletedTask;
    }
}
