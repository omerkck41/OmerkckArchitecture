using FluentAssertions;
using Kck.Core.Abstractions.Entities;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Entities;

public sealed class EntityTests
{
    private sealed class Order : Entity<Guid>
    {
        public Order(Guid id) : base(id) { }
        public Order() { }
    }

    private sealed class OrderCreated : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    [Fact]
    public void Constructor_WithId_SetsId()
    {
        var id = Guid.NewGuid();
        var order = new Order(id);

        order.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_Default_HasDefaultId()
    {
        var order = new Order();

        order.Id.Should().Be(default(Guid));
    }

    [Fact]
    public void AddDomainEvent_SingleEvent_AppearsInCollection()
    {
        var order = new Order(Guid.NewGuid());
        var evt = new OrderCreated();

        order.AddDomainEvent(evt);

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(evt);
    }

    [Fact]
    public void AddDomainEvent_MultipleEvents_AllAppearInCollection()
    {
        var order = new Order(Guid.NewGuid());
        var evt1 = new OrderCreated();
        var evt2 = new OrderCreated();

        order.AddDomainEvent(evt1);
        order.AddDomainEvent(evt2);

        order.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveDomainEvent_ExistingEvent_RemovesItFromCollection()
    {
        var order = new Order(Guid.NewGuid());
        var evt = new OrderCreated();
        order.AddDomainEvent(evt);

        order.RemoveDomainEvent(evt);

        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WithMultipleEvents_EmptiesCollection()
    {
        var order = new Order(Guid.NewGuid());
        order.AddDomainEvent(new OrderCreated());
        order.AddDomainEvent(new OrderCreated());

        order.ClearDomainEvents();

        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_IsReadOnly_DoesNotExposeInternalList()
    {
        var order = new Order(Guid.NewGuid());

        order.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }

    [Fact]
    public void AuditFields_DefaultValues_AreSet()
    {
        var order = new Order(Guid.NewGuid());

        order.CreatedBy.Should().Be(string.Empty);
        order.IsDeleted.Should().BeFalse();
        order.ModifiedBy.Should().BeNull();
        order.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void SoftDelete_SetIsDeleted_UpdatesFields()
    {
        var order = new Order(Guid.NewGuid());

        order.IsDeleted = true;
        order.DeletedBy = "admin";
        order.DeletedDate = DateTime.UtcNow;

        order.IsDeleted.Should().BeTrue();
        order.DeletedBy.Should().Be("admin");
        order.DeletedDate.Should().NotBeNull();
    }
}
