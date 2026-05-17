using CsCheck;
using FluentAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Dynamic;

/// <summary>
/// Faz-B3: Unit + property-based tests for Filter.GetValue&lt;T&gt; type conversions.
/// </summary>
public sealed class FilterGetValueTests
{
    private static Filter Build(object? value) =>
        new Filter { Field = "f", Operator = "eq", Value = value };

    // --- Unit tests ---

    [Fact]
    public void GetValue_WhenValueIsNull_ReturnsDefault()
    {
        var filter = Build(null);
#pragma warning disable IL2026, IL3050
        filter.GetValue<int>().Should().Be(0);
        filter.GetValue<string>().Should().BeNull();
#pragma warning restore IL2026, IL3050
    }

    [Fact]
    public void GetValue_WhenValueIsDirectType_ReturnsDirectly()
    {
        var filter = Build(42);
#pragma warning disable IL2026, IL3050
        filter.GetValue<int>().Should().Be(42);
#pragma warning restore IL2026, IL3050
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("0", 0)]
    [InlineData("-1", -1)]
    public void GetValue_WhenValueIsStringInt_ParsesCorrectly(string input, int expected)
    {
        var filter = Build(input);
#pragma warning disable IL2026, IL3050
        filter.GetValue<int>().Should().Be(expected);
#pragma warning restore IL2026, IL3050
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("True", true)]
    [InlineData("False", false)]
    public void GetValue_WhenValueIsStringBool_ParsesCorrectly(string input, bool expected)
    {
        var filter = Build(input);
#pragma warning disable IL2026, IL3050
        filter.GetValue<bool>().Should().Be(expected);
#pragma warning restore IL2026, IL3050
    }

    [Fact]
    public void GetValue_WhenValueIsStringGuid_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var filter = Build(guid.ToString());
#pragma warning disable IL2026, IL3050
        filter.GetValue<Guid>().Should().Be(guid);
#pragma warning restore IL2026, IL3050
    }

    [Fact]
    public void GetValue_WhenValueIsString_ReturnsString()
    {
        var filter = Build("hello");
#pragma warning disable IL2026, IL3050
        filter.GetValue<string>().Should().Be("hello");
#pragma warning restore IL2026, IL3050
    }

    // --- Property-based tests ---

    [Fact]
    public void GetValue_IntDirectCast_RoundTrips()
    {
        Gen.Int.Sample(value =>
        {
            var filter = Build(value);
#pragma warning disable IL2026, IL3050
            filter.GetValue<int>().Should().Be(value);
#pragma warning restore IL2026, IL3050
        });
    }

    [Fact]
    public void GetValue_IntFromString_RoundTrips()
    {
        Gen.Int.Sample(value =>
        {
            var filter = Build(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
#pragma warning disable IL2026, IL3050
            filter.GetValue<int>().Should().Be(value);
#pragma warning restore IL2026, IL3050
        });
    }

    [Fact]
    public void GetValue_LongDirectCast_RoundTrips()
    {
        Gen.Long.Sample(value =>
        {
            var filter = Build(value);
#pragma warning disable IL2026, IL3050
            filter.GetValue<long>().Should().Be(value);
#pragma warning restore IL2026, IL3050
        });
    }

    // --- TryParseOperator ---

    [Theory]
    [InlineData("eq", FilterOperator.Eq)]
    [InlineData("EQ", FilterOperator.Eq)]
    [InlineData("neq", FilterOperator.Neq)]
    [InlineData("contains", FilterOperator.Contains)]
    [InlineData("startswith", FilterOperator.StartsWith)]
    [InlineData("endswith", FilterOperator.EndsWith)]
    [InlineData("gt", FilterOperator.Gt)]
    [InlineData("lt", FilterOperator.Lt)]
    [InlineData("isnull", FilterOperator.IsNull)]
    [InlineData("isnotnull", FilterOperator.IsNotNull)]
    public void TryParseOperator_KnownOperators_ParsesCorrectly(string input, FilterOperator expected)
    {
        Filter.TryParseOperator(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("unknown_op")]
    public void TryParseOperator_UnknownOrEmpty_ReturnsNull(string? input)
    {
        Filter.TryParseOperator(input).Should().BeNull();
    }

    [Fact]
    public void OperatorEnum_Property_MatchesTryParseOperator()
    {
        var filter = new Filter { Field = "f", Operator = "eq", Value = null };

        filter.OperatorEnum.Should().Be(FilterOperator.Eq);
    }
}
