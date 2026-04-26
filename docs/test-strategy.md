# Test Strategy

Bu dokuman OmerkckArchitecture (`Kck.*`) test piramidini ve hangi test
kategorisinin ne zaman kullanilacagini tanimlar.

## Test Piramidi

```
            /\
           /  \    Integration testleri (Testcontainers)
          /----\   - gercek DB / broker / external service
         /      \  - LS-FAZ-4 PoC: Kck.Persistence.EntityFramework.Tests
        /        \
       /----------\ Unit testleri (xUnit + NSubstitute + InMemory)
      /            \ - tum providerler + abstraction'lar
     /              \ - default test koleksiyonu
    /----------------\
```

## Unit Tests

**Ne icin:** Bir tip/metodun davranisini hizla dogrula.

**Araclar:**
- xUnit `[Fact]` / `[Theory]`
- `FluentAssertions` (assert chain)
- `NSubstitute` (mock interface)
- EF Core: `Microsoft.EntityFrameworkCore.InMemory` provider

**Konum:** `tests/Kck.<Project>.Tests/`

**Filter:** Default test discovery'ye dahil — ozel filter gerekmez.

**Trait:** Yok (default).

**CI:** Hem ubuntu hem windows runner'da kosar.

## Integration Tests (Testcontainers)

**Ne icin:** Mock/InMemory'nin yakalayamadigi gercek davranislari dogrula:
- Concurrency token (xmin, RowVersion)
- Transaction isolation level
- JSON column mappings
- Provider-spesifik query translation
- Connection pool davranisi
- Network/IO timeout pattern'leri

**Araclar:**
- `Testcontainers.PostgreSql`, `Testcontainers.Redis`, `Testcontainers.RabbitMq` vb.
- `IAsyncLifetime` xUnit pattern'i

**Konum:** Mevcut test projesinin icinde, ayri test sinifi:
`<Project>.Tests/<Service>IntegrationTests.cs`

**Trait:** `[Trait("Category", "Integration")]` (sinif veya metod seviyesinde)

**Filter:**
- Unit test step: `--filter "Category!=Integration"`
- Integration test step: `--filter "Category=Integration"` (ubuntu-only)

**CI:** Sadece **ubuntu-latest** runner'da. Windows GitHub-hosted runner'da
Docker pre-installed degil; integration testler skip edilir.

**Yerel calistirma:** Docker Desktop calisir durumda olmali.

```bash
# Sadece unit testler (Docker'siz):
dotnet test --filter "Category!=Integration"

# Sadece integration testler (Docker gerekli):
dotnet test --filter "Category=Integration"
```

## Mevcut Integration Test Coverage (LS-FAZ-4)

| Test Projesi | Container | Durum | Faz |
|---|---|---|---|
| `Kck.Persistence.EntityFramework.Tests` | `postgres:16-alpine` | **PoC** (LS-FAZ-4) | ✓ |
| `Kck.Caching.Redis.Tests` | (mock — InMemory benzeri) | Yayilma bekliyor | LS-FAZ-4.5 |
| `Kck.EventBus.RabbitMq.Tests` | (mock) | Yayilma bekliyor | LS-FAZ-4.5 |
| `Kck.EventBus.AzureServiceBus.Tests` | (mock) | Yayilma bekliyor | LS-FAZ-4.5 |
| `Kck.Search.Elasticsearch.Tests` | (mock) | Yayilma bekliyor | LS-FAZ-4.5 |
| `Kck.Security.Secrets.AzureKeyVault.Tests` | (mock) | Yayilma bekliyor | LS-FAZ-4.5 |

LS-FAZ-4.5 PoC pattern'ini diger 5 test projesine yayinlanmasini hedefliyor.

## Benchmarks (BenchmarkDotNet)

**Ne icin:** Mikro-benchmark ile performans regresyonunu yakala. Test
**degil**, profil aleti.

**Konum:** `tests/Kck.Benchmarks/`

**Calistirma:**

```bash
dotnet run -c Release --project tests/Kck.Benchmarks
dotnet run -c Release --project tests/Kck.Benchmarks -- --filter "*Paginate*"
```

**CI:** Benchmark calistirma CI'da yok (benchmark resultlari deterministik
degil). Gelecekte CI regression detection (bencher.dev veya benchmark
GitHub action) eklenebilir.

## Mutation Testing (Stryker.NET) — Henuz Yok

Bolum 7.4: kritik abstraction'larda (Result, Paginate, Filter) mutation
score baslat — hedef >%70. **Trial mode** olarak isaretli, henuz uygulanmadi.

## Property-Based Testing — Henuz Yok

Bolum 7.3: `Paginate.Create` math, `Filter` parser, `PathSanitizer` icin
FsCheck/CsCheck ile property test. **Trial mode** olarak isaretli.

## Coverage Politikasi

Detay: [`docs/policies/test-coverage.md`](policies/test-coverage.md)

- Mevcut esik: **line %40 / branch %35** (regression onleme)
- Yol haritasi: kademeli yukseltme, test yazimi gerektirir

## Referanslar

- [`docs/policies/test-coverage.md`](policies/test-coverage.md)
- `tasks/library-strategy-2026-04-25.md` Bolum 5.7, 7.1, 7.5
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
