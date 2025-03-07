using Core.Application.Caching;
using Core.Application.Caching.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Core.Test.Caching;

// Entegrasyon testleri: DI container üzerinden cache servislerinin doğru çalışıp çalışmadığını test ediyoruz.
public class CachingIntegrationTests
{
    [Fact]
    public async Task InMemoryCacheService_Integration_Test()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddCachingServices(configuration, settings =>
        {
            settings.Provider = CacheProvider.InMemory;
            settings.DefaultExpiration = TimeSpan.FromMinutes(10);
        });
        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();
        Assert.NotNull(cacheService);

        string key = "testIntegrationKey";
        string value = "testIntegrationValue";
        await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var retrieved = await cacheService.GetAsync<string>(key);
        Assert.Equal(value, retrieved);
    }

    [Fact]
    public async Task DistributedCacheService_Integration_Test()
    {
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:Redis", "localhost"},
                {"ConnectionStrings:InstanceName", "TestInstance"}
            };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        services.AddCachingServices(configuration, settings =>
        {
            settings.Provider = CacheProvider.Distributed;
            settings.DefaultExpiration = TimeSpan.FromMinutes(10);
        });
        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();
        Assert.NotNull(cacheService);

        string key = "distributedTestKey";
        string value = "distributedTestValue";
        await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var retrieved = await cacheService.GetAsync<string>(key);
        Assert.Equal(value, retrieved);
    }
}

// Hata yönetimi testleri: Cache işlemleri sırasında oluşabilecek hata durumlarını simüle ediyoruz.
// Burada FakeDistributedCache ile IDistributedCache üzerinde hatalı senaryolar üretiyoruz.
public class FakeDistributedCache : IDistributedCache
{
    public byte[] Get(string key) => throw new Exception("Get error");
    public Task<byte[]> GetAsync(string key, CancellationToken token = default) => throw new Exception("GetAsync error");
    public void Refresh(string key) => throw new Exception("Refresh error");
    public Task RefreshAsync(string key, CancellationToken token = default) => throw new Exception("RefreshAsync error");
    public void Remove(string key) => throw new Exception("Remove error");
    public Task RemoveAsync(string key, CancellationToken token = default) => throw new Exception("RemoveAsync error");
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw new Exception("Set error");
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw new Exception("SetAsync error");
}

public class ErrorHandlingTests
{
    [Fact]
    public async Task DistributedCacheService_GetAsync_Error_Test()
    {
        var fakeCache = new FakeDistributedCache();
        var cacheService = new DistributedCacheService(fakeCache);
        await Assert.ThrowsAsync<Exception>(async () => await cacheService.GetAsync<string>("errorKey"));
    }

    [Fact]
    public async Task DistributedCacheService_SetAsync_Error_Test()
    {
        var fakeCache = new FakeDistributedCache();
        var cacheService = new DistributedCacheService(fakeCache);
        await Assert.ThrowsAsync<Exception>(async () => await cacheService.SetAsync("errorKey", "value", TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public async Task DistributedCacheService_RemoveAsync_Error_Test()
    {
        var fakeCache = new FakeDistributedCache();
        var cacheService = new DistributedCacheService(fakeCache);
        await Assert.ThrowsAsync<Exception>(async () => await cacheService.RemoveAsync("errorKey"));
    }
}

// Performans testleri: Belirli bir yük altında cache işlemlerinin istenen performansta çalıştığını test ediyoruz.
public class PerformanceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly InMemoryCacheService _cacheService;

    public PerformanceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new InMemoryCacheService(_memoryCache);
    }

    [Fact]
    public async Task InMemoryCache_Performance_Test()
    {
        string keyPrefix = "perfKey";
        string value = "perfValue";
        TimeSpan expiration = TimeSpan.FromMinutes(1);
        int iterations = 10000;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            string key = keyPrefix + i;
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);
            Assert.Equal(value, result);
        }
        sw.Stop();
        // Örneğin 10.000 işlem için 2 saniyeden kısa sürede tamamlanması bekleniyor.
        Assert.True(sw.ElapsedMilliseconds < 2000, $"Performans testi çok uzun sürdü: {sw.ElapsedMilliseconds}ms");
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}