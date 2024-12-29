namespace Core.Application.Caching;

public class CacheSettings
{
    public string Provider { get; set; } = "InMemory"; // Default provider
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
}