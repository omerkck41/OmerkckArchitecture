using System.Text;

namespace Core.ToolKit.Security;

public static class TextSecurity
{
    /// <summary>
    /// Encodes a string to Base64 format.
    /// </summary>
    /// <param name="plainText">The plain text to encode.</param>
    /// <returns>Encoded Base64 string.</returns>
    public static string EncodeToBase64(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentException("Input cannot be null or empty.", nameof(plainText));

        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decodes a Base64 string to plain text.
    /// </summary>
    /// <param name="base64Text">The Base64 encoded string.</param>
    /// <returns>Decoded plain text.</returns>
    public static string DecodeFromBase64(string base64Text)
    {
        if (string.IsNullOrWhiteSpace(base64Text))
            throw new ArgumentException("Input cannot be null or empty.", nameof(base64Text));

        var bytes = Convert.FromBase64String(base64Text);
        return Encoding.UTF8.GetString(bytes);
    }
}