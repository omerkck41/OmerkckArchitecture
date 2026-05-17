namespace Kck.FileStorage.FluentFtp;

/// <summary>
/// Security utility that validates FTP path strings against common path-traversal and
/// injection attack vectors before they are sent to the FTP server.
/// </summary>
internal static class PathSanitizer
{
    /// <summary>
    /// Throws <see cref="ArgumentException"/> when <paramref name="path"/> is null/whitespace,
    /// contains percent-encoded characters, control characters, is absolute, or contains
    /// directory-traversal sequences (<c>..</c>).
    /// </summary>
    public static void Validate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Reject percent-encoding outright — FTP paths are not URL-encoded.
        // Blocks URL-encoded traversal vectors like %2e%2e, ..%2f, and UTF-8 overlong forms (%c0%ae).
        if (path.Contains('%'))
            throw new ArgumentException($"Percent-encoded characters are not allowed in paths: '{path}'", nameof(path));

        // Reject control characters (including null byte, which truncates paths on some stacks).
        if (path.Any(c => c < 0x20 || c == 0x7F))
            throw new ArgumentException($"Control characters are not allowed in paths: '{path}'", nameof(path));

        var normalized = path.Replace('\\', '/');

        if (normalized.StartsWith('/'))
            throw new ArgumentException($"Absolute paths are not allowed: '{path}'", nameof(path));

        if (normalized.Contains(".."))
            throw new ArgumentException($"Path traversal is not allowed: '{path}'", nameof(path));
    }
}
