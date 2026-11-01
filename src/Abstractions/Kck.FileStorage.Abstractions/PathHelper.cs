namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Pure-function path utilities. Static because these are side-effect-free.
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Joins path segments using forward slashes, normalizing any backslashes.
    /// </summary>
    public static string Combine(params string[] segments) =>
        Path.Join(segments).Replace('\\', '/');

    /// <summary>
    /// Throws <see cref="ArgumentException"/> when <paramref name="path"/> is null or white space, contains a percent
    /// sign (URL-encoded traversal such as <c>%2e%2e</c>), a control character, starts with a slash or backslash
    /// (absolute), or contains <c>..</c>. The FTP, Azure Blob and S3 providers run it on every path argument before a
    /// request is built (2026-10-31: a <c>..</c> path reached another Blob container). The path is not changed.
    /// </summary>
    public static void Validate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Storage paths are never URL-encoded: rejecting '%' blocks %2e%2e, ..%2f and overlong forms (%c0%ae).
        if (path.Contains('%'))
            throw new ArgumentException($"Percent-encoded characters are not allowed in paths: '{path}'", nameof(path));

        // Control characters, including the null byte that truncates paths on some stacks.
        if (path.AsSpan().IndexOfAnyInRange('\0', '\u001F') >= 0 || path.Contains('\u007F'))
            throw new ArgumentException($"Control characters are not allowed in paths: '{path}'", nameof(path));

        var normalized = path.Replace('\\', '/');

        if (normalized.StartsWith('/'))
            throw new ArgumentException($"Absolute paths are not allowed: '{path}'", nameof(path));

        if (normalized.Contains(".."))
            throw new ArgumentException($"Path traversal is not allowed: '{path}'", nameof(path));
    }

    /// <summary>
    /// Returns the file extension without the leading dot, or an empty string if none exists.
    /// </summary>
    public static string GetExtension(string path) =>
        Path.GetExtension(path).TrimStart('.');
}
