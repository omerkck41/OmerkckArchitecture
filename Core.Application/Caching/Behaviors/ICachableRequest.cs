namespace Core.Application.Caching.Behaviors;

public interface ICachableRequest
{
    /// <summary>
    /// Cache kullanılıp kullanılmayacağını belirtir.
    /// </summary>
    bool UseCache => true;

    /// <summary>
    /// Cache anahtarı.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Cache'in süresini belirtir.
    /// </summary>
    TimeSpan? CacheExpiration => null;
}