using FluentAssertions;
using Kck.Documents.Abstractions;
using Kck.Documents.ClosedXml;
using Xunit;

namespace Kck.Documents.ClosedXml.Tests;

public sealed class ClosedXmlExcelServiceTests
{
    private readonly ClosedXmlExcelService _sut = new();

    [Fact]
    public async Task CreateAsync_WithHeaders_ProducesValidExcel()
    {
        var worksheets = new[]
        {
            new ExcelWorksheet
            {
                Name = "Test",
                Headers = ["Name", "Age"],
                Rows = [new object?[] { "Alice", 30 }, new object?[] { "Bob", 25 }]
            }
        };

        var result = await _sut.CreateAsync(worksheets);

        result.Content.Should().NotBeEmpty();
        result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        result.FileName.Should().Be("export.xlsx");
    }

    [Fact]
    public async Task CreateFromDataAsync_ProducesValidExcel()
    {
        var data = new[] { new { Name = "Alice", Age = 30 }, new { Name = "Bob", Age = 25 } };

        var result = await _sut.CreateFromDataAsync(data, "People");

        result.Content.Should().NotBeEmpty();
        result.FileName.Should().Be("People.xlsx");
    }

    [Fact]
    public async Task CreateAndRead_Roundtrip()
    {
        var worksheets = new[]
        {
            new ExcelWorksheet
            {
                Name = "Test",
                Headers = ["Col1"],
                Rows = [new object?[] { "Value1" }]
            }
        };

        var created = await _sut.CreateAsync(worksheets);
        using var stream = new MemoryStream(created.Content);
        var rows = await _sut.ReadAsync(stream);

        rows.Should().HaveCount(2); // header + data row
    }
}
