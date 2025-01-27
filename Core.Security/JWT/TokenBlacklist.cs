namespace Core.Security.JWT;

/// <summary>
/// İptal edilen token'lar için bir kara liste temsil eder.
/// </summary>
public class TokenBlacklist : ITokenBlacklist
{
    private readonly HashSet<string> _revokedTokens = new();

    /// <summary>
    /// Bir token'ı kara listeye ekleyerek iptal eder.
    /// </summary>
    /// <param name="token">İptal edilecek token.</param>
    public void RevokeToken(string token)
    {
        _revokedTokens.Add(token);
    }

    /// <summary>
    /// Bir token'ın iptal edilip edilmediğini kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    /// <returns>Token iptal edilmişse true; aksi takdirde false.</returns>
    public bool IsTokenRevoked(string token)
    {
        return _revokedTokens.Contains(token);
    }
}