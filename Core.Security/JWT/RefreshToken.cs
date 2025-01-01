using Core.Persistence.Entities;

namespace Core.Security.JWT;

public class RefreshToken<TId, TUserId> : Entity<TId>
{
    public string? Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedByIp { get; set; }
    public TUserId UserId { get; set; }

    public RefreshToken()
    {
        UserId = default!;
        Token = string.Empty;
        CreatedByIp = string.Empty;
    }

    public RefreshToken(TUserId userId, string token, DateTime expirationDate, string createdByIp)
    {
        UserId = userId;
        Token = token;
        Expires = expirationDate;
        CreatedByIp = createdByIp;
    }

    public RefreshToken(TId id, TUserId userId, string token, DateTime expirationDate, string createdByIp)
        : base(id)
    {
        UserId = userId;
        Token = token;
        Expires = expirationDate;
        CreatedByIp = createdByIp;
    }
}
