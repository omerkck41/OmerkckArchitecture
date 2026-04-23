namespace Kck.Documents.Abstractions;

/// <summary>
/// Result of a document generation operation.
/// </summary>
public sealed class DocumentResult
{
    public required byte[] Content { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}
