namespace Kck.Security.Abstractions.Encryption;

/// <summary>
/// Symmetric encryption for data at rest.
/// </summary>
public interface IEncryptionService
{
    /// <summary>Encrypts plaintext and returns a base64-encoded ciphertext.</summary>
    Task<string> EncryptAsync(string plainText, CancellationToken ct = default);

    /// <summary>Decrypts a base64-encoded ciphertext.</summary>
    Task<string> DecryptAsync(string cipherText, CancellationToken ct = default);
}
