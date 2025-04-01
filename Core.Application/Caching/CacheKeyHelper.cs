using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Application.Caching;

public static class CacheKeyHelper
{
    public static string GenerateKey(params string[] parts)
    {
        if (parts == null || parts.Length == 0)
            throw new CustomArgumentException("Key parts cannot be null or empty.", nameof(parts));

        return string.Join(":", parts.Where(p => !string.IsNullOrEmpty(p)));
    }
}