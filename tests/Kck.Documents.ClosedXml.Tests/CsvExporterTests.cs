using System.Text;
using AwesomeAssertions;
using Kck.Documents.Abstractions;
using Xunit;

namespace Kck.Documents.ClosedXml.Tests;

/// <summary>RFC 4180 output of <see cref="CsvExporter"/>: header from public properties, quoting, nulls, file name.</summary>
public sealed class CsvExporterTests
{
    private readonly CsvExporter _sut = new();

    private sealed record Row(string Name, int Age, string? Note);

    [Fact]
    public async Task ExportAsync_WritesHeaderFromPublicPropertiesThenOneLinePerItem()
    {
        var result = await _sut.ExportAsync(new[] { new Row("Alice", 30, "a"), new Row("Bob", 25, "b") });

        Lines(result).Should().Equal("Name,Age,Note", "Alice,30,a", "Bob,25,b");
    }

    [Fact]
    public async Task ExportAsync_EmptyData_WritesHeaderOnly()
    {
        var result = await _sut.ExportAsync(Array.Empty<Row>());

        Lines(result).Should().Equal("Name,Age,Note");
    }

    [Fact]
    public async Task ExportAsync_QuotesValuesWithCommaQuoteOrNewline()
    {
        var result = await _sut.ExportAsync(new[] { new Row("Doe, John", 1, "says \"hi\"\nbye") });

        Encoding.UTF8.GetString(result.Content).Should().Contain("\"Doe, John\",1,\"says \"\"hi\"\"\nbye\"");
    }

    [Fact]
    public async Task ExportAsync_NullValue_WritesEmptyField()
    {
        var result = await _sut.ExportAsync(new[] { new Row("Alice", 30, null) });

        Lines(result)[1].Should().Be("Alice,30,");
    }

    [Fact]
    public async Task ExportAsync_DefaultsToExportCsvWithTextCsvContentType()
    {
        var result = await _sut.ExportAsync(Array.Empty<Row>());

        result.FileName.Should().Be("export.csv");
        result.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task ExportAsync_UsesTheGivenFileName()
    {
        var result = await _sut.ExportAsync(Array.Empty<Row>(), "people.csv");

        result.FileName.Should().Be("people.csv");
    }

    [Fact]
    public void AddKckDocumentsClosedXml_RegistersCsvExporter()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        Microsoft.Extensions.DependencyInjection.KckDocumentsClosedXmlServiceCollectionExtensions.AddKckDocumentsClosedXml(services);

        using var provider = Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services);
        Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ICsvExporter>(provider)
            .Should().BeOfType<CsvExporter>();
    }

    // The exporter writes lines with the platform newline (StringBuilder.AppendLine); the quoted field keeps its own '\n'.
    private static string[] Lines(DocumentResult result) =>
        Encoding.UTF8.GetString(result.Content).TrimEnd('\r', '\n').Split(Environment.NewLine);
}
