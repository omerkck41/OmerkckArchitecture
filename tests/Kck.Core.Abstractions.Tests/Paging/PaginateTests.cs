using FluentAssertions;
using Kck.Core.Abstractions.Paging;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Paging;

public sealed class PaginateTests
{
    private static List<int> Items(int count) =>
        Enumerable.Range(1, count).ToList();

    [Fact]
    public void Create_FirstPage_HasCorrectPagingProperties()
    {
        var result = Paginate<int>.Create(Items(10), totalCount: 100, index: 0, size: 10);

        result.Index.Should().Be(0);
        result.Size.Should().Be(10);
        result.Pages.Should().Be(10);
        result.TotalRecords.Should().Be(100);
        result.IsFirstPage.Should().BeTrue();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void Create_LastPage_HasCorrectPagingProperties()
    {
        var result = Paginate<int>.Create(Items(5), totalCount: 25, index: 4, size: 5);

        result.IsLastPage.Should().BeTrue();
        result.HasNext.Should().BeFalse();
        result.Pages.Should().Be(5);
    }

    [Fact]
    public void Create_MiddlePage_HasPreviousAndNext()
    {
        var result = Paginate<int>.Create(Items(10), totalCount: 50, index: 2, size: 10);

        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeTrue();
        result.IsFirstPage.Should().BeFalse();
        result.IsLastPage.Should().BeFalse();
    }

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(101, 10, 11)]
    [InlineData(1, 10, 1)]
    [InlineData(0, 10, 0)]
    [InlineData(10, 3, 4)]
    public void Create_PageCount_MatchesCeilingDivision(int total, int size, int expected)
    {
        var result = Paginate<int>.Create([], totalCount: total, index: 0, size: size);

        result.Pages.Should().Be(expected);
    }

    [Fact]
    public void Create_EmptyItems_HasZeroCount()
    {
        var result = Paginate<int>.Create([], totalCount: 0, index: 0, size: 10);

        result.Items.Should().BeEmpty();
        result.TotalRecords.Should().Be(0);
    }

    [Fact]
    public void Create_WithFrom_IsFirstPageWhenIndexEqualsFrom()
    {
        var result = Paginate<int>.Create(Items(10), totalCount: 100, index: 1, size: 10, from: 1);

        result.IsFirstPage.Should().BeTrue();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void Create_FromGreaterThanIndex_ThrowsArgumentException()
    {
        var act = () => Paginate<int>.Create([], totalCount: 10, index: 0, size: 10, from: 1);

        act.Should().Throw<ArgumentException>().WithMessage("*From*Index*");
    }

    [Fact]
    public void Create_ItemsArePreserved()
    {
        var items = Items(3);
        var result = Paginate<int>.Create(items, totalCount: 3, index: 0, size: 10);

        result.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void DefaultConstructor_HasEmptyItems()
    {
        var paginate = new Paginate<string>();

        paginate.Items.Should().BeEmpty();
    }

    [Fact]
    public void PageRequest_DefaultValues_AreCorrect()
    {
        var req = new PageRequest();

        req.Index.Should().Be(0);
        req.Size.Should().Be(10);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(3, 50)]
    public void PageRequest_CustomValues_AreSet(int index, int size)
    {
        var req = new PageRequest(index, size);

        req.Index.Should().Be(index);
        req.Size.Should().Be(size);
    }

    [Fact]
    public void PageRequest_Equality_WorksCorrectly()
    {
        var a = new PageRequest(1, 20);
        var b = new PageRequest(1, 20);
        var c = new PageRequest(2, 20);

        a.Should().Be(b);
        a.Should().NotBe(c);
    }
}
