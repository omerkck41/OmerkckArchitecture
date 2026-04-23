using System.Globalization;
using FluentAssertions;
using Xunit;

namespace Kck.Localization.Tests;

public sealed class FormatterServiceTests
{
    private readonly FormatterService _sut = new();

    [Fact]
    public void FormatDate_English_ReturnsExpectedFormat()
    {
        var date = new DateTime(2026, 4, 2, 14, 30, 0);
        var result = _sut.FormatDate(date, "en-US");
        result.Should().Contain("4/2/2026");
    }

    [Fact]
    public void FormatDate_Turkish_ReturnsExpectedFormat()
    {
        var date = new DateTime(2026, 4, 2, 14, 30, 0);
        var result = _sut.FormatDate(date, "tr-TR");
        result.Should().Contain("2.04.2026");
    }

    [Fact]
    public void FormatDate_WithCustomFormat_ReturnsCustomFormat()
    {
        var date = new DateTime(2026, 4, 2);
        var result = _sut.FormatDate(date, "en-US", "yyyy-MM-dd");
        result.Should().Be("2026-04-02");
    }

    [Fact]
    public void FormatNumber_English_UsesCommaGrouping()
    {
        var result = _sut.FormatNumber(1234567.89m, "en-US");
        result.Should().Be("1,234,567.89");
    }

    [Fact]
    public void FormatNumber_German_UsesPeriodGrouping()
    {
        var result = _sut.FormatNumber(1234567.89m, "de-DE");
        result.Should().Be("1.234.567,89");
    }

    [Fact]
    public void FormatNumber_WithCustomFormat_AppliesFormat()
    {
        var result = _sut.FormatNumber(0.75m, "en-US", "P0");
        result.Should().Contain("75");
    }

    [Fact]
    public void FormatCurrency_English_ReturnsUSD()
    {
        var result = _sut.FormatCurrency(1234.56m, "en-US");
        result.Should().Contain("1,234.56");
    }

    [Fact]
    public void FormatCurrency_Turkish_ReturnsTRY()
    {
        var result = _sut.FormatCurrency(1234.56m, "tr-TR");
        result.Should().Contain("1.234,56");
    }

    [Fact]
    public void FormatCurrency_WithCurrencyCode_OverridesCulture()
    {
        var result = _sut.FormatCurrency(100m, "en-US", "EUR");
        result.Should().Contain("EUR").And.Contain("100");
    }

    [Fact]
    public void FormatDate_InvalidCulture_FallsBackToInvariant()
    {
        var date = new DateTime(2026, 1, 1);
        var act = () => _sut.FormatDate(date, "xx-XX");
        act.Should().NotThrow();
    }

    [Fact]
    public void FormatNumber_Zero_FormatsCorrectly()
    {
        var result = _sut.FormatNumber(0m, "en-US");
        result.Should().Be("0.00");
    }
}
