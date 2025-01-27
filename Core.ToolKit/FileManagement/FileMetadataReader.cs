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

    public static string GetFileOwner(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at path: {filePath}");

        var fileInfo = new FileInfo(filePath);
        var fileSecurity = fileInfo.GetAccessControl();
        return fileSecurity.GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
    }
}