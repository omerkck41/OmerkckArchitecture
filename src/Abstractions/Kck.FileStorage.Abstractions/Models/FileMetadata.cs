namespace Kck.FileStorage.Abstractions;

public sealed class FileMetadata
{
    public required string FileName { get; init; }
    public required string FullPath { get; init; }
    public long SizeInBytes { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? ModifiedAt { get; init; }
}
