using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing;

public class PasswordHasher : IPasswordHasher
{
    public async Task<(byte[] passwordHash, byte[] passwordSalt)> CreatePasswordHashAsync(string password)
    {
        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = await Task.Run(() => hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));


        return (passwordHash, passwordSalt);
    }

    public async Task<bool> VerifyPasswordHashAsync(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = await Task.Run(() => hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        var isValid = computedHash.SequenceEqual(passwordHash);


        return isValid;
    }
}