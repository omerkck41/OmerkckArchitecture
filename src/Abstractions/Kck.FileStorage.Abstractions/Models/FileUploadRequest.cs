namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Encapsulates all parameters required to upload a file to a storage provider.
/// </summary>
public sealed class FileUploadRequest
{
    /// <summary>Gets the stream containing the file data to be uploaded.</summary>
    public required Stream Content { get; init; }

    /// <summary>Gets the name of the file as it should be stored at the destination.</summary>
    public required string FileName { get; init; }

    /// <summary>Gets the target directory path within the storage provider where the file will be placed.</summary>
    public required string DestinationPath { get; init; }

    /// <summary>Gets a value indicating whether an existing file at the destination should be overwritten.</summary>
    public bool Overwrite { get; init; }
}
