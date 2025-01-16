using System.Security.Cryptography;
using System.Text;

namespace Core.Security.MFA;

public class TotpService
{
    public static async Task<string> GenerateTotpCodeAsync(string secretKey)
    {
        try
        {
            var timeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var timeBytes = BitConverter.GetBytes(timeStep);

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
            throw new InvalidOperationException("Failed to generate TOTP code.", ex);
        }
    }

    public async Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey)
    {
        var generatedCode = await GenerateTotpCodeAsync(secretKey);
        return await Task.FromResult(inputCode == generatedCode);
    }
}