using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing;

public class HashingService : IHashingService, IPasswordHasher
{
    public async Task<string> ComputeHashAsync(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = await Task.Run(() => sha256.ComputeHash(bytes));
        var result = Convert.ToBase64String(hash);


        return result;
    }

    public async Task<bool> VerifyHashAsync(string input, string hash)
    {
        var computedHash = await ComputeHashAsync(input);
        var isValid = computedHash == hash;


        return isValid;
    }

    public async Task<string> CreatePasswordHashAsync(string password)
    {
        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Salt ve hash'i birleştirip base64 formatında döndür
        var combinedHash = Convert.ToBase64String(passwordSalt) + ":" + Convert.ToBase64String(passwordHash);
        return await Task.FromResult(combinedHash);
    }

    public async Task<bool> VerifyPasswordHashAsync(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
            return await Task.FromResult(false);

        var storedSalt = Convert.FromBase64String(parts[0]);
        var storedPasswordHash = Convert.FromBase64String(parts[1]);

        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Hash'lerin eşleşip eşleşmediğini kontrol et
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != storedPasswordHash[i])
                return await Task.FromResult(false);
        }

        return await Task.FromResult(true);
    }
}