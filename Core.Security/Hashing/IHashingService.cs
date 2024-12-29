namespace Core.Security.Hashing;

public interface IHashingService
{
    Task<string> ComputeHashAsync(string input);
    Task<bool> VerifyHashAsync(string input, string hash);
}