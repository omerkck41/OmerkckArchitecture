namespace Kck.Documents.Abstractions;

/// <summary>
/// Represents a single worksheet within an Excel document, containing a name, optional headers, and row data.
/// </summary>
public sealed class ExcelWorksheet
{
    /// <summary>Gets the name of the worksheet tab.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rows of cell data, where each row is an ordered list of cell values.</summary>
    public IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; } = [];

    /// <summary>Gets the optional column header labels for the first row.</summary>
    public IReadOnlyList<string>? Headers { get; init; }
}
