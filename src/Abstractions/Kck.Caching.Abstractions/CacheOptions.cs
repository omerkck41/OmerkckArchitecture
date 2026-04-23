namespace Kck.Caching.Abstractions;

public sealed class CacheOptions
{
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);
    public string? KeyPrefix { get; set; }
}
