using System.Security.Cryptography;
using System.Text;

namespace Core.Security.MFA;

public class TotpService
{
    private readonly int _timeStep; // Zaman adımı (default: 30 saniye)

    public TotpService(int timeStep = 30)
    {
        _timeStep = timeStep;
    }

    // TOTP kodu üretir.
    public async Task<string> GenerateTotpCodeAsync(string secretKey, DateTimeOffset? timestamp = null)
    {
        try
        {
            var time = (timestamp ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds() / _timeStep;
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var timeBytes = BitConverter.GetBytes(time);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeBytes);

            using var hmac = new HMACSHA1(keyBytes);
            var hash = hmac.ComputeHash(timeBytes);
            var offset = hash[^1] & 0x0F;

            var binaryCode = (hash[offset] & 0x7F) << 24 |
                             (hash[offset + 1] & 0xFF) << 16 |
                             (hash[offset + 2] & 0xFF) << 8 |
                             (hash[offset + 3] & 0xFF);

            var totp = binaryCode % 1_000_000;
            return await Task.FromResult(totp.ToString("D6"));
        }
        catch (Exception ex)
        {
            // Loglama yapılabilir.
            throw new InvalidOperationException("Failed to generate TOTP code.", ex);
        }
    }

    // TOTP kodunu doğrular (önceki ve sonraki zaman dilimlerini de kontrol eder).
    public async Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey)
    {
        try
        {
            var currentCode = await GenerateTotpCodeAsync(secretKey);
            var previousCode = await GenerateTotpCodeAsync(secretKey, DateTimeOffset.UtcNow.AddSeconds(-_timeStep));
            var nextCode = await GenerateTotpCodeAsync(secretKey, DateTimeOffset.UtcNow.AddSeconds(_timeStep));

            return inputCode == currentCode || inputCode == previousCode || inputCode == nextCode;
        }
        catch (Exception ex)
        {
            // Loglama yapılabilir.
            throw new InvalidOperationException("Failed to validate TOTP code.", ex);
        }
    }
}