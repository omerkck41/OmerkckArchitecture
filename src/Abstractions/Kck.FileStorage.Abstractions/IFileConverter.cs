namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Converts between file representations.
/// </summary>
public interface IFileConverter
{
    Task<string> ToBase64Async(Stream input, CancellationToken ct = default);
    Task<Stream> FromBase64Async(string base64, CancellationToken ct = default);
    Task<byte[]> ToBytesAsync(Stream input, CancellationToken ct = default);
}
