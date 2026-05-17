namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Reads metadata from local files.
/// </summary>
public interface IFileMetadataReader
{
    /// <summary>
    /// Reads and returns the metadata for the file at the specified local path.
    /// </summary>
    FileMetadata GetMetadata(string filePath);
}
