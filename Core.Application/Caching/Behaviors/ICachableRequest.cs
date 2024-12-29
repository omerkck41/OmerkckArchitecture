namespace Core.Application.Caching.Behaviors;

public interface ICachableRequest
{
    /// <summary>
    /// Cache kullanılıp kullanılmayacağını belirtir.
    /// </summary>
    bool UseCache { get; }

    /// <summary>
    /// Cache anahtarı.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Cache'in süresini belirtir.
    /// </summary>
    TimeSpan? CacheExpiration { get; }
}