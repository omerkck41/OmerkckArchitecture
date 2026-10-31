using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Sample.Orders.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kck.Sample.Orders.Data;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    // Dates are written in UTC (audit fields); read them back as UTC too, so JSON shows the "Z" suffix.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.UseUtcDateTimes();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(order =>
        {
            // The delete rules decide what a soft delete takes along. Cascade: cancelling an order soft-deletes its
            // (auto-included, so tracked) lines. Restrict: a customer with orders cannot be deleted.
            order.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
            order.HasOne<Customer>().WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(16);
            order.Navigation(o => o.Lines).AutoInclude();

            // Concurrency tokens (RowVersion is SQL Server only, see below): saving an order that another request paid,
            // shipped or cancelled since it was read throws DbUpdateConcurrencyException, which the handlers return as
            // 409. Without them two requests could both pay an order, or cancel a shipped one.
            order.Property(o => o.Status).IsConcurrencyToken();
            order.Property(o => o.IsDeleted).IsConcurrencyToken();
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
            customer.Property(c => c.Name).HasMaxLength(Customer.NameMaxLength);
            customer.HasData(SampleData.Customers);
        });

        // Soft-deleted rows are hidden from every query unless a query asks for them (QueryOptions.WithDeleted).
        modelBuilder.ApplyGlobalSoftDeleteQueryFilter();

        // FullEntity.RowVersion is a [Timestamp] column that only SQL Server generates; SQLite and PostgreSQL do not,
        // so it is not mapped and optimistic concurrency checks are off.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => t.FindProperty(nameof(FullEntity<Guid>.RowVersion)) is not null))
        {
            modelBuilder.Entity(entityType.ClrType).Ignore(nameof(FullEntity<Guid>.RowVersion));
        }
    }
}
