namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Holds descriptive metadata about a file stored in a remote or local file storage provider.
/// </summary>
public sealed class FileMetadata
{
    /// <summary>Gets the file name without its directory path.</summary>
    public required string FileName { get; init; }

    /// <summary>Gets the full path of the file within the storage provider.</summary>
    public required string FullPath { get; init; }

    /// <summary>Gets the size of the file in bytes.</summary>
    public long SizeInBytes { get; init; }

    /// <summary>Gets the UTC timestamp when the file was created, or <see langword="null"/> if unavailable.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Gets the UTC timestamp of the last modification, or <see langword="null"/> if unavailable.</summary>
    public DateTimeOffset? ModifiedAt { get; init; }
}
