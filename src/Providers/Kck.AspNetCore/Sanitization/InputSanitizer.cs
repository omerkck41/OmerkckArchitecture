using System.Text.RegularExpressions;

namespace Kck.AspNetCore.Sanitization;

/// <summary>
/// Source-generated regex-based implementation of <see cref="IInputSanitizer"/> that strips
/// HTML-injection characters and validates alphanumeric constraints.
/// </summary>
internal sealed partial class InputSanitizer : IInputSanitizer
{
    /// <inheritdoc/>
    public string Sanitize(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return DangerousCharsRegex().Replace(input, string.Empty).Trim();
    }

    /// <inheritdoc/>
    public bool IsAlphanumeric(string input)
    {
        return AlphanumericRegex().IsMatch(input);
    }

    [GeneratedRegex(@"[<>""'&;()]")]
    private static partial Regex DangerousCharsRegex();

    [GeneratedRegex("^[a-zA-Z0-9]*$")]
    private static partial Regex AlphanumericRegex();
}
