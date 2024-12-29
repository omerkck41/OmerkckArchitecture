using System.Text.RegularExpressions;

namespace Core.ToolKit.Security;

public static class MaskingHelper
{
    /// <summary>
    /// Masks the middle part of a sensitive string such as an email or phone number.
    /// </summary>
    /// <param name="input">The sensitive string to mask.</param>
    /// <param name="visibleStart">Number of characters to show at the start.</param>
    /// <param name="visibleEnd">Number of characters to show at the end.</param>
    /// <returns>Masked string.</returns>
    public static string Mask(string input, int visibleStart = 2, int visibleEnd = 2)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        if (input.Length <= visibleStart + visibleEnd)
            return input;

        var maskedMiddle = new string('*', input.Length - visibleStart - visibleEnd);
        return input.Substring(0, visibleStart) + maskedMiddle + input.Substring(input.Length - visibleEnd);
    }

    /// <summary>
    /// Masks all digits in a string except for the last four.
    /// </summary>
    /// <param name="input">The input string containing digits.</param>
    /// <returns>Masked string.</returns>
    public static string MaskDigits(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        return Regex.Replace(input, @"\d(?=\d{4})", "*");
    }
}
