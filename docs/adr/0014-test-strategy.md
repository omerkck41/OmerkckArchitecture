# ADR-0014: Test Stratejisi (BenchmarkDotNet + Testcontainers + Coverage Policy)

Tarih: 2026-04-26
Durum: Onaylandi

## Baglam

Library Strategy raporu (2026-04-25) Bolum 5.7, 7.1, 7.5:

- **5.7:** `BenchmarkDotNet` yok, `tests/benchmarks/` klasoru yok. Performans
  iyilestirmesi yapildiginda (LS-FAZ-5/6 hedeflenenler) **regresyonu
  gorebilecek baseline yok**.
- **7.1:** Coverage gate `LINE=40 / BRANCH=30`. Yerel olcum `40.7 / 36.3`.
  Endustri standardi (Polly, Serilog, EF Core) %80+ — fark cok buyuk.
- **7.5:** 6 integration test projesi var: `Kck.Caching.Redis.Tests`,
  `Kck.EventBus.RabbitMq.Tests`, `Kck.EventBus.AzureServiceBus.Tests`,
  `Kck.Search.Elasticsearch.Tests`, `Kck.Persistence.EntityFramework.Tests`,
  `Kck.Security.Secrets.AzureKeyVault.Tests` — **gercek container ile mi yoksa
  mock'la mi?** Ozelikle persistence/ORM tarafinda mock-with-InMemory "prod'da
  patladi" risk vektoru.

Kutuphane bu uc eksigi tek faza topladi (LS-FAZ-4): altyapi tanitma. Coverage
**yukseltme** ve Testcontainers'i **5 diger projeye yayma** ayri fazlar
(test yazimi gerektirir).

## Karar

Uc altyapi parcasi LS-FAZ-4 kapsaminda kuruldu:

### 1. BenchmarkDotNet Iskelesi
- `tests/Kck.Benchmarks` projesi (net10.0 console app)
- `BenchmarkDotNet 0.15.0` -> `Directory.Packages.props`
- 3 baslangic benchmark (dis-bagimliliksiz):
  `PaginateCreateBenchmarks`, `ResultBenchmarks`, `JsonSerializationBenchmarks`
- Redis/EF/MediatR benchmark'lari Testcontainers entegrasyonu sonrasi
  (LS-FAZ-4.5+) eklenir
- README run instructions

### 2. Coverage Gate Sertlestirme (Regression Onleme)
- `BRANCH_THRESHOLD: 30 -> 35` (gercek %36.3 -> 1.3 puan pay)
- `LINE_THRESHOLD: 40` (sabit, gercek %40.7)
- `docs/policies/test-coverage.md` ile yol haritasi: 40/35 -> 50/45 -> 65/50 -> 75/60
- **Esik yukseltme test yazimi gerektirir** — kararname ile yukseltilmez

### 3. Testcontainers PoC
- `Kck.Persistence.EntityFramework.Tests` icine `EfRepositoryIntegrationTests`
- `postgres:16-alpine` container, `IAsyncLifetime` lifecycle
- `[Trait("Category", "Integration")]` ile filtreleniyor
- CI'da unit/integration step'leri ayri:
  - Unit: hem ubuntu hem windows (`Category!=Integration`)
  - Integration: ubuntu-only (`Category=Integration`, Docker gerekli)
- `docs/test-strategy.md` rehber dokuman

## Alternatifler Degerlendirildi

### Coverage tarafi
1. **Plan dosyasinin "kademe-1" hedefi (50/40):** Reddedildi — gercek %40.7
   line oldugu icin gate'i hemen kirardi. Test yazimi olmadan yukseltme
   kararname olur.
2. **Coverage gate'i tamamen kaldir:** Reddedildi — regression koruma kayboldu.
3. **`coverlet.runsettings` ile `Exclude`:** Reddedildi — `samples/` ve
   `tests/Kck.Benchmarks` test asseti referansi yok zaten, otomatik disinda.

### Benchmark tarafi
1. **`xunit-performance` veya `nbench`:** Reddedildi — BenchmarkDotNet
   industriyel standart, Microsoft .NET ekibi kullaniyor.
2. **Stryker.NET mutation testing once:** Reddedildi — daha agir, Bolum 7.4
   "Trial mode" olarak isaretli, BenchmarkDotNet'ten sonra ele alinacak.

### Integration test tarafi
1. **Docker Compose servis tanimlari:** Reddedildi — Testcontainers daha
   isolated (her test kendi container), CI ile yerel arasinda ayni davranis,
   parametrize edilebilir.
2. **`WebApplicationFactory` ile ASP.NET Core tarafi:** Disarida — bu PoC
   sadece Persistence katmanini hedefliyor.
3. **Tum 6 integration projesi tek seferde:** Reddedildi — kapsam patlar,
   PoC pattern'i kanitlanmadan yayilmaz. LS-FAZ-4.5'e ertelendi.
4. **PostgreSQL yerine SQL Server:** Reddedildi — SqlServer container'i
   (`mcr.microsoft.com/mssql/server:2022-latest`) Linux'ta calisiyor ancak
   PostgreSQL daha hizli baslar (~5s vs ~30s) ve EF Core supported.

## Sonuclar

**Olumlu:**
- LS-FAZ-5/6 performans calismasi BenchmarkDotNet baseline'i ile kanitli
  regresyon testi yapabilir.
- Coverage regression korumasi var; gercek-yukseltme acik politika ile.
- Integration test pattern PoC'lendi — diger 5 projeye yayilma yolu acik.
- CI integration step'i ubuntu-only — windows runner'da Docker bekleme yok.

**Olumsuz:**
- Yerel calistirma icin Docker Desktop gereksinimi (sadece integration
  testler icin, unit testler etkilenmiyor).
- Integration test step CI suresine ~30-60s ekler (postgres start + 2 test).
- Diger 5 integration test projesi hala mock-based. LS-FAZ-4.5 PoC'i yayar.
- BenchmarkDotNet sonuclarinda regression detection (bencher.dev / GitHub
  action) henuz yok.

## Yayilma Planı (LS-FAZ-4.5)

PoC pattern basariyla CI'da kostuktan sonra:

| Test Projesi | Container Image |
|---|---|
| `Kck.Caching.Redis.Tests` | `redis:7-alpine` |
| `Kck.Security.TokenBlacklist.Redis.Tests` | `redis:7-alpine` |
| `Kck.EventBus.RabbitMq.Tests` | `rabbitmq:3-management-alpine` |
| `Kck.EventBus.AzureServiceBus.Tests` | (yok — Azure'a baglanir, sandbox testi) |
| `Kck.Search.Elasticsearch.Tests` | `docker.elastic.co/elasticsearch/elasticsearch:8` |
| `Kck.Security.Secrets.AzureKeyVault.Tests` | (yok — Azurite veya mock) |

## Referanslar

- `tasks/library-strategy-2026-04-25.md` Bolum 5.7, 7.1, 7.5
- `tasks/library-strategy-faz4-test-benchmark-2026-04-26.md`
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- `docs/policies/test-coverage.md`
- `docs/test-strategy.md`
