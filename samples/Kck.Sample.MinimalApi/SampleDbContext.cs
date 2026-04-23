using Kck.Persistence.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kck.Sample.MinimalApi;

public sealed class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
    }
}
