using AwesomeAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Abstractions;

internal static class FilterFactory
{
    public static Filter F(string field, string op, object? value, string? logic = null, IEnumerable<Filter>? filters = null) =>
        new() { Field = field, Operator = op, Value = value, Logic = logic, Filters = filters };

    public static Filter F(string field, FilterOperator op, object? value, string? logic = null, IEnumerable<Filter>? filters = null) =>
        F(field, op.ToString().ToLowerInvariant(), value, logic, filters);

    public static Sort S(string field, string dir, int priority = 0) =>
        new() { Field = field, Dir = dir, Priority = priority };
}

#pragma warning disable IL2026, IL3050
public sealed class DynamicQueryTests
{
    private sealed class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Nick { get; set; }
    }

    private static readonly Person[] Data =
    [
        new() { Name = "Alice", Age = 30, Nick = null },
        new() { Name = "Bob", Age = 25, Nick = "bobby" },
        new() { Name = "Carol", Age = 30, Nick = "cc" },
        new() { Name = "Dave", Age = 41, Nick = null },
    ];

    private static string[] Run(DynamicQuery q) =>
        Data.AsQueryable().ToDynamic(q).Select(p => p.Name).ToArray();

    private static string[] RunFilter(Filter f) => Run(new DynamicQuery([], f));

    // --- operators ---

    [Fact]
    public void ToDynamic_EqOnInt_FiltersExactMatches() =>
        RunFilter(FilterFactory.F("Age", FilterOperator.Eq, 30)).Should().Equal("Alice", "Carol");

    [Fact]
    public void ToDynamic_Neq_ExcludesMatches() =>
        RunFilter(FilterFactory.F("Age", FilterOperator.Neq, 30)).Should().Equal("Bob", "Dave");

    [Fact]
    public void ToDynamic_Lt_IsStrict() =>
        RunFilter(FilterFactory.F("Age", FilterOperator.Lt, 30)).Should().Equal("Bob");

    [Fact]
    public void ToDynamic_Lte_IsInclusive() =>
        RunFilter(FilterFactory.F("Age", FilterOperator.Lte, 30)).Should().Equal("Alice", "Bob", "Carol");

    [Fact]
    public void ToDynamic_Gt_IsStrict() =>
        RunFilter(FilterFactory.F("Age", FilterOperator.Gt, 30)).Should().Equal("Dave");

    [Fact]
    public void ToDynamic_Gte_IsInclusive() =>
        RunFilter(FilterFactory.F("Age", FilterOperator.Gte, 30)).Should().Equal("Alice", "Carol", "Dave");

    [Fact]
    public void ToDynamic_Contains_MatchesSubstring() =>
        RunFilter(FilterFactory.F("Name", FilterOperator.Contains, "ar")).Should().Equal("Carol");

    [Fact]
    public void ToDynamic_StartsWith_MatchesPrefixOnly() =>
        RunFilter(FilterFactory.F("Name", FilterOperator.StartsWith, "A")).Should().Equal("Alice");

    [Fact]
    public void ToDynamic_EndsWith_MatchesSuffixOnly() =>
        RunFilter(FilterFactory.F("Name", FilterOperator.EndsWith, "e")).Should().Equal("Alice", "Dave");

    [Fact]
    public void ToDynamic_IsNull_ReturnsNullRows() =>
        RunFilter(FilterFactory.F("Nick", FilterOperator.IsNull, null)).Should().Equal("Alice", "Dave");

    [Fact]
    public void ToDynamic_IsNotNull_ReturnsNonNullRows() =>
        RunFilter(FilterFactory.F("Nick", FilterOperator.IsNotNull, null)).Should().Equal("Bob", "Carol");

    [Fact]
    public void ToDynamic_OperatorStringUpperCase_IsParsedCaseInsensitively() =>
        RunFilter(FilterFactory.F("Age", "GT", 30)).Should().Equal("Dave");

    [Fact]
    public void ToDynamic_UnknownOperator_ThrowsArgumentException()
    {
        var act = () => RunFilter(FilterFactory.F("Age", "bogus", 1));

        act.Should().Throw<ArgumentException>().WithMessage("*bogus*not supported*");
    }

    [Fact]
    public void ToDynamic_UnknownField_Throws()
    {
        var act = () => RunFilter(FilterFactory.F("Nope", FilterOperator.Eq, 1));

        act.Should().Throw<ArgumentException>();
    }

    // --- groups ---

    [Fact]
    public void ToDynamic_GroupDefaultLogic_CombinesWithAnd()
    {
        var group = FilterFactory.F("", "eq", null, filters:
        [
            FilterFactory.F("Age", FilterOperator.Eq, 30),
            FilterFactory.F("Name", FilterOperator.StartsWith, "C"),
        ]);

        RunFilter(group).Should().Equal("Carol");
    }

    [Fact]
    public void ToDynamic_GroupOrLogic_CombinesWithOr()
    {
        var group = FilterFactory.F("", "eq", null, logic: "OR", filters:
        [
            FilterFactory.F("Age", FilterOperator.Eq, 25),
            FilterFactory.F("Age", FilterOperator.Eq, 41),
        ]);

        RunFilter(group).Should().Equal("Bob", "Dave");
    }

    [Fact]
    public void ToDynamic_EmptyGroup_AppliesNoFilter()
    {
        var group = FilterFactory.F("", "eq", null, filters: []);

        RunFilter(group).Should().Equal("Alice", "Bob", "Carol", "Dave");
    }

    [Fact]
    public void ToDynamic_NestedGroups_Evaluate()
    {
        var inner = FilterFactory.F("", "eq", null, logic: "or", filters:
        [
            FilterFactory.F("Name", FilterOperator.EndsWith, "b"),
            FilterFactory.F("Name", FilterOperator.EndsWith, "l"),
        ]);
        var outer = FilterFactory.F("", "eq", null, filters: [inner, FilterFactory.F("Age", FilterOperator.Lt, 30)]);

        RunFilter(outer).Should().Equal("Bob");
    }

    // --- sorting ---

    [Fact]
    public void ToDynamic_NoFilterNoSort_ReturnsSourceOrder() =>
        Run(new DynamicQuery()).Should().Equal("Alice", "Bob", "Carol", "Dave");

    [Fact]
    public void ToDynamic_SortAsc_OrdersAscending() =>
        Run(new DynamicQuery([FilterFactory.S("Age", "asc")], null)).Should().Equal("Bob", "Alice", "Carol", "Dave");

    [Fact]
    public void ToDynamic_SortDescAnyOtherDir_OrdersDescending() =>
        Run(new DynamicQuery([FilterFactory.S("Age", "desc")], null)).Should().Equal("Dave", "Alice", "Carol", "Bob");

    [Fact]
    public void ToDynamic_SortDirAscUpperCase_IsCaseInsensitive() =>
        Run(new DynamicQuery([FilterFactory.S("Name", "ASC")], null)).Should().Equal("Alice", "Bob", "Carol", "Dave");

    [Fact]
    public void ToDynamic_MultipleSorts_AppliedByPriority()
    {
        var sorts = new[]
        {
            FilterFactory.S("Name", "desc", priority: 2),
            FilterFactory.S("Age", "asc", priority: 1),
        };

        Run(new DynamicQuery(sorts, null)).Should().Equal("Bob", "Carol", "Alice", "Dave");
    }

    [Fact]
    public void ToDynamic_ThenBySecondAsc_UsesAscending()
    {
        var sorts = new[]
        {
            FilterFactory.S("Age", "desc", priority: 1),
            FilterFactory.S("Name", "asc", priority: 2),
        };

        Run(new DynamicQuery(sorts, null)).Should().Equal("Dave", "Alice", "Carol", "Bob");
    }

    [Fact]
    public void ToDynamic_FilterAndSort_AppliesBoth()
    {
        var q = new DynamicQuery([FilterFactory.S("Name", "desc")], FilterFactory.F("Age", FilterOperator.Eq, 30));

        Run(q).Should().Equal("Carol", "Alice");
    }

    // --- DynamicQuery / Sort / Filter models ---

    [Fact]
    public void DynamicQuery_NullSort_BecomesEmpty()
    {
        var q = new DynamicQuery(null, null);

        q.Sort.Should().BeEmpty();
        q.Filter.Should().BeNull();
    }

    [Fact]
    public void Sort_Initializer_SetsFieldsAndDefaultPriority()
    {
        var s = FilterFactory.S("Age", "asc");

        s.Field.Should().Be("Age");
        s.Dir.Should().Be("asc");
        s.Priority.Should().Be(0);
    }

    [Theory]
    [InlineData("eq", FilterOperator.Eq)]
    [InlineData("isnotnull", FilterOperator.IsNotNull)]
    [InlineData("startswith", FilterOperator.StartsWith)]
    public void OperatorEnum_ParsesOperatorString(string op, FilterOperator expected) =>
        FilterFactory.F("F", op, 1).OperatorEnum.Should().Be(expected);

    [Theory]
    [InlineData("EQ", FilterOperator.Eq)]
    [InlineData("isNull", FilterOperator.IsNull)]
    [InlineData("contains", FilterOperator.Contains)]
    public void TryParseOperator_KnownValue_IgnoresCase(string input, FilterOperator expected) =>
        Filter.TryParseOperator(input).Should().Be(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nope")]
    public void TryParseOperator_NullBlankOrUnknown_ReturnsNull(string? input) =>
        Filter.TryParseOperator(input).Should().BeNull();
}
#pragma warning restore IL2026, IL3050


