namespace Kck.Security.Abstractions.Hashing;

/// <summary>
/// Password hashing using a modern algorithm (e.g. Argon2id, bcrypt).
/// </summary>
public interface IHashingService
{
    /// <summary>Hashes a password and returns the encoded hash string.</summary>
    Task<string> HashAsync(string password, CancellationToken ct = default);

    /// <summary>Verifies a password against a stored hash. Uses timing-safe comparison.</summary>
    Task<bool> VerifyAsync(string password, string hash, CancellationToken ct = default);
}
