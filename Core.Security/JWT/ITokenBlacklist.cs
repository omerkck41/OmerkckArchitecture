namespace Core.Security.JWT;

public interface ITokenBlacklist
{
    /// <summary>
    /// Bir token'ı kara listeye ekler.
    /// </summary>
    /// <param name="token">Kara listeye eklenecek token.</param>
    Task RevokeTokenAsync(string token, string reason);

    /// <summary>
    /// Bir token'ın kara listede olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    Task<bool> IsTokenRevokedAsync(string token);
}