using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Extensions;
//#if (IncludeSample)
using KckWebApiTemplate.Api.Orders;
//#endif
using Microsoft.EntityFrameworkCore;

namespace KckWebApiTemplate.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
//#if (IncludeSample)
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

//#endif
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
//#if (IncludeSample)
        modelBuilder.Entity<Order>(order =>
        {
            // Cascade: cancelling (soft-deleting) an order soft-deletes its lines too. Restrict: never the customer.
            order.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
            order.HasOne<Customer>().WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
            order.Navigation(o => o.Lines).AutoInclude();
        });
        modelBuilder.Entity<OrderLine>(line =>
        {
            line.Property(l => l.Sku).HasMaxLength(Product.SkuMaxLength);
            line.Property(l => l.UnitPrice).HasPrecision(18, 2);
        });
        modelBuilder.Entity<Product>(product =>
        {
            product.Property(p => p.Id).HasMaxLength(Product.SkuMaxLength);
            product.Property(p => p.Name).HasMaxLength(200);
            product.Property(p => p.UnitPrice).HasPrecision(18, 2);
            product.HasData(SampleData.Products);
        });
        modelBuilder.Entity<Customer>(customer =>
        {
            customer.Property(c => c.Name).HasMaxLength(200);
            customer.HasData(SampleData.Customers);
        });

//#endif
        // Soft-deleted rows are hidden from every query unless a query asks for them (QueryOptions.IncludeDeleted).
        modelBuilder.ApplyGlobalSoftDeleteQueryFilter();

//#if (!UseSqlServer)
        // FullEntity.RowVersion is a [Timestamp] column that only SQL Server generates. This provider does not, so the
        // column is not mapped and optimistic concurrency checks are off (README.md, "Concurrency").
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => t.FindProperty(nameof(FullEntity<Guid>.RowVersion)) is not null))
        {
            modelBuilder.Entity(entityType.ClrType).Ignore(nameof(FullEntity<Guid>.RowVersion));
        }
//#endif
    }
}
