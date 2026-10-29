using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Interceptors;

// Soft delete of an owner with cascade dependents and owned types. Every scenario runs through DbContext.Remove under
// the audit interceptor and through a repository delete (with and without the interceptor): both give the same rows.
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class SoftDeleteRelationshipTests : IAsyncLifetime
#pragma warning restore CA1001
{
    public enum DeletePath
    {
        Remove,
        Repository,
        RepositoryWithInterceptor,
    }

    public static TheoryData<DeletePath> Paths => [DeletePath.Remove, DeletePath.Repository, DeletePath.RepositoryWithInterceptor];

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await using var ctx = NewContext();
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _connection.DisposeAsync();

    private RelationshipDbContext NewContext(DeletePath path = DeletePath.Remove)
    {
        var builder = new DbContextOptionsBuilder<RelationshipDbContext>().UseSqlite(_connection);
        if (path != DeletePath.Repository)
            builder.AddInterceptors(new AuditInterceptor());
        return new RelationshipDbContext(builder.Options);
    }

    private static Task SoftDeleteAsync<TEntity>(RelationshipDbContext ctx, TEntity entity, DeletePath path)
        where TEntity : Entity<Guid>
    {
        if (path == DeletePath.Remove)
        {
            ctx.Remove(entity);
            return Task.CompletedTask;
        }

        return new EfRepository<TEntity, Guid>(ctx).DeleteAsync(entity);
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_KeepsHardDeletableDependents(DeletePath path)
    {
        var orderId = Guid.NewGuid();
        await SeedAsync(new Order { Id = orderId, Lines = [new OrderLine { Id = Guid.NewGuid() }, new OrderLine { Id = Guid.NewGuid() }] });

        await using (var ctx = NewContext(path))
        {
            var order = await ctx.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == orderId);
            await SoftDeleteAsync(ctx, order, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.Orders.IgnoreQueryFilters().SingleAsync(o => o.Id == orderId)).IsDeleted.Should().BeTrue();
        (await check.OrderLines.CountAsync(l => l.OrderId == orderId)).Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_SoftDeletesSoftDeletableDependentsDownTheChain(DeletePath path)
    {
        var orderId = Guid.NewGuid();
        await SeedAsync(new Order
        {
            Id = orderId,
            Notes = [new OrderNote { Id = Guid.NewGuid(), Replies = [new NoteReply { Id = Guid.NewGuid() }] }],
        });

        await using (var ctx = NewContext(path))
        {
            var order = await ctx.Orders.Include(o => o.Notes).ThenInclude(n => n.Replies).SingleAsync(o => o.Id == orderId);
            await SoftDeleteAsync(ctx, order, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.OrderNotes.IgnoreQueryFilters().SingleAsync(n => n.OrderId == orderId)).IsDeleted.Should().BeTrue();
        (await check.NoteReplies.IgnoreQueryFilters().SingleAsync()).IsDeleted.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_KeepsOwnedValues(DeletePath path)
    {
        var customerId = Guid.NewGuid();
        await SeedAsync(new Customer { Id = customerId, Address = new Address { City = "Izmir" } });

        await using (var ctx = NewContext(path))
        {
            var customer = await ctx.Customers.SingleAsync(c => c.Id == customerId);
            await SoftDeleteAsync(ctx, customer, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        var stored = await check.Customers.IgnoreQueryFilters().SingleAsync(c => c.Id == customerId);
        stored.IsDeleted.Should().BeTrue();
        stored.Address.Should().NotBeNull();
        stored.Address!.City.Should().Be("Izmir");
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_KeepsTheWholeSubtreeOfAKeptDependent(DeletePath path)
    {
        var orderId = Guid.NewGuid();
        await SeedAsync(new Order
        {
            Id = orderId,
            Lines =
            [
                new OrderLine
                {
                    Id = Guid.NewGuid(),
                    Details = [new LineDetail { Id = Guid.NewGuid() }],
                    Notes = [new LineNote { Id = Guid.NewGuid() }],
                },
            ],
        });

        await using (var ctx = NewContext(path))
        {
            var order = await ctx.Orders
                .Include(o => o.Lines).ThenInclude(l => l.Details)
                .Include(o => o.Lines).ThenInclude(l => l.Notes)
                .SingleAsync(o => o.Id == orderId);
            await SoftDeleteAsync(ctx, order, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.LineDetails.CountAsync()).Should().Be(1);
        // The line stays, so its notes stay with it.
        (await check.LineNotes.IgnoreQueryFilters().SingleAsync()).IsDeleted.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_KeepsDependentsWithoutANavigation(DeletePath path)
    {
        var invoiceId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Invoices.Add(new Invoice { Id = invoiceId });
            seed.InvoiceLines.Add(new InvoiceLine { Id = Guid.NewGuid(), InvoiceId = invoiceId });
            await seed.SaveChangesAsync();
        }

        await using (var ctx = NewContext(path))
        {
            var invoice = await ctx.Invoices.SingleAsync(i => i.Id == invoiceId);
            await ctx.InvoiceLines.Where(l => l.InvoiceId == invoiceId).ToListAsync();
            await SoftDeleteAsync(ctx, invoice, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.InvoiceLines.CountAsync(l => l.InvoiceId == invoiceId)).Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_KeepsOptionalDependentsLinked(DeletePath path)
    {
        var basketId = Guid.NewGuid();
        await SeedAsync(new Basket { Id = basketId, Items = [new BasketItem { Id = Guid.NewGuid() }] });

        await using (var ctx = NewContext(path))
        {
            var basket = await ctx.Baskets.Include(b => b.Items).SingleAsync(b => b.Id == basketId);
            await SoftDeleteAsync(ctx, basket, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.BasketItems.CountAsync(i => i.BasketId == basketId)).Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_LeavesSoftDeletableOptionalDependentsActive(DeletePath path)
    {
        var basketId = Guid.NewGuid();
        await SeedAsync(new Basket { Id = basketId, Coupons = [new BasketCoupon { Id = Guid.NewGuid() }] });

        await using (var ctx = NewContext(path))
        {
            var basket = await ctx.Baskets.Include(b => b.Coupons).SingleAsync(b => b.Id == basketId);
            await SoftDeleteAsync(ctx, basket, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        var coupon = await check.BasketCoupons.IgnoreQueryFilters().SingleAsync();
        coupon.IsDeleted.Should().BeFalse();
        coupon.BasketId.Should().Be(basketId);
        coupon.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public async Task RepositorySoftDelete_OfAnUnsavedEntity_DetachesIt()
    {
        await using (var ctx = NewContext(DeletePath.Repository))
        {
            var order = new Order { Id = Guid.NewGuid() };
            ctx.Orders.Add(order);
            await new EfRepository<Order, Guid>(ctx).DeleteAsync(order);
            ctx.Entry(order).State.Should().Be(EntityState.Detached);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.Orders.IgnoreQueryFilters().AnyAsync()).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_RestoresOnlyTheNulledPartOfACompositeForeignKey(DeletePath path)
    {
        var tenantId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        await SeedAsync(new Cart { Id = cartId, TenantId = tenantId, Lines = [new CartLine { Id = Guid.NewGuid(), TenantId = tenantId }] });

        await using (var ctx = NewContext(path))
        {
            var cart = await ctx.Carts.Include(c => c.Lines).SingleAsync(c => c.Id == cartId);
            await SoftDeleteAsync(ctx, cart, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        var line = await check.CartLines.SingleAsync();
        line.CartId.Should().Be(cartId);
        line.TenantId.Should().Be(tenantId);
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_DoesNotUndoAMoveToAnotherOwner(DeletePath path)
    {
        var basketId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        await SeedAsync(new Basket { Id = basketId, Items = [new BasketItem { Id = itemId }] }, new Basket { Id = otherId });

        await using (var ctx = NewContext(path))
        {
            var basket = await ctx.Baskets.Include(b => b.Items).SingleAsync(b => b.Id == basketId);
            basket.Items.Single().BasketId = otherId;
            await SoftDeleteAsync(ctx, basket, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.BasketItems.SingleAsync(i => i.Id == itemId)).BasketId.Should().Be(otherId);
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_KeepsManyToManyLinks(DeletePath path)
    {
        var postId = Guid.NewGuid();
        await SeedAsync(new Post { Id = postId, Tags = [new Tag { Id = Guid.NewGuid() }] });

        await using (var ctx = NewContext(path))
        {
            var post = await ctx.Posts.Include(p => p.Tags).SingleAsync(p => p.Id == postId);
            await SoftDeleteAsync(ctx, post, path);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        var stored = await check.Posts.IgnoreQueryFilters().Include(p => p.Tags).SingleAsync(p => p.Id == postId);
        stored.Tags.Should().ContainSingle();
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task SoftDelete_DoesNotKeepADependentAnotherRemovedOwnerDeletes(DeletePath path)
    {
        var shipmentId = Guid.NewGuid();
        var carrierId = Guid.NewGuid();
        await SeedAsync(
            new Carrier { Id = carrierId },
            new Shipment { Id = shipmentId, Parcels = [new Parcel { Id = Guid.NewGuid(), CarrierId = carrierId }] });

        await using (var ctx = NewContext(path))
        {
            var shipment = await ctx.Shipments.Include(s => s.Parcels).SingleAsync(s => s.Id == shipmentId);
            var carrier = await ctx.Carriers.SingleAsync(c => c.Id == carrierId);
            ctx.Carriers.Remove(carrier);
            await SoftDeleteAsync(ctx, shipment, path);
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
        await SeedAsync(new Order { Id = orderId, Lines = [new OrderLine { Id = Guid.NewGuid() }] });

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

    [Fact]
    public async Task PermanentDelete_HardDeletesSoftDeletableDependentsWithIt()
    {
        var shipmentId = Guid.NewGuid();
        await SeedAsync(new Shipment { Id = shipmentId, Labels = [new ShipmentLabel { Id = Guid.NewGuid() }] });

        await using (var ctx = NewContext())
        {
            var shipment = await ctx.Shipments.Include(s => s.Labels).SingleAsync(s => s.Id == shipmentId);
            await new EfRepository<Shipment, Guid>(ctx).DeleteAsync(shipment, permanent: true);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.Shipments.IgnoreQueryFilters().AnyAsync()).Should().BeFalse();
        (await check.ShipmentLabels.IgnoreQueryFilters().AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Remove_OfAnOwnerThatIsNotSoftDeletable_HardDeletesSoftDeletableDependentsWithIt()
    {
        var carrierId = Guid.NewGuid();
        await SeedAsync(new Carrier { Id = carrierId, Notes = [new CarrierNote { Id = Guid.NewGuid() }] });

        await using (var ctx = NewContext())
        {
            var carrier = await ctx.Carriers.Include(c => c.Notes).SingleAsync(c => c.Id == carrierId);
            ctx.Carriers.Remove(carrier);
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.Carriers.AnyAsync()).Should().BeFalse();
        (await check.CarrierNotes.IgnoreQueryFilters().AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task RepositorySoftDelete_LeavesEntitiesRemovedEarlierToBeHardDeleted()
    {
        var invoiceId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        await SeedAsync(new Invoice { Id = invoiceId }, new Order { Id = orderId });

        await using (var ctx = NewContext(DeletePath.Repository))
        {
            ctx.Invoices.Remove(await ctx.Invoices.SingleAsync());
            await new EfRepository<Order, Guid>(ctx).DeleteAsync(await ctx.Orders.SingleAsync());
            await ctx.SaveChangesAsync();
        }

        await using var check = NewContext();
        (await check.Invoices.IgnoreQueryFilters().AnyAsync()).Should().BeFalse();
        (await check.Orders.IgnoreQueryFilters().SingleAsync()).IsDeleted.Should().BeTrue();
    }

    private async Task SeedAsync(params object[] entities)
    {
        await using var seed = NewContext();
        seed.AddRange(entities);
        await seed.SaveChangesAsync();
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
        public List<LineNote> Notes { get; set; } = [];
    }

    private sealed class LineDetail : Entity<Guid>
    {
        public Guid OrderLineId { get; set; }
    }

    private sealed class LineNote : FullEntity<Guid>
    {
        public Guid OrderLineId { get; set; }
    }

    private sealed class OrderNote : FullEntity<Guid>
    {
        public Guid OrderId { get; set; }
        public List<NoteReply> Replies { get; set; } = [];
    }

    private sealed class NoteReply : FullEntity<Guid>
    {
        public Guid OrderNoteId { get; set; }
    }

    private sealed class Invoice : FullEntity<Guid>;

    private sealed class InvoiceLine : Entity<Guid>
    {
        public Guid InvoiceId { get; set; }
    }

    private sealed class Basket : FullEntity<Guid>
    {
        public List<BasketItem> Items { get; set; } = [];
        public List<BasketCoupon> Coupons { get; set; } = [];
    }

    private sealed class BasketItem : Entity<Guid>
    {
        public Guid? BasketId { get; set; }
    }

    private sealed class BasketCoupon : FullEntity<Guid>
    {
        public Guid? BasketId { get; set; }
    }

    private sealed class Cart : FullEntity<Guid>
    {
        public Guid TenantId { get; set; }
        public List<CartLine> Lines { get; set; } = [];
    }

    private sealed class CartLine : Entity<Guid>
    {
        public Guid TenantId { get; set; }
        public Guid? CartId { get; set; }
    }

    private sealed class Post : FullEntity<Guid>
    {
        public List<Tag> Tags { get; set; } = [];
    }

    private sealed class Tag : Entity<Guid>;

    private sealed class Shipment : FullEntity<Guid>
    {
        public List<Parcel> Parcels { get; set; } = [];
        public List<ShipmentLabel> Labels { get; set; } = [];
    }

    private sealed class Parcel : Entity<Guid>
    {
        public Guid ShipmentId { get; set; }
        public Guid CarrierId { get; set; }
    }

    private sealed class ShipmentLabel : FullEntity<Guid>
    {
        public Guid ShipmentId { get; set; }
    }

    private sealed class Carrier : Entity<Guid>
    {
        public List<CarrierNote> Notes { get; set; } = [];
    }

    private sealed class CarrierNote : FullEntity<Guid>
    {
        public Guid CarrierId { get; set; }
    }

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
        public DbSet<NoteReply> NoteReplies => Set<NoteReply>();
        public DbSet<LineDetail> LineDetails => Set<LineDetail>();
        public DbSet<LineNote> LineNotes => Set<LineNote>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
        public DbSet<Basket> Baskets => Set<Basket>();
        public DbSet<BasketItem> BasketItems => Set<BasketItem>();
        public DbSet<BasketCoupon> BasketCoupons => Set<BasketCoupon>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartLine> CartLines => Set<CartLine>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<Parcel> Parcels => Set<Parcel>();
        public DbSet<ShipmentLabel> ShipmentLabels => Set<ShipmentLabel>();
        public DbSet<Carrier> Carriers => Set<Carrier>();
        public DbSet<CarrierNote> CarrierNotes => Set<CarrierNote>();
        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(b =>
            {
                b.HasMany(e => e.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
                b.HasMany(e => e.Notes).WithOne().HasForeignKey(n => n.OrderId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<OrderLine>(b =>
            {
                b.HasMany(l => l.Details).WithOne().HasForeignKey(d => d.OrderLineId).OnDelete(DeleteBehavior.Cascade);
                b.HasMany(l => l.Notes).WithOne().HasForeignKey(n => n.OrderLineId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<OrderNote>()
                .HasMany(n => n.Replies).WithOne().HasForeignKey(r => r.OrderNoteId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<InvoiceLine>()
                .HasOne<Invoice>().WithMany().HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Basket>(b =>
            {
                b.HasMany(e => e.Items).WithOne().HasForeignKey(i => i.BasketId);
                b.HasMany(e => e.Coupons).WithOne().HasForeignKey(c => c.BasketId);
            });
            modelBuilder.Entity<Cart>(b =>
            {
                b.HasAlternateKey(c => new { c.TenantId, c.Id });
                b.HasMany(c => c.Lines).WithOne()
                    .HasForeignKey(l => new { l.TenantId, l.CartId })
                    .HasPrincipalKey(c => new { c.TenantId, c.Id });
            });
            // ClientCascade: the database FK restricts, as with SQL Server's "multiple cascade paths" workaround.
            modelBuilder.Entity<Shipment>(b =>
            {
                b.HasMany(e => e.Parcels).WithOne().HasForeignKey(p => p.ShipmentId).OnDelete(DeleteBehavior.ClientCascade);
                b.HasMany(e => e.Labels).WithOne().HasForeignKey(l => l.ShipmentId).OnDelete(DeleteBehavior.ClientCascade);
            });
            modelBuilder.Entity<Carrier>(b =>
            {
                b.HasMany<Parcel>().WithOne().HasForeignKey(p => p.CarrierId).OnDelete(DeleteBehavior.ClientCascade);
                b.HasMany(c => c.Notes).WithOne().HasForeignKey(n => n.CarrierId).OnDelete(DeleteBehavior.ClientCascade);
            });
            modelBuilder.Entity<Post>(b => b.HasMany(e => e.Tags).WithMany());
            modelBuilder.Entity<Customer>(b => b.OwnsOne(e => e.Address));

            // SQLite has no row version column type.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => t.ClrType.IsAssignableTo(typeof(ISoftDeletable))))
                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(FullEntity<Guid>.RowVersion));
        }
    }
}
