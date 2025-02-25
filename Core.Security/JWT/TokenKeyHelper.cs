namespace Core.Security.JWT;

public static class TokenKeyHelper
{
    private const string KeyPrefix = "user_token_";

    public static string BuildKey<TUserId>(TUserId userId, string token)
    {
        return $"{KeyPrefix}{userId}_{token}";
    }
}