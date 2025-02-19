using System.Security.Cryptography;
using System.Text;

namespace Core.Security.MFA;

public class TotpService : ITotpService
{
    private readonly int _timeStep; // Zaman adımı (default: 30 saniye)

    public TotpService(int timeStep = 30)
    {
        _timeStep = timeStep;
    }

    // TOTP kodu üretir.
    public Task<string> GenerateTotpCodeAsync(string secretKey, DateTimeOffset? timestamp = null)
    {
        var time = (timestamp ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds() / _timeStep;
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var timeBytes = BitConverter.GetBytes(time);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(timeBytes);
        var offset = hash[^1] & 0x0F;

        var binaryCode = (hash[offset] & 0x7F) << 24 |
                         (hash[offset + 1] & 0xFF) << 16 |
                         (hash[offset + 2] & 0xFF) << 8 |
                         (hash[offset + 3] & 0xFF);

        var totp = binaryCode % 1_000_000;
        return Task.FromResult(totp.ToString("D6"));
    }

    // TOTP kodunu doğrular (önceki ve sonraki zaman dilimlerini de kontrol eder).
    public async Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey)
    {
        var timeOffsets = new[] { -_timeStep * 2, -_timeStep, 0, _timeStep, _timeStep * 2 };
        foreach (var offset in timeOffsets)
        {
            var generatedCode = await GenerateTotpCodeAsync(secretKey, DateTimeOffset.UtcNow.AddSeconds(offset));
            if (inputCode == generatedCode)
                return true;
        }
        return false;
    }
}