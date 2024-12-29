using System.Text.RegularExpressions;

namespace Core.ToolKit.Security;

public static class InputSanitizer
{
    /// <summary>
    /// Removes potentially harmful characters from user input.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>Sanitized string.</returns>
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        var sanitizedInput = Regex.Replace(input, @"[<>\""']", string.Empty);

        return sanitizedInput.Trim();
    }

    /// <summary>
    /// Validates if the input contains only alphanumeric characters.
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public static bool IsAlphanumeric(string input)
    {
        return Regex.IsMatch(input, "^[a-zA-Z0-9]*$");
    }
}