namespace Kck.Documents.Abstractions;

public sealed class ExcelWorksheet
{
    public required string Name { get; init; }
    public IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; } = [];
    public IReadOnlyList<string>? Headers { get; init; }
}
