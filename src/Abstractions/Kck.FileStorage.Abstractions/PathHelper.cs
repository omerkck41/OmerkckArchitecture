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

    public static string Combine(params string[] segments) =>
        Path.Join(segments).Replace('\\', '/');

    public static bool IsValidPath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !path.Split('/', '\\').Any(s => DangerousSegments.Contains(s))
        && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;

    public static string Sanitize(string path) =>
        string.Join("/", path.Split('/', '\\').Where(s => !DangerousSegments.Contains(s)));

    public static string GetExtension(string path) =>
        Path.GetExtension(path).TrimStart('.');
}
