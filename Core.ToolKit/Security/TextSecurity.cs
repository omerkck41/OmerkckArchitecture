using System.Text;

namespace Core.ToolKit.Security;

public static class TextSecurity
{
    public static string SecureEncodeToBase64(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentException("Input cannot be null or empty.", nameof(plainText));

        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }

    public static string SecureDecodeFromBase64(string base64Text)
    {
        if (string.IsNullOrWhiteSpace(base64Text))
            throw new ArgumentException("Input cannot be null or empty.", nameof(base64Text));

        try
        {
            var bytes = Convert.FromBase64String(base64Text);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Input is not a valid Base64 string.", nameof(base64Text));
        }
    }
}