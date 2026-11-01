using AwesomeAssertions;
using ClosedXML.Excel;
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

    [Fact]
    public async Task ReadAsync_EmptyCellInsideRow_ReadsNullAtItsPosition()
    {
        using var stream = Workbook(sheet =>
        {
            sheet.Cell(1, 1).Value = "A";
            sheet.Cell(1, 3).Value = "C";
        });

        var rows = await _sut.ReadAsync(stream);

        rows.Should().ContainSingle();
        rows[0].Should().Equal(new object?[] { "A", null, "C" });
    }

    [Fact]
    public async Task ReadAsync_RowShorterThanWidestRow_IsPaddedWithNulls()
    {
        using var stream = Workbook(sheet =>
        {
            sheet.Cell(1, 1).Value = "Name";
            sheet.Cell(1, 2).Value = "Age";
            sheet.Cell(1, 3).Value = "City";
            sheet.Cell(2, 1).Value = "Alice";
            sheet.Cell(2, 2).Value = 30;
        });

        var rows = await _sut.ReadAsync(stream);

        rows.Should().HaveCount(2);
        rows[0].Should().Equal("Name", "Age", "City");
        rows[1].Should().Equal(new object?[] { "Alice", "30", null });
    }

    [Fact]
    public async Task ReadAsync_RowsTimesWidthAboveLimit_ThrowsBeforeAllocating()
    {
        // 700 rows with one value each in the last Excel column: 700 × 16384 = 11,468,800 cells > MaxReadCells.
        using var stream = Workbook(sheet =>
        {
            for (var row = 1; row <= 700; row++)
                sheet.Cell(row, XLHelper.MaxColumnNumber).Value = row;
        });

        var act = () => _sut.ReadAsync(stream);

        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*700 used rows*16384 columns*");
    }

    [Fact]
    public async Task ReadAsync_RowsTimesWidthWithinLimit_Reads()
    {
        // 600 × 16384 = 9,830,400 cells < MaxReadCells: the far column is padded, nothing is rejected.
        using var stream = Workbook(sheet =>
        {
            for (var row = 1; row <= 600; row++)
                sheet.Cell(row, XLHelper.MaxColumnNumber).Value = row;
        });

        var rows = await _sut.ReadAsync(stream);

        rows.Should().HaveCount(600);
        rows[0].Should().HaveCount(XLHelper.MaxColumnNumber);
        rows[0][XLHelper.MaxColumnNumber - 1].Should().Be("1");
    }

    private static MemoryStream Workbook(Action<IXLWorksheet> fill)
    {
        using var workbook = new XLWorkbook();
        fill(workbook.AddWorksheet("Sheet1"));
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
