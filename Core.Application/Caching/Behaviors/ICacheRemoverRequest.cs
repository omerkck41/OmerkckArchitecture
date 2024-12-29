namespace Core.Application.Caching.Behaviors;

public interface ICacheRemoverRequest
{
    /// <summary>
    /// Cache anahtarını belirtir.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Cache'in kaldırılıp kaldırılmayacağını belirtir.
    /// </summary>
    bool RemoveCache { get; }
}