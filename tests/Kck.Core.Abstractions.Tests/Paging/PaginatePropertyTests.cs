using CsCheck;
using FluentAssertions;
using Kck.Core.Abstractions.Paging;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Paging;

/// <summary>
/// Faz-B3: CsCheck property-based tests for Paginate.Create math invariants.
/// </summary>
public sealed class PaginatePropertyTests
{
    [Fact]
    public void Create_Pages_AlwaysCeilingDivision()
    {
        Gen.Select(Gen.Int[1, 100], Gen.Int[0, 10_000])
            .Sample((size, total) =>
            {
                var result = Paginate<int>.Create([], total, 0, size);
                var expected = (int)Math.Ceiling(total / (double)size);
                result.Pages.Should().Be(expected);
            });
    }

    [Fact]
    public void Create_TotalRecords_AlwaysMatchesInput()
    {
        Gen.Select(Gen.Int[1, 50], Gen.Int[0, 5_000])
            .Sample((size, total) =>
            {
                var result = Paginate<int>.Create([], total, 0, size);
                result.TotalRecords.Should().Be(total);
                result.Count.Should().Be(total);
            });
    }

    [Fact]
    public void Create_FirstPage_IsFirstPageAlwaysTrue()
    {
        Gen.Int[1, 100].Sample(size =>
        {
            var result = Paginate<int>.Create([], 1000, index: 0, size: size);
            result.IsFirstPage.Should().BeTrue();
            result.HasPrevious.Should().BeFalse();
        });
    }

    [Fact]
    public void Create_HasNext_ConsistentWithPagesAndIndex()
    {
        Gen.Select(Gen.Int[1, 50], Gen.Int[1, 1_000])
            .Sample((size, total) =>
            {
                var pages = (int)Math.Ceiling(total / (double)size);
                if (pages < 2) return;

                var index = pages / 2;
                var result = Paginate<int>.Create([], total, index, size);

                var expectedHasNext = index + 1 < pages;
                result.HasNext.Should().Be(expectedHasNext,
                    $"index={index}, pages={pages}, size={size}, total={total}");
            });
    }

    [Fact]
    public void Create_FromEqualsIndex_IsFirstPage()
    {
        Gen.Select(Gen.Int[0, 10], Gen.Int[1, 50])
            .Sample((from, size) =>
            {
                var result = Paginate<int>.Create([], 500, index: from, size: size, from: from);
                result.IsFirstPage.Should().BeTrue($"from={from} equals index={from}");
                result.HasPrevious.Should().BeFalse();
            });
    }
}
