using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing;

public class HashingService : IHashingService
{
    private readonly ILogger<HashingService> _logger;

    public HashingService(ILogger<HashingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ComputeHashAsync(string input)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = await Task.Run(() => sha256.ComputeHash(bytes));
            var result = Convert.ToBase64String(hash);

            _logger.LogInformation("Hash successfully computed.");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing hash.");
            throw;
        }
    }

    public async Task<bool> VerifyHashAsync(string input, string hash)
    {
        try
        {
            var computedHash = await ComputeHashAsync(input);
            var isValid = computedHash == hash;

            _logger.LogInformation("Hash verification result: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying hash.");
            throw;
        }
    }
}