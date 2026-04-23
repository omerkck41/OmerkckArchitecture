using System.Security.Cryptography;
using Kck.Security.Abstractions.Hashing;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Kck.Security.Argon2;

/// <summary>
/// Argon2id password hashing with timing-safe verification.
/// Hash format: $argon2id$v=19$m={memory},t={iterations},p={parallelism}${salt}${hash}
/// </summary>
public sealed class Argon2HashingService : IHashingService
{
    private readonly Argon2Options _options;

    public Argon2HashingService(IOptionsMonitor<Argon2Options> options)
    {
        _options = options.CurrentValue;
    }

    public async Task<string> HashAsync(string password, CancellationToken ct = default)
    {
        var salt = RandomNumberGenerator.GetBytes(_options.SaltLength);

        using var argon2 = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = _options.DegreeOfParallelism,
            MemorySize = _options.MemorySize,
            Iterations = _options.Iterations
        };

        var hash = await argon2.GetBytesAsync(_options.HashLength);

        var saltB64 = Convert.ToBase64String(salt);
        var hashB64 = Convert.ToBase64String(hash);

        return $"$argon2id$v=19$m={_options.MemorySize},t={_options.Iterations},p={_options.DegreeOfParallelism}${saltB64}${hashB64}";
    }

    public async Task<bool> VerifyAsync(string password, string hash, CancellationToken ct = default)
    {
        var parts = ParseHash(hash);
        if (parts is null) return false;

        using var argon2 = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = parts.Value.Salt,
            DegreeOfParallelism = parts.Value.Parallelism,
            MemorySize = parts.Value.Memory,
            Iterations = parts.Value.Iterations
        };

        var computedHash = await argon2.GetBytesAsync(parts.Value.Hash.Length);

        return CryptographicOperations.FixedTimeEquals(computedHash, parts.Value.Hash);
    }

    private static (byte[] Salt, byte[] Hash, int Memory, int Iterations, int Parallelism)? ParseHash(string encoded)
    {
        // Format: $argon2id$v=19$m={m},t={t},p={p}${salt}${hash}
        var segments = encoded.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 5 || segments[0] != "argon2id") return null;

        var paramParts = segments[2].Split(',');
        if (paramParts.Length != 3) return null;

        if (!int.TryParse(paramParts[0].AsSpan(2), out var memory)) return null;
        if (!int.TryParse(paramParts[1].AsSpan(2), out var iterations)) return null;
        if (!int.TryParse(paramParts[2].AsSpan(2), out var parallelism)) return null;

        var salt = Convert.FromBase64String(segments[3]);
        var hash = Convert.FromBase64String(segments[4]);

        return (salt, hash, memory, iterations, parallelism);
    }
}
