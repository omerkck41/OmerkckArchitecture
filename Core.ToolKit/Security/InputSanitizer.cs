using System.Text.RegularExpressions;

namespace Core.ToolKit.Security;

public static class InputSanitizer
{
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        var sanitizedInput = Regex.Replace(input, @"[<>\""'&;()]", string.Empty);
        return sanitizedInput.Trim();
    }

    public static bool IsAlphanumeric(string input)
    {
        return Regex.IsMatch(input, "^[a-zA-Z0-9]*$");
    }
}