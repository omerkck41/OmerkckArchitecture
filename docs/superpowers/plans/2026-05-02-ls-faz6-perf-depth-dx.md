# LS-FAZ-6: Performans Derinlik & DX Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** CacheServiceBase'deki unbounded memory leak'i lock striping ile düzelt, Result<T>'ye functional pipeline API ekle ve IReadRepository'ye QueryOptions overload'ları ile eski bool API'sini deprecate et.

**Architecture:**
- Asama 1: `CacheServiceBase` — static `ConcurrentDictionary<string, SemaphoreSlim>` kaldırılıp 64-stripe sabit semaphore dizisiyle değiştirilir. Abstractions paketine sıfır bağımlılık eklenmez.
- Asama 2: `Result<T>` — `ResultExtensions` static sınıfına `Map`, `Bind`, `Tap`, `Ensure` extension metodları eklenir. Yeni `Kck.Core.Abstractions.Tests` projesi oluşturulur.
- Asama 3: `IReadRepository` — `QueryOptions` overload'ları default interface implementation olarak eklenir, bool parametreli overload'lar `[Obsolete(DiagnosticId="KCK0100")]` ile işaretlenir.

**Tech Stack:** .NET 10 / net8.0;net10.0 multi-target, xUnit + FluentAssertions + NSubstitute, TreatWarningsAsErrors=true, PublicApiAnalyzers

---

## Mühendislik Kararları (Principal Engineer)

### AsyncKeyedLock vs Lock Striping
`Kck.Caching.Abstractions` sıfır üçüncü-taraf bağımlılığına sahiptir. `AsyncKeyedLock` eklemek tüm tüketicilerin transitif bağımlılık grafiğini büyütür. Lock striping (64 sabit semaphore, key hash'e göre seçim) önbellek stampede prevention için matematiksel olarak yeterlidir: yanlış seri (farklı key → aynı stripe) gecikmeyi artırır ama doğruluğu bozmaz.

### IReadRepository Obsolete Stratejisi
Default interface implementation ile QueryOptions overload eklenir → bool overload `[Obsolete]` işaretlenir → `CS0618` `WarningsNotAsErrors`'a alınır. Bu yaklaşım v0.x'te tüketicilere uyum süresi tanır, v1.0 major bump'ta (LS-FAZ-8) bool overload'lar tamamen kaldırılır.

---

## Dosya Haritası

### Asama 1 — CacheServiceBase Lock Striping
| Eylem | Dosya |
|---|---|
| Modify | `src/Abstractions/Kck.Caching.Abstractions/CacheServiceBase.cs` |
| Test (mevcut) | `tests/Kck.Caching.InMemory.Tests/InMemoryCacheServiceTests.cs` |

### Asama 2 — Result<T> Functional Extensions
| Eylem | Dosya |
|---|---|
| Create | `src/Abstractions/Kck.Core.Abstractions/Results/ResultExtensions.cs` |
| Create | `tests/Kck.Core.Abstractions.Tests/Kck.Core.Abstractions.Tests.csproj` |
| Create | `tests/Kck.Core.Abstractions.Tests/Results/ResultExtensionsTests.cs` |
| Add to solution | `OmerkckArchitecture.sln` |

### Asama 3 — IReadRepository QueryOptions
| Eylem | Dosya |
|---|---|
| Modify | `src/Abstractions/Kck.Persistence.Abstractions/Repositories/IReadRepository.cs` |
| Modify | `src/Providers/Kck.Persistence.EntityFramework/Repositories/EfRepository.cs` |
| Modify | `Directory.Build.props` (CS0618 → WarningsNotAsErrors) |
| Modify | `src/Abstractions/Kck.Persistence.Abstractions/PublicAPI.Unshipped.txt` |
| Test (mevcut) | `tests/Kck.Persistence.EntityFramework.Tests/EfRepositoryTests.cs` |

### Final
| Eylem | Dosya |
|---|---|
| Create | `docs/adr/0016-ls-faz6-perf-depth-dx.md` |
| Modify | `CHANGELOG.md` |
| Modify | `tasks/checkpoint.md` |

---

## Task 1: CacheServiceBase — Lock Striping

**Files:**
- Modify: `src/Abstractions/Kck.Caching.Abstractions/CacheServiceBase.cs`
- Test: `tests/Kck.Caching.InMemory.Tests/InMemoryCacheServiceTests.cs`

### Neden: Mevcut Problem

`CacheServiceBase` şu an `static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks` kullanıyor. Unique key'ler (kullanıcı ID'si, session gibi) için her yeni key bir `SemaphoreSlim` oluşturur ve hiç temizlenmez — unbounded memory leak. Kodun içindeki yorum da bunu itiraf ediyor: *"Locks dictionary grows unbounded for unique keys."*

Lock striping çözümü: 64 adet önceden ayrılmış `SemaphoreSlim` array'i. Key'in hash'i modulo 64 ile stripe seçilir. Memory sabit, doğruluk korunur, sadece farklı key'lerin aynı stripe'a denk gelmesi durumunda küçük seri basıncı oluşur (önbellek için kabul edilebilir).

- [ ] **Step 1: Mevcut concurrent stampede testinin geçtiğini doğrula**

```bash
dotnet test tests/Kck.Caching.InMemory.Tests/ -v normal
```
Beklenen: tüm testler PASS (özellikle `GetOrSetAsync_ConcurrentCalls_ShouldCallFactoryOnlyOnce`).

- [ ] **Step 2: Lock striping implementasyonunu uygula**

`src/Abstractions/Kck.Caching.Abstractions/CacheServiceBase.cs` dosyasını şu içerikle değiştir:

```csharp
namespace Kck.Caching.Abstractions;

public abstract class CacheServiceBase : ICacheService
{
    // 64-stripe lock array: constant memory, zero external deps, correct for cache stampede prevention.
    // Instance-level (not static) so InMemory and Redis providers don't share stripes and
    // unnecessarily serialize each other's GetOrSetAsync calls.
    // False serialization (two distinct keys → same stripe) only reduces parallelism, never correctness.
    // Power-of-two count enables cheap bitwise modulo.
    private readonly SemaphoreSlim[] _stripes =
        Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    private SemaphoreSlim GetStripe(string key) =>
        _stripes[(uint)key.GetHashCode() % (uint)_stripes.Length];

    protected abstract CacheOptions Options { get; }

    public abstract Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    public abstract Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    public abstract Task RemoveAsync(string key, CancellationToken ct = default);
    public abstract Task<bool> ExistsAsync(string key, CancellationToken ct = default);
    public abstract Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var existing = await GetAsync<T>(key, ct).ConfigureAwait(false);
        if (existing is not null)
            return existing;

        var semaphore = GetStripe(BuildKey(key));
        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            existing = await GetAsync<T>(key, ct).ConfigureAwait(false);
            if (existing is not null)
                return existing;

            var value = await factory().ConfigureAwait(false);
            await SetAsync(key, value, expiration, ct).ConfigureAwait(false);
            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    protected string BuildKey(string key) =>
        string.IsNullOrEmpty(Options.KeyPrefix) ? key : $"{Options.KeyPrefix}{key}";
}
```

- [ ] **Step 3: Build kontrolü**

```bash
dotnet build src/Abstractions/Kck.Caching.Abstractions/ -c Release
```
Beklenen: `0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Tüm testleri çalıştır**

```bash
dotnet test tests/Kck.Caching.InMemory.Tests/ -v normal
```
Beklenen: tüm testler PASS. `GetOrSetAsync_ConcurrentCalls_ShouldCallFactoryOnlyOnce` hâlâ geçmeli.

- [ ] **Step 5: Yeni test — yüksek unique-key hacminde GetOrSetAsync çalışır**

`tests/Kck.Caching.InMemory.Tests/InMemoryCacheServiceTests.cs` dosyasına şu testi ekle:

```csharp
[Fact]
public async Task GetOrSetAsync_HighCardinalityKeys_ShouldCompleteWithoutError()
{
    // Eski implementasyon 1000 unique key için 1000 SemaphoreSlim biriktirirdi (memory leak).
    // Yeni stripe implementasyonu sabit 64 instance kullanır.
    // Not: memory doğrulaması için dotnet-counters profiler kullanın.
    for (var i = 0; i < 1000; i++)
    {
        await _sut.GetOrSetAsync($"unique-key-{i}", () => Task.FromResult($"v{i}"));
    }

    var result = await _sut.GetOrSetAsync("unique-key-0", () => Task.FromResult("miss"));
    result.Should().Be("v0", "cached value should still be accessible after high-cardinality load");
}
```

- [ ] **Step 6: Testleri çalıştır**

```bash
dotnet test tests/Kck.Caching.InMemory.Tests/ -v normal
```
Beklenen: tüm testler PASS (yeni test dahil).

- [ ] **Step 7: PublicAPI.Unshipped.txt kontrol**

```bash
dotnet build src/Abstractions/Kck.Caching.Abstractions/ -c Release
```
`PublicAPI.Unshipped.txt`'e yeni public member eklenmedi — dosya değişmemiş olmalı.

- [ ] **Step 8: Commit**

```bash
git checkout -b feature/ls-faz6-perf-depth-dx
git add src/Abstractions/Kck.Caching.Abstractions/CacheServiceBase.cs
git add tests/Kck.Caching.InMemory.Tests/InMemoryCacheServiceTests.cs
git commit -m "perf(caching): replace unbounded SemaphoreSlim dict with 64-stripe lock array

CacheServiceBase.GetOrSetAsync held a static ConcurrentDictionary that grew
without bound for unique cache keys (session IDs, per-user keys). Replaces it
with a fixed 64-element stripe array — constant memory, zero external deps.
False serialization risk is acceptable for cache stampede prevention.

Closes LS-FAZ-6 §5.2 (P1 memory leak)"
```

---

## Task 2: Result<T> — Functional Pipeline Extensions

**Files:**
- Create: `src/Abstractions/Kck.Core.Abstractions/Results/ResultExtensions.cs`
- Create: `tests/Kck.Core.Abstractions.Tests/Kck.Core.Abstractions.Tests.csproj`
- Create: `tests/Kck.Core.Abstractions.Tests/Results/ResultExtensionsTests.cs`

### Neden: Mevcut Problem

`Result<T>` şu an sadece `Match` sunuyor — bu terminal bir işlem. `Map`/`Bind`/`Tap`/`Ensure` olmadan her adımda `Match` yazılması gerekiyor ki bu kod gürültüsü oluşturur. Bu dört metod standart monadik operasyon ve sıfır kırıcı değişiklik: tamamen yeni bir extension sınıfı.

**API Tasarımı:**
- `Map<U>(Func<T, U> mapper)` — başarı değerini dönüştür, hata geçişini koru
- `Bind<U>(Func<T, Result<U>> binder)` — başarı → yeni Result zinciri (flatMap)  
- `Tap(Action<T> action)` — yan etki çalıştır, Result'ı değiştirme (loglama/event için)
- `Ensure(Func<T, bool> predicate, Error error)` — koşul sağlanmazsa failure'a çevir

- [ ] **Step 1: Test projesi oluştur**

```bash
dotnet new xunit -n Kck.Core.Abstractions.Tests -o tests/Kck.Core.Abstractions.Tests --framework net10.0
```

- [ ] **Step 2: Test projesinin csproj'unu düzelt**

`tests/Kck.Core.Abstractions.Tests/Kck.Core.Abstractions.Tests.csproj` içeriğini şununla değiştir:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Explicit: Directory.Build.props sets net8.0;net10.0 default but test projects target net10.0 only -->
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <ProjectReference Include="..\..\src\Abstractions\Kck.Core.Abstractions\Kck.Core.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

Otomatik oluşturulan `UnitTest1.cs` ve `GlobalUsings.cs` dosyalarını sil.

- [ ] **Step 3: Test projesini solution'a ekle**

```bash
dotnet sln add tests/Kck.Core.Abstractions.Tests/Kck.Core.Abstractions.Tests.csproj
```

- [ ] **Step 4: Önce başarısız testleri yaz**

`tests/Kck.Core.Abstractions.Tests/Results/ResultExtensionsTests.cs`:

```csharp
using FluentAssertions;
using Kck.Core.Abstractions.Results;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Results;

public sealed class ResultExtensionsTests
{
    private static readonly Error TestError = new("TEST_ERR", "test error");

    // --- Map ---

    [Fact]
    public void Map_SuccessResult_ShouldTransformValue()
    {
        var result = Result<int>.Success(5);
        var mapped = result.Map(x => x * 2);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_FailureResult_ShouldPropagateError()
    {
        var result = Result<int>.Failure(TestError);
        var mapped = result.Map(x => x * 2);
        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().Be(TestError);
    }

    // --- Bind ---

    [Fact]
    public void Bind_SuccessResult_ShouldChainNextOperation()
    {
        var result = Result<int>.Success(5);
        var bound = result.Bind(x => Result<string>.Success(x.ToString()));
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_SuccessResult_WhenBinderFails_ShouldReturnFailure()
    {
        var result = Result<int>.Success(5);
        var bound = result.Bind(_ => Result<string>.Failure(TestError));
        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().Be(TestError);
    }

    [Fact]
    public void Bind_FailureResult_ShouldNotCallBinder()
    {
        var binderCalled = false;
        var result = Result<int>.Failure(TestError);
        var bound = result.Bind(x => { binderCalled = true; return Result<string>.Success(x.ToString()); });
        bound.IsSuccess.Should().BeFalse();
        binderCalled.Should().BeFalse();
    }

    // --- Tap ---

    [Fact]
    public void Tap_SuccessResult_ShouldExecuteActionAndReturnSameResult()
    {
        var executed = false;
        var result = Result<int>.Success(42);
        var tapped = result.Tap(_ => executed = true);
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(42);
        executed.Should().BeTrue();
    }

    [Fact]
    public void Tap_FailureResult_ShouldNotExecuteAction()
    {
        var executed = false;
        var result = Result<int>.Failure(TestError);
        var tapped = result.Tap(_ => executed = true);
        tapped.IsSuccess.Should().BeFalse();
        executed.Should().BeFalse();
    }

    // --- Ensure ---

    [Fact]
    public void Ensure_SuccessResult_PredicatePasses_ShouldRemainSuccess()
    {
        var result = Result<int>.Success(10);
        var ensured = result.Ensure(x => x > 5, TestError);
        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public void Ensure_SuccessResult_PredicateFails_ShouldReturnFailure()
    {
        var result = Result<int>.Success(3);
        var ensured = result.Ensure(x => x > 5, TestError);
        ensured.IsSuccess.Should().BeFalse();
        ensured.Error.Should().Be(TestError);
    }

    [Fact]
    public void Ensure_FailureResult_ShouldPropagateOriginalError()
    {
        var differentError = new Error("OTHER", "other error");
        var result = Result<int>.Failure(TestError);
        var ensured = result.Ensure(x => x > 5, differentError);
        ensured.IsSuccess.Should().BeFalse();
        ensured.Error.Should().Be(TestError, "original error should propagate, not the ensure error");
    }

    // --- Pipeline ---

    [Fact]
    public void Pipeline_ChainedOperations_ShouldWorkTogether()
    {
        var sideEffectValue = 0;
        var result = Result<int>.Success(5)
            .Ensure(x => x > 0, new Error("NEG", "must be positive"))
            .Map(x => x * 2)
            .Tap(x => sideEffectValue = x)
            .Bind(x => x > 5
                ? Result<string>.Success($"large:{x}")
                : Result<string>.Failure(new Error("SMALL", "too small")));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("large:10");
        sideEffectValue.Should().Be(10);
    }
}
```

- [ ] **Step 5: Testi çalıştır — başarısız olduğunu doğrula**

```bash
dotnet test tests/Kck.Core.Abstractions.Tests/ -v normal
```
Beklenen: BUILD ERROR — `ResultExtensions` sınıfı henüz yok.

- [ ] **Step 6: ResultExtensions implementasyonunu yaz**

`src/Abstractions/Kck.Core.Abstractions/Results/ResultExtensions.cs`:

```csharp
namespace Kck.Core.Abstractions.Results;

/// <summary>
/// Functional pipeline extensions for <see cref="Result{T}"/>:
/// Map (transform value), Bind (chain operations), Tap (side effects), Ensure (guard).
/// </summary>
public static class ResultExtensions
{
    /// <summary>Transforms the success value; propagates failure unchanged.</summary>
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return result.IsSuccess
            ? Result<U>.Success(mapper(result.Value!))
            : Result<U>.Failure(result.Error!);
    }

    /// <summary>Chains a Result-returning operation; propagates failure without calling binder.</summary>
    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return result.IsSuccess
            ? binder(result.Value!)
            : Result<U>.Failure(result.Error!);
    }

    /// <summary>Executes a side-effect action on success; returns the original result unchanged.</summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.IsSuccess)
            action(result.Value!);
        return result;
    }

    /// <summary>Converts to failure if the predicate is not satisfied; propagates existing failure.</summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (!result.IsSuccess)
            return result;
        return predicate(result.Value!)
            ? result
            : Result<T>.Failure(error);
    }
}
```

- [ ] **Step 7: Testleri çalıştır — geçtiğini doğrula**

```bash
dotnet test tests/Kck.Core.Abstractions.Tests/ -v normal
```
Beklenen: tüm testler PASS.

- [ ] **Step 8: PublicAPI.Unshipped.txt güncelle**

`src/Abstractions/Kck.Core.Abstractions/PublicAPI.Unshipped.txt` dosyasına ekle:

```
static Kck.Core.Abstractions.Results.ResultExtensions.Bind<T, U>(this Kck.Core.Abstractions.Results.Result<T>! result, System.Func<T, Kck.Core.Abstractions.Results.Result<U>!>! binder) -> Kck.Core.Abstractions.Results.Result<U>!
static Kck.Core.Abstractions.Results.ResultExtensions.Ensure<T>(this Kck.Core.Abstractions.Results.Result<T>! result, System.Func<T, bool>! predicate, Kck.Core.Abstractions.Results.Error error) -> Kck.Core.Abstractions.Results.Result<T>!
static Kck.Core.Abstractions.Results.ResultExtensions.Map<T, U>(this Kck.Core.Abstractions.Results.Result<T>! result, System.Func<T, U>! mapper) -> Kck.Core.Abstractions.Results.Result<U>!
static Kck.Core.Abstractions.Results.ResultExtensions.Tap<T>(this Kck.Core.Abstractions.Results.Result<T>! result, System.Action<T>! action) -> Kck.Core.Abstractions.Results.Result<T>!
Kck.Core.Abstractions.Results.ResultExtensions
```

- [ ] **Step 9: Tam build**

```bash
dotnet build src/Abstractions/Kck.Core.Abstractions/ -c Release
```
Beklenen: `0 Warning(s) 0 Error(s)`

> **Not:** PublicAPI analyzer tam imzaları otomatik üretir. Build hata verirse `PublicAPI.Unshipped.txt`'deki satırları build çıktısındaki önerilerle değiştir.

- [ ] **Step 10: Commit**

```bash
git add src/Abstractions/Kck.Core.Abstractions/Results/ResultExtensions.cs
git add src/Abstractions/Kck.Core.Abstractions/PublicAPI.Unshipped.txt
git add tests/Kck.Core.Abstractions.Tests/
git add OmerkckArchitecture.sln
git commit -m "feat(core): add Map/Bind/Tap/Ensure functional pipeline extensions to Result<T>

Enables composing Result operations without repeated Match calls.
Pure extension class — additive, zero breaking changes.

Closes LS-FAZ-6 §4.3"
```

---

## Task 3: IReadRepository — QueryOptions Overloads + Obsolete Bool API

**Files:**
- Modify: `src/Abstractions/Kck.Persistence.Abstractions/Repositories/IReadRepository.cs`
- Modify: `src/Providers/Kck.Persistence.EntityFramework/Repositories/EfRepository.cs`
- Modify: `Directory.Build.props`
- Modify: `src/Abstractions/Kck.Persistence.Abstractions/PublicAPI.Unshipped.txt`
- Test: `tests/Kck.Persistence.EntityFramework.Tests/EfRepositoryTests.cs`

### Neden: Mevcut Problem

`IReadRepository` şu an `bool withDeleted = false, bool enableTracking = true` pozisyonel parametreler kullanıyor. Arama sitelerinde `GetAsync(pred, null, false, true)` gibi anlaşılmaz çağrılar oluşuyor. `QueryOptions` record zaten mevcut (`QueryOptions.Tracking`, `QueryOptions.WithDeleted` gibi hazır factory property'leri var) ama interface bu record'u kabul etmiyor.

**Strateji:**
1. `QueryOptions` overload'larını **default interface implementation** olarak ekle (non-breaking)
2. Bool parametreli overload'ları `[Obsolete(DiagnosticId = "KCK0100")]` ile işaretle
3. `Directory.Build.props`'a `CS0618` → `WarningsNotAsErrors` ekle (geçiş süresi)
4. `EfRepository`'ye concrete `QueryOptions` implementasyonu ekle (optimize path)

- [ ] **Step 1: CS0618 WarningsNotAsErrors'a ekle**

`Directory.Build.props` içindeki `<WarningsNotAsErrors>` satırını bul ve `CS0618` ekle:

```xml
<WarningsNotAsErrors>$(WarningsNotAsErrors);CA1716;CA1000;CS0618</WarningsNotAsErrors>
```

Neden: `[Obsolete]` işaretli metodu implement etmek CS0618 üretir (EfRepository etkilenir). Bu uyarıyı hata olmaktan çıkar ama görünür bırak.

- [ ] **Step 2: Build — mevcut state temiz**

```bash
dotnet build -c Release
```
Beklenen: `0 Error(s)`

- [ ] **Step 3: IReadRepository'ye QueryOptions overload'larını ekle**

`src/Abstractions/Kck.Persistence.Abstractions/Repositories/IReadRepository.cs`:

```csharp
using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Read-only repository operations. Use when write access is not needed (ISP compliance).
/// </summary>
public interface IReadRepository<T, TId> : IQuery<T> where T : Entity<TId>
{
    // ── Bool overloads (deprecated) ────────────────────────────────────────────
    // Prefer the QueryOptions overloads below. These will be removed in v2.0.

    /// <inheritdoc cref="GetAsync(Expression{Func{T,bool}},QueryOptions,Expression{Func{T,object}}[]?,CancellationToken)"/>
    [Obsolete("Use the QueryOptions overload instead. Will be removed in v2.0.", DiagnosticId = "KCK0100")]
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
                      Expression<Func<T, object>>[]? includes = null,
                      bool withDeleted = false,
                      bool enableTracking = true,
                      CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetListAsync(Expression{Func{T,bool}}?,Func{IQueryable{T},IOrderedQueryable{T}}?,QueryOptions,Expression{Func{T,object}}[]?,int,int,CancellationToken)"/>
    [Obsolete("Use the QueryOptions overload instead. Will be removed in v2.0.", DiagnosticId = "KCK0100")]
    Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null,
                                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                    Expression<Func<T, object>>[]? includes = null,
                                    int index = 0, int size = 10,
                                    bool withDeleted = false,
                                    bool enableTracking = true,
                                    CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetListByDynamicAsync(Dynamic.DynamicQuery,Expression{Func{T,bool}}?,QueryOptions,Expression{Func{T,object}}[]?,int,int,CancellationToken)"/>
    [Obsolete("Use the QueryOptions overload instead. Will be removed in v2.0.", DiagnosticId = "KCK0100")]
    Task<IPaginate<T>> GetListByDynamicAsync(Dynamic.DynamicQuery dynamic,
                                             Expression<Func<T, bool>>? predicate = null,
                                             Expression<Func<T, object>>[]? includes = null,
                                             int index = 0, int size = 10,
                                             bool withDeleted = false,
                                             bool enableTracking = true,
                                             CancellationToken cancellationToken = default);

    /// <inheritdoc cref="AnyAsync(Expression{Func{T,bool}}?,QueryOptions,CancellationToken)"/>
    [Obsolete("Use the QueryOptions overload instead. Will be removed in v2.0.", DiagnosticId = "KCK0100")]
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null,
                        bool withDeleted = false,
                        bool enableTracking = false,
                        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetByIdAsync(TId,QueryOptions,CancellationToken)"/>
    [Obsolete("Use the QueryOptions overload instead. Will be removed in v2.0.", DiagnosticId = "KCK0100")]
    Task<T?> GetByIdAsync(TId id,
                          bool withDeleted = false,
                          bool enableTracking = false,
                          CancellationToken cancellationToken = default);

    /// <inheritdoc cref="CountAsync(Expression{Func{T,bool}}?,QueryOptions,CancellationToken)"/>
    [Obsolete("Use the QueryOptions overload instead. Will be removed in v2.0.", DiagnosticId = "KCK0100")]
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                         bool withDeleted = false,
                         bool enableTracking = false,
                         CancellationToken cancellationToken = default);

    // ── QueryOptions overloads (preferred) ────────────────────────────────────

    /// <summary>Returns the first entity matching <paramref name="predicate"/> or null.</summary>
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
                      QueryOptions options,
                      Expression<Func<T, object>>[]? includes = null,
                      CancellationToken cancellationToken = default)
#pragma warning disable CS0618
        => GetAsync(predicate, includes, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore CS0618

    /// <summary>Returns a paginated list of entities matching the optional <paramref name="predicate"/>.</summary>
    Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null,
                                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                    QueryOptions options = default,
                                    Expression<Func<T, object>>[]? includes = null,
                                    int index = 0, int size = 10,
                                    CancellationToken cancellationToken = default)
#pragma warning disable CS0618
        => GetListAsync(predicate, orderBy, includes, index, size, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore CS0618

    /// <summary>Returns a paginated list filtered by a dynamic query.</summary>
    Task<IPaginate<T>> GetListByDynamicAsync(Dynamic.DynamicQuery dynamic,
                                             Expression<Func<T, bool>>? predicate = null,
                                             QueryOptions options = default,
                                             Expression<Func<T, object>>[]? includes = null,
                                             int index = 0, int size = 10,
                                             CancellationToken cancellationToken = default)
#pragma warning disable CS0618
        => GetListByDynamicAsync(dynamic, predicate, includes, index, size, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore CS0618

    /// <summary>Returns true if any entity satisfies the optional <paramref name="predicate"/>.</summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null,
                        QueryOptions options = default,
                        CancellationToken cancellationToken = default)
#pragma warning disable CS0618
        => AnyAsync(predicate, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore CS0618

    /// <summary>Returns the entity with the given <paramref name="id"/> or null.</summary>
    Task<T?> GetByIdAsync(TId id,
                          QueryOptions options = default,
                          CancellationToken cancellationToken = default)
#pragma warning disable CS0618
        => GetByIdAsync(id, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore CS0618

    /// <summary>Returns the count of entities satisfying the optional <paramref name="predicate"/>.</summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                         QueryOptions options = default,
                         CancellationToken cancellationToken = default)
#pragma warning disable CS0618
        => CountAsync(predicate, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore CS0618
}
```

- [ ] **Step 4: Build — hata yok mu?**

```bash
dotnet build src/Abstractions/Kck.Persistence.Abstractions/ -c Release
```
Beklenen: `0 Error(s)`

- [ ] **Step 5: EfRepository'ye concrete QueryOptions implementasyonları ekle**

`EfRepository` override etmezse default interface implementation'lar `GetAsync(bool)` bool overload'ları üzerinden çalışır — bu doğru ama EfRepository'nin kendi `Query()` metodunu doğrudan çağırarak daha verimli olabilir. Aşağıdaki 6 metodu `EfRepository` sınıfına ekle (mevcut `GetAsync` metodunun hemen altına):

```csharp
// ── QueryOptions overloads (IReadRepository default impl override) ─────────

/// <inheritdoc/>
public Task<T?> GetAsync(
    Expression<Func<T, bool>> predicate,
    QueryOptions options,
    Expression<Func<T, object>>[]? includes = null,
    CancellationToken cancellationToken = default)
    => GetAsync(predicate, includes, options.IncludeDeleted, options.AsTracking, cancellationToken);

/// <inheritdoc/>
public Task<IPaginate<T>> GetListAsync(
    Expression<Func<T, bool>>? predicate = null,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
    QueryOptions options = default,
    Expression<Func<T, object>>[]? includes = null,
    int index = 0, int size = 10,
    CancellationToken cancellationToken = default)
    => GetListAsync(predicate, orderBy, includes, index, size, options.IncludeDeleted, options.AsTracking, cancellationToken);

/// <inheritdoc/>
public Task<IPaginate<T>> GetListByDynamicAsync(
    DynamicQuery dynamic,
    Expression<Func<T, bool>>? predicate = null,
    QueryOptions options = default,
    Expression<Func<T, object>>[]? includes = null,
    int index = 0, int size = 10,
    CancellationToken cancellationToken = default)
    => GetListByDynamicAsync(dynamic, predicate, includes, index, size, options.IncludeDeleted, options.AsTracking, cancellationToken);

/// <inheritdoc/>
public Task<bool> AnyAsync(
    Expression<Func<T, bool>>? predicate = null,
    QueryOptions options = default,
    CancellationToken cancellationToken = default)
    => AnyAsync(predicate, options.IncludeDeleted, options.AsTracking, cancellationToken);

/// <inheritdoc/>
public Task<T?> GetByIdAsync(
    TId id,
    QueryOptions options = default,
    CancellationToken cancellationToken = default)
    => GetByIdAsync(id, options.IncludeDeleted, options.AsTracking, cancellationToken);

/// <inheritdoc/>
public Task<int> CountAsync(
    Expression<Func<T, bool>>? predicate = null,
    QueryOptions options = default,
    CancellationToken cancellationToken = default)
    => CountAsync(predicate, options.IncludeDeleted, options.AsTracking, cancellationToken);
```

- [ ] **Step 6: Full solution build**

```bash
dotnet build -c Release
```
Beklenen: `0 Error(s)`

> **CS0618 notu:** `EfRepository`'nin kendi concrete metodları (`this.GetAsync(pred, includes, bool, bool, ct)` gibi) `[Obsolete]`'den etkilenmez — `[Obsolete]` interface deklarasyonunda, concrete class metodunda değil. CS0618 sadece `IReadRepository<T,TId>` referansı üzerinden bool overload çağrıldığında üretilir. `Directory.Build.props`'taki `CS0618 → WarningsNotAsErrors` bu call site'ları hata olmaktan çıkarır.

- [ ] **Step 7: Var olan EF testlerinin geçtiğini doğrula**

```bash
dotnet test tests/Kck.Persistence.EntityFramework.Tests/ -v normal
```
Beklenen: tüm testler PASS.

- [ ] **Step 8: QueryOptions overload testleri ekle**

`tests/Kck.Persistence.EntityFramework.Tests/EfRepositoryTests.cs` dosyasını aç, mevcut test sınıfına şu testleri ekle.

> **Önemli:** Test sınıfının constructor'ını incele, `_context` ve `_sut` isimlerini gerçek değişken isimleriyle eşleştir. `QueryOptions.Default` yoksa `new QueryOptions()` ya da `default(QueryOptions)` kullan.

```csharp
[Fact]
public async Task GetByIdAsync_WithQueryOptions_ShouldReturnEntity()
{
    // Arrange
    var entity = await _sut.AddAsync(new TestEntity { Name = "qo-test" });
    await _context.SaveChangesAsync();

    // Act — QueryOptions overload (yeni API)
    var result = await _sut.GetByIdAsync(entity.Id, new QueryOptions());

    // Assert
    result.Should().NotBeNull();
    result!.Name.Should().Be("qo-test");
}

[Fact]
public async Task GetListAsync_WithQueryOptions_ShouldReturnResults()
{
    // Arrange
    await _sut.AddAsync(new TestEntity { Name = "qo-list-1" });
    await _context.SaveChangesAsync();

    // Act — QueryOptions overload (yeni API)
    var list = await _sut.GetListAsync(options: new QueryOptions());

    // Assert
    list.Items.Should().NotBeEmpty();
}
```

> **Not — Tracking testi:** EF Core InMemory provider tracking assertion için güvenilir değildir (ChangeTracker davranışı provider'a göre değişir). Tracking doğrulaması için mevcut Testcontainers integration test pattern'ini (`Kck.Persistence.EntityFramework.Tests/EfRepositoryIntegrationTests.cs`) kullanın.

- [ ] **Step 9: Testleri çalıştır**

```bash
dotnet test tests/Kck.Persistence.EntityFramework.Tests/ -v normal
```
Beklenen: tüm testler PASS.

- [ ] **Step 10: PublicAPI.Unshipped.txt güncelle**

```bash
dotnet build src/Abstractions/Kck.Persistence.Abstractions/ -c Release
```
Build çıktısındaki API analyzer önerilerini `PublicAPI.Unshipped.txt`'e ekle. Yeni QueryOptions overload'larının tamamı listelenmeli.

- [ ] **Step 11: Commit**

```bash
git add src/Abstractions/Kck.Persistence.Abstractions/Repositories/IReadRepository.cs
git add src/Providers/Kck.Persistence.EntityFramework/Repositories/EfRepository.cs
git add src/Abstractions/Kck.Persistence.Abstractions/PublicAPI.Unshipped.txt
git add Directory.Build.props
git add tests/Kck.Persistence.EntityFramework.Tests/
git commit -m "feat(persistence): add QueryOptions overloads to IReadRepository, deprecate bool API

Adds QueryOptions-based overloads via default interface implementations.
Bool-parametered overloads marked [Obsolete(DiagnosticId=KCK0100)] for v2.0 removal.
EfRepository gets concrete implementations for direct dispatch.
CS0618 moved to WarningsNotAsErrors for transition period.

Closes LS-FAZ-6 §4.2"
```

---

## Task 4: ADR + Changelog

**Files:**
- Create: `docs/adr/0016-ls-faz6-perf-depth-dx.md`
- Modify: `CHANGELOG.md`

- [ ] **Step 1: ADR yaz**

`docs/adr/0016-ls-faz6-perf-depth-dx.md`:

```markdown
# ADR-0016: LS-FAZ-6 Mimari Kararları — Perf Derinlik & DX

**Tarih:** 2026-05-02  
**Durum:** Kabul Edildi  
**Bağlam:** Library Strategy FAZ-6 (rapor §4.2, §4.3, §5.2)

## Karar 1 — CacheServiceBase: AsyncKeyedLock yerine Lock Striping

**Problem:** `static ConcurrentDictionary<string, SemaphoreSlim>` unique key'ler için sınırsız büyüme.

**Seçenekler:**
- A) `AsyncKeyedLock` paketi (4M+ download, MIT)
- B) Lock striping — 64 sabit `SemaphoreSlim`, key hash % 64

**Karar:** B — Lock Striping  
**Gerekçe:** `Kck.Caching.Abstractions` sıfır üçüncü-taraf bağımlılığa sahip; bunu korumak tüm tüketicilerin bağımlılık grafiğini küçük tutar. Lock striping, önbellek stampede prevention için matematiksel olarak yeterli: false serialization (farklı key → aynı stripe) gecikmeyi artırır, doğruluğu bozmaz. 64-stripe: 256 byte sabit bellek.

## Karar 2 — Result<T> Extension API

**Karar:** `Map`, `Bind`, `Tap`, `Ensure` — static extension sınıfı.  
**Gerekçe:** Her biri tek sorumluluk; extension class mevcut sealed tipi değiştirmeden genişletir. `Bind` = flatMap (Railway Oriented Programming). `Tap` = yan etki köprüsü (loglama). `Ensure` = guard clause.

## Karar 3 — IReadRepository Bool API Deprecation

**Karar:** Default interface implementation olarak QueryOptions overload'ları, bool overload'lar `[Obsolete(DiagnosticId="KCK0100")]`.  
**Gerekçe:** Non-breaking (mevcut implementorlar otomatik default impl alır). v0.x'te geçiş süresi tanınır. v2.0'da bool overload'lar tamamen kaldırılır (LS-FAZ-8 kapsamı). CS0618 → WarningsNotAsErrors ile tüketiciler derleme hatası almaz ama uyarı alır.

## Ertelenen Maddeler

- **§5.1 / §13.4** JSON source-gen: per-type `JsonSerializerContext` tasarım tartışması gerekli, ayrı ADR.
- **§9.4** Migration guide: v1.0 major bump öncesi yazmak erken.
- **§13.5** Mutation testing: Stryker.NET kurulum fazı ayrı.
```

- [ ] **Step 2: CHANGELOG.md güncelle**

`CHANGELOG.md`'deki `[Unreleased]` bölümüne ekle:

```markdown
### Added
- `Result<T>`: `Map`, `Bind`, `Tap`, `Ensure` functional pipeline extension methods (`Kck.Core.Abstractions`)
- `IReadRepository<T,TId>`: `QueryOptions` overloads for `GetAsync`, `GetListAsync`, `GetListByDynamicAsync`, `AnyAsync`, `GetByIdAsync`, `CountAsync`

### Changed
- `CacheServiceBase.GetOrSetAsync`: replaced unbounded `ConcurrentDictionary<string, SemaphoreSlim>` with 64-stripe fixed lock array — eliminates memory leak for unique cache keys

### Deprecated
- `IReadRepository<T,TId>`: bool-parameter overloads (`withDeleted`, `enableTracking`) marked `[Obsolete(DiagnosticId="KCK0100")]`; use `QueryOptions` overloads. Will be removed in v2.0.
```

- [ ] **Step 3: Commit**

```bash
git add docs/adr/0016-ls-faz6-perf-depth-dx.md
git add CHANGELOG.md
git commit -m "docs: add ADR-0016 and CHANGELOG entries for LS-FAZ-6"
```

---

## Task 5: Son Doğrulama & PR

- [ ] **Step 1: Full solution build**

```bash
dotnet build -c Release
```
Beklenen: `0 Error(s)`

- [ ] **Step 2: Tüm testler**

```bash
dotnet test --configuration Release
```
Beklenen: tüm testler PASS.

- [ ] **Step 3: Checkpoint güncelle**

`tasks/checkpoint.md` dosyasında FAZ-6 satırını güncelle:

```
| 6 | Performans Derinlik & DX | 5.2, 4.3, 4.2 | Additive + Obsolete | **TAMAMLANDI — PR BEKLIYOR** |
```

- [ ] **Step 4: PR aç (kullanıcı onayından sonra)**

```bash
gh pr create \
  --title "LS-FAZ-6: Performans Derinlik & DX" \
  --body "..." \
  --base main \
  --head feature/ls-faz6-perf-depth-dx
```

PR açmadan önce kullanıcıdan onay al.

---

## Özet Tablo

| Task | Kapsam | Risk | Test Stratejisi |
|---|---|---|---|
| 1 — Lock Striping | `CacheServiceBase` | Düşük (behavioral change, test coverage var) | Mevcut stampede testi + yeni unique-key testi |
| 2 — Result Extensions | `Kck.Core.Abstractions` | Sıfır (additive) | Yeni test projesi, 11 test |
| 3 — QueryOptions API | `IReadRepository` + `EfRepository` | Orta (interface change, CS0618) | Mevcut EF testleri + 2 yeni QueryOptions testi |
| 4 — ADR + Changelog | Belgeler | — | — |

**Toplam beklenen değişiklik:** ~4 dosya modify, ~3 dosya create, ~1 yeni test projesi, ~13 yeni test.
