using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing;

public static class HashingHelper
{
    /// <summary>
    /// Creates a password hash and salt using HMACSHA512.
    /// </summary>
    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty or null.", nameof(password));

        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    /// <summary>
    /// Verifies the given password against the hash and salt.
    /// </summary>
    public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty or null.", nameof(password));
        if (passwordHash == null || passwordHash.Length == 0)
            throw new ArgumentException("Invalid password hash.", nameof(passwordHash));
        if (passwordSalt == null || passwordSalt.Length == 0)
            throw new ArgumentException("Invalid password salt.", nameof(passwordSalt));

        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
    }

    /// <summary>
    /// Computes a SHA256 hash for the given input.
    /// </summary>
    public static string ComputeHash(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty or null.", nameof(input));

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verifies a SHA256 hash against the input.
    /// </summary>
    public static bool VerifyHash(string input, string hash)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty or null.", nameof(input));
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty or null.", nameof(hash));

        var computedHash = ComputeHash(input);
        return computedHash == hash;
    }
}