using Core.Application.Caching;
using Core.Application.Caching.Behaviors;
using Core.Application.Caching.Services;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Core.Test.Caching;

// Basit bir response sınıfı
public class DummyResponse
{
    public string Value { get; set; }
}

// ICachableRequest implementasyonu
public class DummyCachableRequest : ICachableRequest, IRequest<DummyResponse>
{
    public bool UseCache { get; set; }
    public string CacheKey { get; set; }
    public TimeSpan? CacheExpiration { get; set; }
}

// ICacheRemoverRequest implementasyonu
public class DummyCacheRemoverRequest : ICacheRemoverRequest, IRequest<DummyResponse>
{
    public string CacheKey { get; set; }
    public bool RemoveCache { get; set; }
}

public class CachingBehaviorTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly CacheSettings _cacheSettings;
    private readonly CachingBehavior<DummyCachableRequest, DummyResponse> _behavior;

    public CachingBehaviorTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _cacheSettings = new CacheSettings { DefaultExpiration = TimeSpan.FromMinutes(10) };
        _behavior = new CachingBehavior<DummyCachableRequest, DummyResponse>(_cacheServiceMock.Object, _cacheSettings);
    }

    [Fact]
    public async Task Handle_WhenUseCacheFalse_ShouldNotAccessCache()
    {
        var request = new DummyCachableRequest { UseCache = false };
        var response = new DummyResponse { Value = "Test" };

        Task<DummyResponse> Next() => Task.FromResult(response);

        var result = await _behavior.Handle(request, Next, CancellationToken.None);

        Assert.Equal(response.Value, result.Value);
        _cacheServiceMock.Verify(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<DummyResponse>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnCachedResponse()
    {
        var request = new DummyCachableRequest
        {
            UseCache = true,
            CacheKey = "dummyKey",
            CacheExpiration = TimeSpan.FromMinutes(5)
        };

        var cachedResponse = new DummyResponse { Value = "Cached" };

        _cacheServiceMock.Setup(x => x.ExistsAsync(request.CacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _cacheServiceMock.Setup(x => x.GetAsync<DummyResponse>(request.CacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        bool nextCalled = false;
        Task<DummyResponse> Next() { nextCalled = true; return Task.FromResult(new DummyResponse { Value = "New" }); }

        var result = await _behavior.Handle(request, Next, CancellationToken.None);

        Assert.False(nextCalled);
        Assert.Equal(cachedResponse.Value, result.Value);
    }

    [Fact]
    public async Task Handle_WhenCacheDoesNotExist_ShouldCallNextAndSetCache()
    {
        var request = new DummyCachableRequest
        {
            UseCache = true,
            CacheKey = "dummyKey",
            CacheExpiration = TimeSpan.FromMinutes(5)
        };

        _cacheServiceMock.Setup(x => x.ExistsAsync(request.CacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var newResponse = new DummyResponse { Value = "New" };

        Task<DummyResponse> Next() => Task.FromResult(newResponse);

        var result = await _behavior.Handle(request, Next, CancellationToken.None);

        Assert.Equal(newResponse.Value, result.Value);
        _cacheServiceMock.Verify(x => x.SetAsync(request.CacheKey, newResponse, request.CacheExpiration.Value, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class CacheRemovingBehaviorTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly CacheRemovingBehavior<DummyCacheRemoverRequest, DummyResponse> _behavior;

    public CacheRemovingBehaviorTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _behavior = new CacheRemovingBehavior<DummyCacheRemoverRequest, DummyResponse>(_cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheKeyNotEmpty_ShouldRemoveCache()
    {
        var request = new DummyCacheRemoverRequest
        {
            CacheKey = "dummyKey",
            RemoveCache = true
        };

        var response = new DummyResponse { Value = "Test" };

        Task<DummyResponse> Next() => Task.FromResult(response);

        var result = await _behavior.Handle(request, Next, CancellationToken.None);

        Assert.Equal(response.Value, result.Value);
        _cacheServiceMock.Verify(x => x.RemoveAsync(request.CacheKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCacheKeyEmpty_ShouldNotRemoveCache()
    {
        var request = new DummyCacheRemoverRequest
        {
            CacheKey = "",
            RemoveCache = true
        };

        var response = new DummyResponse { Value = "Test" };

        Task<DummyResponse> Next() => Task.FromResult(response);

        var result = await _behavior.Handle(request, Next, CancellationToken.None);

        Assert.Equal(response.Value, result.Value);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class CacheKeyHelperTests
{
    [Fact]
    public void GenerateKey_WithValidParts_ShouldReturnJoinedString()
    {
        var result = CacheKeyHelper.GenerateKey("Part1", "Part2", "Part3");
        Assert.Equal("Part1:Part2:Part3", result);
    }

    [Fact]
    public void GenerateKey_WithNullOrEmptyParts_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CacheKeyHelper.GenerateKey());
    }
}

public class InMemoryCacheServiceTests : IDisposable
{
    private readonly InMemoryCacheService _cacheService;
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new InMemoryCacheService(_memoryCache);
    }

    [Fact]
    public async Task SetAndGetAsync_ShouldStoreAndRetrieveValue()
    {
        string key = "testKey";
        var value = "testValue";
        TimeSpan expiration = TimeSpan.FromMinutes(5);

        await _cacheService.SetAsync(key, value, expiration);
        var retrieved = await _cacheService.GetAsync<string>(key);

        Assert.Equal(value, retrieved);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveValue()
    {
        string key = "testKey";
        var value = "testValue";
        TimeSpan expiration = TimeSpan.FromMinutes(5);

        await _cacheService.SetAsync(key, value, expiration);
        await _cacheService.RemoveAsync(key);
        var exists = await _cacheService.ExistsAsync(key);

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrueIfValueExists()
    {
        string key = "testKey";
        var value = "testValue";
        TimeSpan expiration = TimeSpan.FromMinutes(5);

        await _cacheService.SetAsync(key, value, expiration);
        var exists = await _cacheService.ExistsAsync(key);

        Assert.True(exists);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}

public class DistributedCacheServiceTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly DistributedCacheService _cacheService;

    public DistributedCacheServiceTests()
    {
        _distributedCacheMock = new Mock<IDistributedCache>();
        _cacheService = new DistributedCacheService(_distributedCacheMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyNotFound_ShouldReturnDefault()
    {
        string key = "testKey";
        _distributedCacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
    .ReturnsAsync((byte[])null);

        var result = await _cacheService.GetAsync<string>(key);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeAndSetValue()
    {
        string key = "testKey";
        var value = new DummyResponse { Value = "Test" };
        TimeSpan expiration = TimeSpan.FromMinutes(5);

        await _cacheService.SetAsync(key, value, expiration);

        var expectedSerializedData = System.Text.Json.JsonSerializer.Serialize(value);
        _distributedCacheMock.Verify(x => x.SetAsync(key,
            It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == expectedSerializedData),
            It.Is<DistributedCacheEntryOptions>(opts => opts.AbsoluteExpirationRelativeToNow == expiration),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallRemoveAsync()
    {
        string key = "testKey";

        await _cacheService.RemoveAsync(key);

        _distributedCacheMock.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        string key = "testKey";
        _distributedCacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
    .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("some data"));

        var result = await _cacheService.ExistsAsync(key);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
    {
        string key = "testKey";
        _distributedCacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
    .ReturnsAsync((byte[])null);

        var result = await _cacheService.ExistsAsync(key);

        Assert.False(result);
    }
}

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCachingServices_WithInMemoryProvider_ShouldRegisterInMemoryCacheService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddCachingServices(configuration, settings =>
        {
            settings.Provider = CacheProvider.InMemory;
            settings.DefaultExpiration = TimeSpan.FromMinutes(20);
        });

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();
        Assert.NotNull(cacheService);
        Assert.IsType<InMemoryCacheService>(cacheService);
    }

    [Fact]
    public void AddCachingServices_WithDistributedProvider_ShouldRegisterDistributedCacheService()
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
            settings.DefaultExpiration = TimeSpan.FromMinutes(20);
        });

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();
        Assert.NotNull(cacheService);
        Assert.IsType<DistributedCacheService>(cacheService);
    }
}