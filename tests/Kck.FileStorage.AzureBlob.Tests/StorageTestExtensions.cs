using System.Text;
using Kck.FileStorage.Abstractions;

namespace Kck.FileStorage.AzureBlob.Tests;

/// <summary>Text upload/download helpers for the container tests.</summary>
internal static class StorageTestExtensions
{
    public static async Task UploadTextAsync(
        this IFileStorageService storage, string destination, string fileName, string text, bool overwrite = true)
    {
        using var content = new MemoryStream(Encoding.UTF8.GetBytes(text));
        await storage.UploadAsync(new FileUploadRequest
        {
            Content = content,
            FileName = fileName,
            DestinationPath = destination,
            Overwrite = overwrite
        });
    }

    public static async Task<string> DownloadTextAsync(this IFileStorageService storage, string path)
    {
        await using var stream = await storage.DownloadAsync(path);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
