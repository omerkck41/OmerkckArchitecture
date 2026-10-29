using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Entities;

public sealed class AuditableEntityTests
{
    private sealed class Doc : FullEntity<int>
    {
        public Doc() { }
        public Doc(int id) : base(id) { }
    }

    private sealed class Audited(string id) : AuditableEntity<string>(id);

    private sealed class Evt : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    [Fact]
    public void AuditableEntity_Constructor_SetsIdAndDefaultsAudit()
    {
        var before = DateTime.UtcNow;

        var entity = new Audited("a-1");

        entity.Id.Should().Be("a-1");
        entity.CreatedBy.Should().BeEmpty();
        entity.CreatedDate.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
        entity.ModifiedBy.Should().BeNull();
        entity.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public void FullEntity_Defaults_AreNotDeleted()
    {
        var doc = new Doc(5);

        doc.Id.Should().Be(5);
        doc.IsDeleted.Should().BeFalse();
        doc.DeletedBy.Should().BeNull();
        doc.DeletedDate.Should().BeNull();
        doc.RowVersion.Should().BeNull();
    }

    [Fact]
    public void FullEntity_SetAllProperties_RoundTripsThroughInterfaces()
    {
        var when = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var doc = new Doc
        {
            CreatedBy = "c", CreatedDate = when, ModifiedBy = "m", ModifiedDate = when,
            IsDeleted = true, DeletedBy = "d", DeletedDate = when, RowVersion = [1, 2]
        };

        ((IAuditable)doc).CreatedBy.Should().Be("c");
        ((IAuditable)doc).ModifiedBy.Should().Be("m");
        ((ISoftDeletable)doc).DeletedBy.Should().Be("d");
        doc.DeletedDate.Should().Be(when);
        doc.RowVersion.Should().Equal(1, 2);
    }

    [Fact]
    public void RemoveDomainEvent_EventNotPresent_LeavesCollectionUnchanged()
    {
        var doc = new Doc();
        var kept = new Evt();
        doc.AddDomainEvent(kept);

        doc.RemoveDomainEvent(new Evt());

        doc.DomainEvents.Should().ContainSingle().Which.Should().BeSameAs(kept);
    }

    [Fact]
    public void PageRequest_Defaults_AreIndexZeroSizeTen()
    {
        var request = new PageRequest();

        request.Index.Should().Be(0);
        request.Size.Should().Be(10);
    }

    [Fact]
    public void Paginate_DefaultConstructor_HasEmptyItems()
    {
        var page = new Paginate<int>();

        page.Items.Should().BeEmpty();
        page.TotalRecords.Should().Be(0);
    }

    [Fact]
    public void Create_FromGreaterThanIndex_Throws()
    {
        var act = () => Paginate<int>.Create([], 10, index: 0, size: 5, from: 1);

        act.Should().Throw<ArgumentException>().WithMessage("*From*Index*");
    }

    [Fact]
    public void Create_SizeZero_HasZeroPagesAndIsLastPage()
    {
        var page = Paginate<int>.Create([], 10, index: 0, size: 0);

        page.Pages.Should().Be(0);
        page.HasNext.Should().BeFalse();
        page.IsLastPage.Should().BeTrue();
    }

    [Fact]
    public void Create_OneBasedFrom_ComputesNavigationRelativeToFrom()
    {
        var page = Paginate<int>.Create([1, 2], 6, index: 2, size: 2, from: 1);

        page.Pages.Should().Be(3);
        page.HasPrevious.Should().BeTrue();
        page.HasNext.Should().BeTrue();
        page.IsFirstPage.Should().BeFalse();
        page.IsLastPage.Should().BeFalse();
    }
}
