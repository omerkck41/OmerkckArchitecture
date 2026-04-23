namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Reads metadata from local files.
/// </summary>
public interface IFileMetadataReader
{
    FileMetadata GetMetadata(string filePath);
}
