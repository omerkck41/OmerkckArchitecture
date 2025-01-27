namespace Core.ToolKit.FileManagement;

public static class PathHelper
{
    /// <summary>
    /// Combines multiple path segments into a single path.
    /// </summary>
    /// <param name="paths">Path segments to combine.</param>
    /// <returns>Combined path.</returns>
    public static string Combine(params string[] paths)
    {
        return Path.Combine(paths);
    }

    /// <summary>
    /// Validates if a given path is a valid file path.
    /// </summary>
    /// <param name="path">Path to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public static bool IsValidPath(string path)
    {
        if (!IsValidPath(path))
            return false;

        try
        {
            var fileInfo = new FileInfo(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string SanitizePath(string path)
    {
        var invalidChars = Path.GetInvalidPathChars();
        return new string(path.Where(c => !invalidChars.Contains(c)).ToArray());
    }

    /// <summary>
    /// Checks if a file has a specific extension.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <param name="extension">Expected extension (e.g., ".txt").</param>
    /// <returns>True if the file has the expected extension.</returns>
    public static bool HasExtension(string filePath, string extension)
    {
        return Path.GetExtension(filePath)?.Equals(extension, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Retrieves all file paths in a directory with an optional search pattern and recursive option.
    /// </summary>
    /// <param name="directoryPath">The directory to search in.</param>
    /// <param name="searchPattern">Optional file search pattern (e.g., "*.txt").</param>
    /// <param name="searchOption">SearchOption to include subdirectories or not.</param>
    /// <returns>List of file paths matching the criteria.</returns>
    public static IEnumerable<string> GetFilePaths(string directoryPath, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found at path: {directoryPath}");

        return Directory.GetFiles(directoryPath, searchPattern, searchOption);
    }

    /// <summary>
    /// Retrieves all directory paths in a directory with an optional recursive option.
    /// </summary>
    /// <param name="directoryPath">The directory to search in.</param>
    /// <param name="searchOption">SearchOption to include subdirectories or not.</param>
    /// <returns>List of directory paths.</returns>
    public static IEnumerable<string> GetDirectoryPaths(string directoryPath, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found at path: {directoryPath}");

        return Directory.GetDirectories(directoryPath, "*", searchOption);
    }
}