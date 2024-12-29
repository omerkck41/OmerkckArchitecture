namespace Core.Security.Hashing;

public interface IPasswordHasher
{
    Task<(byte[] passwordHash, byte[] passwordSalt)> CreatePasswordHashAsync(string password);
    Task<bool> VerifyPasswordHashAsync(string password, byte[] passwordHash, byte[] passwordSalt);
}