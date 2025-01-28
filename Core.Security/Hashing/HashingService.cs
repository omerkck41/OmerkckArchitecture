using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing;

public static class HashingService
{
    public static async Task<(string PasswordHash, string PasswordSalt)> CreatePasswordHashAsync(string password)
    {
        using var hmac = new HMACSHA512();
        var passwordSalt = Convert.ToBase64String(hmac.Key); // Salt'ı base64 formatında döndür
        var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password))); // Hash'i base64 formatında döndür

        return await Task.FromResult((passwordHash, passwordSalt));
    }

    public static async Task<bool> VerifyPasswordHashAsync(string password, string passwordHash, string passwordSalt)
    {
        var saltBytes = Convert.FromBase64String(passwordSalt); // Salt'ı byte dizisine çevir
        var storedHashBytes = Convert.FromBase64String(passwordHash); // Hash'i byte dizisine çevir

        using var hmac = new HMACSHA512(saltBytes);
        var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Yeni hash'i hesapla

        // Hash'lerin eşleşip eşleşmediğini kontrol et
        for (int i = 0; i < computedHashBytes.Length; i++)
        {
            if (computedHashBytes[i] != storedHashBytes[i])
                return await Task.FromResult(false);
        }

        return await Task.FromResult(true);
    }
}