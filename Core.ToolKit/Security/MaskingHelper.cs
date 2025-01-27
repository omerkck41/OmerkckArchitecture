using System.Text.RegularExpressions;

namespace Core.ToolKit.Security;

public static class MaskingHelper
{
    public static string Mask(string input, int visibleStart = 2, int visibleEnd = 2)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        if (visibleStart < 0 || visibleEnd < 0)
            throw new ArgumentException("Visible start and end values cannot be negative.");

        if (input.Length <= visibleStart + visibleEnd)
            return input;

        var maskedMiddle = new string('*', input.Length - visibleStart - visibleEnd);
        return input.Substring(0, visibleStart) + maskedMiddle + input.Substring(input.Length - visibleEnd);
    }

    public static string MaskAllDigitsExceptLastFour(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        return Regex.Replace(input, @"\d(?=\d{4})", "*");
    }
}
