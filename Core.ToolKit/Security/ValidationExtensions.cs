using System.Text.RegularExpressions;

namespace Core.ToolKit.Security;

public static class ValidationExtensions
{
    /// <summary>
    /// Validates if the input is a valid email address.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    /// <summary>
    /// Validates if the input is a valid phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public static bool IsValidPhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

        const string phonePattern = @"^\+?[1-9]\d{1,14}$";
        return Regex.IsMatch(phoneNumber, phonePattern);
    }

    /// <summary>
    /// Checks if a string is a valid URL.
    /// </summary>
    /// <param name="url">The URL string to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public static bool IsValidUrl(this string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        const string urlPattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";

        return Regex.IsMatch(url, urlPattern);
    }
}