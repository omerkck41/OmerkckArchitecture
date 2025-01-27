namespace Core.Application.Caching;

public class CacheSettings
{
    public CacheProvider Provider { get; set; } = CacheProvider.InMemory; // Default provider
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
}

public enum CacheProvider
{
    InMemory,
    Distributed
}