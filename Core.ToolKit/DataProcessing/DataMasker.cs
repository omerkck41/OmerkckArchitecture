using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.ToolKit.DataProcessing;

public static class DataMasker
{
    /// <summary>
    /// Masks sensitive data by showing only the first and last visible characters.
    /// </summary>
    /// <param name="input">The sensitive data to mask.</param>
    /// <param name="visibleStart">Number of visible characters at the start.</param>
    /// <param name="visibleEnd">Number of visible characters at the end.</param>
    /// <returns>Masked string.</returns>
    public static string MaskSensitiveData(string input, int visibleStart = 2, int visibleEnd = 2)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        if (input.Length <= visibleStart + visibleEnd)
            return input;

        var maskedMiddle = new string('*', input.Length - visibleStart - visibleEnd);
        return input.Substring(0, visibleStart) + maskedMiddle + input.Substring(input.Length - visibleEnd);
    }

    /// <summary>
    /// Masks an email address while keeping the domain visible.
    /// </summary>
    /// <param name="email">The email address to mask.</param>
    /// <returns>Masked email address.</returns>
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        var parts = email.Split('@');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw new FormatException("Invalid email format.");

        return MaskSensitiveData(parts[0]) + "@" + parts[1];
    }

    /// <summary>
    /// Masks all but the last four digits of a phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number to mask.</param>
    /// <returns>Masked phone number.</returns>
    public static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));

        return Regex.Replace(phoneNumber, @"\d(?=\d{4})", "*");
    }

    public static string MaskCreditCard(string creditCardNumber)
    {
        if (string.IsNullOrWhiteSpace(creditCardNumber))
            throw new ArgumentException("Credit card number cannot be null or empty.", nameof(creditCardNumber));

        if (creditCardNumber.Length <= 4)
            return creditCardNumber;

        return new string('*', creditCardNumber.Length - 4) + creditCardNumber.Substring(creditCardNumber.Length - 4);
    }

    /// <summary>
    /// Encrypts sensitive data using AES encryption.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="key">The encryption key.</param>
    /// <returns>Encrypted data in Base64 format.</returns>
    public static string Encrypt(string plainText, string key)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentException("Plain text cannot be null or empty.", nameof(plainText));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        using var aes = Aes.Create();
        var keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
        aes.Key = keyBytes;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    /// <summary>
    /// Decrypts sensitive data encrypted using AES.
    /// </summary>
    /// <param name="encryptedText">The encrypted data in Base64 format.</param>
    /// <param name="key">The decryption key.</param>
    /// <returns>Decrypted plain text.</returns>
    public static string Decrypt(string encryptedText, string key)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
            throw new ArgumentException("Encrypted text cannot be null or empty.", nameof(encryptedText));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var encryptedBytes = Convert.FromBase64String(encryptedText);
        using var aes = Aes.Create();

        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
        aes.IV = encryptedBytes[..16];

        using var decryptor = aes.CreateDecryptor();
        using var memoryStream = new MemoryStream(encryptedBytes[16..]);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

        using var reader = new StreamReader(cryptoStream);
        return reader.ReadToEnd();
    }
}