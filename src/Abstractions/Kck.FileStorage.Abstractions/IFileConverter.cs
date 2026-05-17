namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Converts between file representations.
/// </summary>
public interface IFileConverter
{
    /// <summary>
    /// Reads the stream and returns its contents encoded as a Base64 string.
    /// </summary>
    Task<string> ToBase64Async(Stream input, CancellationToken ct = default);
    /// <summary>
    /// Decodes a Base64 string and returns the resulting data as a readable stream.
    /// </summary>
    Task<Stream> FromBase64Async(string base64, CancellationToken ct = default);
    /// <summary>
    /// Reads the stream and returns its contents as a byte array.
    /// </summary>
    Task<byte[]> ToBytesAsync(Stream input, CancellationToken ct = default);
}
