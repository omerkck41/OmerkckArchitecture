namespace Kck.Documents.Abstractions;

/// <summary>
/// Result of a document generation operation.
/// </summary>
public sealed class DocumentResult
{
    /// <summary>
    /// The raw byte content of the generated document.
    /// </summary>
    public required byte[] Content { get; init; }
    /// <summary>
    /// The MIME type of the document (e.g., "text/csv", "application/pdf").
    /// </summary>
    public required string ContentType { get; init; }
    /// <summary>
    /// The suggested file name including extension for the document.
    /// </summary>
    public required string FileName { get; init; }
}
