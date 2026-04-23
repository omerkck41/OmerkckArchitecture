namespace Kck.FileStorage.FluentFtp;

internal static class PathSanitizer
{
    public static void Validate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Reject percent-encoding outright — FTP paths are not URL-encoded.
        // Blocks URL-encoded traversal vectors like %2e%2e, ..%2f, and UTF-8 overlong forms (%c0%ae).
        if (path.Contains('%'))
            throw new ArgumentException($"Percent-encoded characters are not allowed in paths: '{path}'", nameof(path));

        // Reject control characters (including null byte, which truncates paths on some stacks).
        foreach (var c in path)
        {
            if (c < 0x20 || c == 0x7F)
                throw new ArgumentException($"Control characters are not allowed in paths: '{path}'", nameof(path));
        }

        var normalized = path.Replace('\\', '/');

        if (normalized.StartsWith('/'))
            throw new ArgumentException($"Absolute paths are not allowed: '{path}'", nameof(path));

        if (normalized.Contains(".."))
            throw new ArgumentException($"Path traversal is not allowed: '{path}'", nameof(path));
    }
}
