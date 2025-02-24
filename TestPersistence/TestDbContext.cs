using Core.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace TestPersistence;

public class TestDbContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global soft delete filter'ı uygula
        modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit bilgilerini uygulayan extension method çağrısı
        ChangeTracker.ApplyAuditInformation("ArchonApp-System");
        return await base.SaveChangesAsync(cancellationToken);
    }
}