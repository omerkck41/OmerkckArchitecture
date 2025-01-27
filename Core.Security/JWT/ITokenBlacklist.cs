namespace Core.Security.JWT;

public interface ITokenBlacklist
{
    /// <summary>
    /// Bir token'ı kara listeye ekler.
    /// </summary>
    /// <param name="token">Kara listeye eklenecek token.</param>
    void RevokeToken(string token);

    /// <summary>
    /// Bir token'ın kara listede olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    bool IsTokenRevoked(string token);
}