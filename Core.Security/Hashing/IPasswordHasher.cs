namespace Core.Security.Hashing;

public interface IPasswordHasher
{
    Task<string> CreatePasswordHashAsync(string password);
    Task<bool> VerifyPasswordHashAsync(string password, string storedHash);
}