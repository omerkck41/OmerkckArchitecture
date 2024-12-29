namespace Core.Application.Caching;

public static class CacheKeyHelper
{
    public static string GenerateKey(params string[] parts)
    {
        return string.Join(":", parts);
    }
}