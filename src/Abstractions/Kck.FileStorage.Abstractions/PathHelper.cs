using System.Collections.Frozen;

namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Pure-function path utilities. Static because these are side-effect-free.
/// </summary>
public static class PathHelper
{
    private static readonly FrozenSet<string> DangerousSegments =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "..", "~" }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Joins path segments using forward slashes, normalizing any backslashes.
    /// </summary>
    public static string Combine(params string[] segments) =>
        Path.Join(segments).Replace('\\', '/');

    /// <summary>
    /// Returns true if the path is non-empty, contains no dangerous traversal segments, and has no invalid characters.
    /// </summary>
    public static bool IsValidPath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !path.Split('/', '\\').Any(s => DangerousSegments.Contains(s))
        && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;

    /// <summary>
    /// Removes path traversal segments (e.g., "..", "~") and returns a sanitized forward-slash path.
    /// </summary>
    public static string Sanitize(string path) =>
        string.Join("/", path.Split('/', '\\').Where(s => !DangerousSegments.Contains(s)));

    /// <summary>
    /// Returns the file extension without the leading dot, or an empty string if none exists.
    /// </summary>
    public static string GetExtension(string path) =>
        Path.GetExtension(path).TrimStart('.');
}
