using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Interceptors;

// DbContext.Remove on a soft-deletable owner, with cascade dependents and owned types, under the audit interceptor.
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class AuditInterceptorRelationshipTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await using var ctx = NewContext();
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _connection.DisposeAsync();

    private RelationshipDbContext NewContext() => new(new DbContextOptionsBuilder<RelationshipDbContext>()
        .UseSqlite(_connection)
        .AddInterceptors(new AuditInterceptor())
        .Options);

    [Fact]
    public async Task Remove_SoftDeletableOwner_KeepsItsHardDeletableDependents()
    {
        var orderId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Orders.Add(new Order { Id = orderId, Lines = [new OrderLine { Id = Guid.NewGuid() }, new OrderLine { Id = Guid.NewGuid() }] });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var order = await ctx.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == orderId);
            ctx.Orders.Remove(order);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.Orders.IgnoreQueryFilters().SingleAsync(o => o.Id == orderId)).IsDeleted.Should().BeTrue();
        (await check.OrderLines.CountAsync(l => l.OrderId == orderId)).Should().Be(2);
    }

    [Fact]
    public async Task Remove_SoftDeletableOwner_SoftDeletesItsSoftDeletableDependents()
    {
        var orderId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Orders.Add(new Order { Id = orderId, Notes = [new OrderNote { Id = Guid.NewGuid() }] });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var order = await ctx.Orders.Include(o => o.Notes).SingleAsync(o => o.Id == orderId);
            ctx.Orders.Remove(order);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        var notes = await check.OrderNotes.IgnoreQueryFilters().Where(n => n.OrderId == orderId).ToListAsync();
        notes.Should().ContainSingle().Which.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Remove_SoftDeletableOwner_KeepsItsOwnedValues()
    {
        var customerId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Customers.Add(new Customer { Id = customerId, Address = new Address { City = "Izmir" } });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var customer = await ctx.Customers.SingleAsync(c => c.Id == customerId);
            ctx.Customers.Remove(customer);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        var stored = await check.Customers.IgnoreQueryFilters().SingleAsync(c => c.Id == customerId);
        stored.IsDeleted.Should().BeTrue();
        stored.Address.Should().NotBeNull();
        stored.Address!.City.Should().Be("Izmir");
    }

    [Fact]
    public async Task Remove_SoftDeletableOwner_KeepsDependentsOfDependents()
    {
        var orderId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Orders.Add(new Order
            {
                Id = orderId,
                Lines = [new OrderLine { Id = Guid.NewGuid(), Details = [new LineDetail { Id = Guid.NewGuid() }] }],
            });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var order = await ctx.Orders.Include(o => o.Lines).ThenInclude(l => l.Details).SingleAsync(o => o.Id == orderId);
            ctx.Orders.Remove(order);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.LineDetails.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Remove_SoftDeletableOwner_KeepsDependentsWithoutANavigation()
    {
        var invoiceId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Invoices.Add(new Invoice { Id = invoiceId });
            seed.InvoiceLines.Add(new InvoiceLine { Id = Guid.NewGuid(), InvoiceId = invoiceId });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var invoice = await ctx.Invoices.SingleAsync(i => i.Id == invoiceId);
            await ctx.InvoiceLines.Where(l => l.InvoiceId == invoiceId).ToListAsync();
            ctx.Invoices.Remove(invoice);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.InvoiceLines.CountAsync(l => l.InvoiceId == invoiceId)).Should().Be(1);
    }

    [Fact]
    public async Task Remove_SoftDeletableOwner_KeepsOptionalDependentsLinked()
    {
        var basketId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Baskets.Add(new Basket { Id = basketId, Items = [new BasketItem { Id = Guid.NewGuid() }] });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var basket = await ctx.Baskets.Include(b => b.Items).SingleAsync(b => b.Id == basketId);
            ctx.Baskets.Remove(basket);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.BasketItems.CountAsync(i => i.BasketId == basketId)).Should().Be(1);
    }

    [Fact]
    public async Task Remove_SoftDeletableOwner_DoesNotUndoAMoveToAnotherOwner()
    {
        var basketId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Baskets.Add(new Basket { Id = basketId, Items = [new BasketItem { Id = itemId }] });
            seed.Baskets.Add(new Basket { Id = otherId });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var basket = await ctx.Baskets.Include(b => b.Items).SingleAsync(b => b.Id == basketId);
            basket.Items.Single().BasketId = otherId;
            ctx.Baskets.Remove(basket);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.BasketItems.SingleAsync(i => i.Id == itemId)).BasketId.Should().Be(otherId);
    }

    [Fact]
    public async Task Remove_SoftDeletableOwner_KeepsManyToManyLinks()
    {
        var postId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Posts.Add(new Post { Id = postId, Tags = [new Tag { Id = Guid.NewGuid() }] });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var post = await ctx.Posts.Include(p => p.Tags).SingleAsync(p => p.Id == postId);
            ctx.Posts.Remove(post);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        var stored = await check.Posts.IgnoreQueryFilters().Include(p => p.Tags).SingleAsync(p => p.Id == postId);
        stored.Tags.Should().ContainSingle();
    }

    [Fact]
    public async Task Remove_SoftDeletableOwner_DoesNotKeepADependentAnotherRemovedOwnerDeletes()
    {
        var shipmentId = Guid.NewGuid();
        var carrierId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Carriers.Add(new Carrier { Id = carrierId });
            seed.Shipments.Add(new Shipment
            {
                Id = shipmentId,
                Parcels = [new Parcel { Id = Guid.NewGuid(), CarrierId = carrierId }],
            });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var shipment = await ctx.Shipments.Include(s => s.Parcels).SingleAsync(s => s.Id == shipmentId);
            var carrier = await ctx.Carriers.SingleAsync(c => c.Id == carrierId);
            ctx.Carriers.Remove(carrier);
            ctx.Shipments.Remove(shipment);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.Carriers.AnyAsync(c => c.Id == carrierId)).Should().BeFalse();
        (await check.Parcels.AnyAsync()).Should().BeFalse();
        (await check.Shipments.IgnoreQueryFilters().SingleAsync(s => s.Id == shipmentId)).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task PermanentDelete_OfSoftDeletableOwner_StillCascadesToDependents()
    {
        var orderId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Orders.Add(new Order { Id = orderId, Lines = [new OrderLine { Id = Guid.NewGuid() }] });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var order = await ctx.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == orderId);
            await new EfRepository<Order, Guid>(ctx).DeleteAsync(order, permanent: true);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.Orders.IgnoreQueryFilters().AnyAsync(o => o.Id == orderId)).Should().BeFalse();
        (await check.OrderLines.CountAsync(l => l.OrderId == orderId)).Should().Be(0);
    }

    private sealed class Order : FullEntity<Guid>
    {
        public List<OrderLine> Lines { get; set; } = [];
        public List<OrderNote> Notes { get; set; } = [];
    }

    private sealed class OrderLine : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public List<LineDetail> Details { get; set; } = [];
    }

    private sealed class LineDetail : Entity<Guid>
    {
        public Guid OrderLineId { get; set; }
    }

    private sealed class OrderNote : FullEntity<Guid>
    {
        public Guid OrderId { get; set; }
    }

    private sealed class Invoice : FullEntity<Guid>;

    private sealed class InvoiceLine : Entity<Guid>
    {
        public Guid InvoiceId { get; set; }
    }

    private sealed class Basket : FullEntity<Guid>
    {
        public List<BasketItem> Items { get; set; } = [];
    }

    private sealed class BasketItem : Entity<Guid>
    {
        public Guid? BasketId { get; set; }
    }

    private sealed class Post : FullEntity<Guid>
    {
        public List<Tag> Tags { get; set; } = [];
    }

    private sealed class Tag : Entity<Guid>;

    private sealed class Shipment : FullEntity<Guid>
    {
        public List<Parcel> Parcels { get; set; } = [];
    }

    private sealed class Parcel : Entity<Guid>
    {
        public Guid ShipmentId { get; set; }
        public Guid CarrierId { get; set; }
    }

    private sealed class Carrier : Entity<Guid>;

    private sealed class Customer : FullEntity<Guid>
    {
        public Address? Address { get; set; }
    }

    private sealed class Address
    {
        public string City { get; set; } = string.Empty;
    }

    private sealed class RelationshipDbContext(DbContextOptions<RelationshipDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();
        public DbSet<OrderNote> OrderNotes => Set<OrderNote>();
        public DbSet<LineDetail> LineDetails => Set<LineDetail>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
        public DbSet<Basket> Baskets => Set<Basket>();
        public DbSet<BasketItem> BasketItems => Set<BasketItem>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<Parcel> Parcels => Set<Parcel>();
        public DbSet<Carrier> Carriers => Set<Carrier>();
        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(b =>
            {
                b.Ignore(e => e.RowVersion);
                b.HasMany(e => e.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
                b.HasMany(e => e.Notes).WithOne().HasForeignKey(n => n.OrderId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<OrderLine>()
                .HasMany(l => l.Details).WithOne().HasForeignKey(d => d.OrderLineId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderNote>(b => b.Ignore(e => e.RowVersion));
            modelBuilder.Entity<Invoice>(b => b.Ignore(e => e.RowVersion));
            modelBuilder.Entity<InvoiceLine>()
                .HasOne<Invoice>().WithMany().HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Basket>(b =>
            {
                b.Ignore(e => e.RowVersion);
                b.HasMany(e => e.Items).WithOne().HasForeignKey(i => i.BasketId);
            });
            // ClientCascade: the database FK restricts, as with SQL Server's "multiple cascade paths" workaround.
            modelBuilder.Entity<Shipment>(b =>
            {
                b.Ignore(e => e.RowVersion);
                b.HasMany(e => e.Parcels).WithOne().HasForeignKey(p => p.ShipmentId).OnDelete(DeleteBehavior.ClientCascade);
            });
            modelBuilder.Entity<Parcel>()
                .HasOne<Carrier>().WithMany().HasForeignKey(p => p.CarrierId).OnDelete(DeleteBehavior.ClientCascade);
            modelBuilder.Entity<Post>(b =>
            {
                b.Ignore(e => e.RowVersion);
                b.HasMany(e => e.Tags).WithMany();
            });
            modelBuilder.Entity<Customer>(b =>
            {
                b.Ignore(e => e.RowVersion);
                b.OwnsOne(e => e.Address);
            });
        }
    }
}
