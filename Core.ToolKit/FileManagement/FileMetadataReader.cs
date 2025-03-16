using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Core.ToolKit.FileManagement;

public static class FileMetadataReader
{
    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>File size in bytes.</returns>
    public static long GetFileSize(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at path: {filePath}");

        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }

    /// <summary>
    /// Gets the creation date of the file.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>Creation date of the file.</returns>
    public static DateTime GetCreationDate(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at path: {filePath}");

        var fileInfo = new FileInfo(filePath);
        return fileInfo.CreationTime;
    }

    /// <summary>
    /// Gets the last modified date of the file.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>Last modified date of the file.</returns>
    public static DateTime GetLastModifiedDate(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at path: {filePath}");

        var fileInfo = new FileInfo(filePath);
        return fileInfo.LastWriteTime;
    }

    public static FileAttributes GetFileAttributes(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at path: {filePath}");

        return File.GetAttributes(filePath);
    }

    [SupportedOSPlatform("windows")]
    public static string GetFileOwner(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at path: {filePath}");

        // Çalışma zamanında platform kontrolü
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("File owner retrieval is only supported on Windows.");

        var fileInfo = new FileInfo(filePath);
        var fileSecurity = fileInfo.GetAccessControl();
        var owner = fileSecurity.GetOwner(typeof(NTAccount));
        if (owner is null)
            throw new InvalidOperationException("File owner could not be determined.");
        return owner.ToString();
    }
}