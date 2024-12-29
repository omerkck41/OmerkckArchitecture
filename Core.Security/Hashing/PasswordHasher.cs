using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing;

public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;

    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger;
    }

    public async Task<(byte[] passwordHash, byte[] passwordSalt)> CreatePasswordHashAsync(string password)
    {
        try
        {
            using var hmac = new HMACSHA512();
            var passwordSalt = hmac.Key;
            var passwordHash = await Task.Run(() => hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            _logger.LogInformation("Password hash and salt successfully created.");
            return (passwordHash, passwordSalt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating password hash.");
            throw;
        }
    }

    public async Task<bool> VerifyPasswordHashAsync(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        try
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = await Task.Run(() => hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            var isValid = computedHash.SequenceEqual(passwordHash);

            _logger.LogInformation("Password hash verification result: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password hash.");
            throw;
        }
    }
}