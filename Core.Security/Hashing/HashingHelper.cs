using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing;

public static class HashingHelper
{
    /// <summary>
    /// Creates a password hash and salt using HMACSHA512 asynchronously.
    /// </summary>
    public static async Task<(byte[] PasswordHash, byte[] PasswordSalt)> CreatePasswordHashAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty or null.", nameof(password));

        return await Task.Run(() =>
        {
            using var hmac = new HMACSHA512();
            var passwordSalt = hmac.Key;
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return (passwordHash, passwordSalt);
        });
    }

    /// <summary>
    /// Verifies the given password against the hash and salt asynchronously.
    /// </summary>
    public static async Task<bool> VerifyPasswordHashAsync(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty or null.", nameof(password));
        if (passwordHash == null || passwordHash.Length == 0)
            throw new ArgumentException("Invalid password hash.", nameof(passwordHash));
        if (passwordSalt == null || passwordSalt.Length == 0)
            throw new ArgumentException("Invalid password salt.", nameof(passwordSalt));

        return await Task.Run(() =>
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        });
    }

    /// <summary>
    /// Computes a SHA256 hash for the given input asynchronously.
    /// </summary>
    public static async Task<string> ComputeHashAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty or null.", nameof(input));

        return await Task.Run(() =>
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        });
    }

    /// <summary>
    /// Verifies a SHA256 hash against the input asynchronously.
    /// </summary>
    public static async Task<bool> VerifyHashAsync(string input, string hash)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty or null.", nameof(input));
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty or null.", nameof(hash));

        var computedHash = await ComputeHashAsync(input);
        return computedHash == hash;
    }
}