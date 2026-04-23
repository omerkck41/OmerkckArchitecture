using System.Text.RegularExpressions;

namespace Kck.AspNetCore.Sanitization;

internal sealed partial class InputSanitizer : IInputSanitizer
{
    public string Sanitize(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return DangerousCharsRegex().Replace(input, string.Empty).Trim();
    }

    public bool IsAlphanumeric(string input)
    {
        return AlphanumericRegex().IsMatch(input);
    }

    [GeneratedRegex(@"[<>""'&;()]")]
    private static partial Regex DangerousCharsRegex();

    [GeneratedRegex("^[a-zA-Z0-9]*$")]
    private static partial Regex AlphanumericRegex();
}
