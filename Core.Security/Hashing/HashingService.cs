using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing;

public class HashingService : IHashingService
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
}