namespace Kck.FileStorage.Abstractions;

public sealed class FileUploadRequest
{
    public required Stream Content { get; init; }
    public required string FileName { get; init; }
    public required string DestinationPath { get; init; }
    public bool Overwrite { get; init; }
}
