using Core.Persistence.Entities;

namespace Core.Security.Entities;

public class OtpAuthenticator<TUserId> : Entity<TUserId>
{
    public TUserId UserId { get; set; }
    public string SecretKey { get; set; }
    public bool IsVerified { get; set; }

    public OtpAuthenticator()
    {
        UserId = default!;
        SecretKey = string.Empty;
    }

    public OtpAuthenticator(TUserId userId, string secretKey, bool isVerified)
    {
        UserId = userId;
        SecretKey = secretKey;
        IsVerified = isVerified;
    }

    public OtpAuthenticator(TUserId id, TUserId userId, string secretKey, bool isVerified) : base(id)
    {
        UserId = userId;
        SecretKey = secretKey;
        IsVerified = isVerified;
    }
}