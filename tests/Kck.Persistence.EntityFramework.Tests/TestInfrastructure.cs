using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Tests;

public class TestEntity : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).HasMaxLength(100);
            // RowVersion (Timestamp) is not supported by InMemory provider — ignore it
            b.Ignore(e => e.RowVersion);
        });
    }
}
