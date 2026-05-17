# Frequently Asked Questions

## Setup & Registration

### Hangi paketi yüklemeliyim?

Hızlı başlangıç için `Kck.Bundle.WebApi` veya `Kck.Bundle.MinimalApi` ekleyin — tek pakette
tüm temel provider'lar gelir. Yalnızca belirli modüllere ihtiyaç duyuyorsanız tekil provider
paketlerini (`Kck.Caching.Redis`, `Kck.EventBus.RabbitMq`, vb.) tercih edin.

### "No service for type 'ICacheService' has been registered" hatası

`AddKckCaching()` veya bundle'da caching bölümünü atlamışsınızdır. `Program.cs` içinde:

```csharp
builder.Services.AddKckCaching(opts => opts.KeyPrefix = "myapp:");
builder.Services.AddKckInMemoryCache();  // ya da AddKckRedisCache(...)
```

### AddKck* çağrılarının sırası önemli mi?

`AddKckCaching()` → `AddKckRedisCache()` gibi önce abstraction, sonra provider. Ancak
MediatR/Mediator pipeline'ı davranış sırasına dikkat edin: `AddKckPipeline()` builder
üzerinde `.AddBehavior<>()` çağrılarının sırasıyla çalışır.

---

## Entity & Persistence

### Entity<TId> soyut sınıftan türeyen sınıfımda parametre constructor kullanamıyorum

Entity `protected Entity(TId id)` constructor'a sahip. Türetilen sınıfta bunu çağırın:

```csharp
public class Product : Entity<Guid>
{
    private Product() { }
    public Product(Guid id, string name) : base(id) { Name = name; }
    public string Name { get; private set; } = string.Empty;
}
```

### EF Core migration'da "The entity type requires a primary key" hatası

EF Core, `Entity<TId>.Id`'yi otomatik olarak PK olarak tanımaz. `OnModelCreating` içinde
ya da fluent konfigürasyonda belirtin:

```csharp
modelBuilder.Entity<Product>().HasKey(p => p.Id);
```

`IEntityTypeConfiguration<T>` ile ayrı dosyada yapılandırmanız önerilir.

### SoftDelete filtresi otomatik uygulanıyor mu?

Hayır, `EfRepository` varsayılan sorgularına global filtre eklenmez. `DbContext`'inizde kendiniz
`HasQueryFilter(e => !e.IsDeleted)` eklemeniz gerekir. Bu kasıtlı bir tasarım kararıdır —
admin paneli gibi silinen kayıtları gösteren ekranlar için filtre gerekmiyor olabilir.

### Paginate<T> ile IQueryable kullanımı

```csharp
// DOĞRU — önce Where/OrderBy, sonra ToPaginateAsync
var result = await _repo.GetListAsync(
    predicate: p => p.IsActive,
    orderBy: q => q.OrderBy(p => p.Name),
    index: 0, size: 20, cancellationToken: ct);
```

`ToPaginateAsync` extension metodu, `totalCount` için ayrı `COUNT(*)` sorgusu çalıştırır.
Büyük tablolarda performans için `QueryOptions` ile gerekli alanları `Select` edin.

---

## Caching

### Redis bağlantısı "Connection refused" hatası

`RedisConnectionHostedService`, uygulama başlarken bağlanır. Bağlantı başarısız olursa
uygulama ayağa kalkar ama `ICacheService` çağrıları `InvalidOperationException` fırlatır.

Kontrol listesi:
1. `CacheOptions.RedisConnectionString` doğru mu?
2. Redis sunucusu erişilebilir mi? (`redis-cli ping`)
3. `ASPNETCORE_ENVIRONMENT=Development` ise `appsettings.Development.json`'a bakın.

### InMemory cache ile Redis cache farkı nedir?

| Özellik | InMemory | Redis | HybridCache |
|---|---|---|---|
| Dağıtık | Hayır | Evet | Evet (L2) |
| Hız | En hızlı | Ağ gecikmesi | L1 cache hit = InMemory hızı |
| Stampede koruması | CacheServiceBase (64 stripe) | CacheServiceBase (64 stripe) | HybridCache yerleşik |
| Multi-instance | Hayır | Evet | Evet |

### KeyPrefix ne için kullanılır?

Aynı Redis instance'ını birden fazla uygulama veya ortam (staging/production) paylaşıyorsa
key çakışmalarını önler. Öneri: `"{appname}:{env}:"` formatı.

---

## Event Bus

### IntegrationEvent'im yayınlandı ama handler çalışmadı

1. `Subscribe<TEvent, THandler>()` çağrıldı mı? DI container handler'ı kayıt ediyor ama
   subscribe ayrıca yapılmalı (genellikle startup'da).
2. Handler, DI'da kayıtlı mı? `AddKckEventBus()` builder üzerinden handler'ları kaydedin.
3. RabbitMQ/Azure kullanıyorsanız exchange/queue adlarının eşleştiğini doğrulayın.

### RabbitMQ bağlantısı kesilince mesajlar kaybolur mu?

`InMemoryEventBus` fire-and-forget'tir — kalıcılık yok. `RabbitMqEventBus` kullanıyorsanız
durability için exchange ve queue'yu `durable: true` ile tanımladığınızdan emin olun.
Kritik iş süreçleri için Outbox pattern entegrasyonunu değerlendirin (bakınız: Faz E listesi).

---

## Pipeline (MediatR / Mediator)

### MediatR mı, Mediator mi?

`Kck.Pipeline.MediatR` kullanımdan kaldırılmıştır (ADR-0021). Yeni projelerde
`Kck.Pipeline.Mediator` (Mediator.Abstractions 3.x) kullanın. Migrasyon kılavuzu:
[docs/migrations/mediatR-to-mediator.md](migrations/mediatR-to-mediator.md).

### Behavior sırasını nasıl belirlerim?

`AddKckPipeline()` sonrasında `.AddBehavior<>()` çağrılarının sırası pipeline sırasını belirler:

```csharp
builder.AddKckPipeline()
    .AddBehavior<LoggingBehavior>()      // ilk çalışır
    .AddBehavior<ValidationBehavior>()   // ikinci
    .AddBehavior<CachingBehavior>();     // üçüncü
```

---

## Security

### JWT token "IDX10223: Lifetime validation failed" hatası

Sunucu saati ile token oluşturma zamanı arasında fark var. `ClockSkew` toleransını kontrol edin:

```csharp
options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);
```

### Argon2 hash doğrulaması yavaş

Bu beklenen davranıştır — Argon2 kasıtlı olarak yavaştır (DoS karşıtı tasarım).
`Argon2Options.Iterations` ve `MemorySize` değerlerini ortamınıza göre ayarlayın.
Minimum önerilen: `Iterations=3, MemorySize=65536`.

---

## CI/CD & Build

### NU1004 — lock file package version mismatch

Tipik neden: farklı SDK versiyonu veya Directory.Packages.props değişikliği.

```bash
dotnet restore --force-evaluate  # lock dosyalarını yeniden oluşturur
git add "**packages.lock.json"
```

SDK versiyonu sabitleyin (global.json). Detay: [ADR-0011](adr/0011-multi-target-net8-net10.md).

### CS0433 — Azure.Core çakışması

`Azure.Identity >= 1.21.0` ve `Azure.Core >= 1.54.0` birlikte kullanıldığında oluşabilir.
`Directory.Packages.props` içinde `Azure.Identity 1.21.0+`'a yükseltin. Detay:
[MEMORY: Azure CS0433](https://github.com/omerkck41/OmerkckArchitecture/blob/main/SECURITY.md).

### "Coverage threshold not met" CI hatası

Lokal olarak coverage ölçün:

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:TextSummary
cat coverage-report/Summary.txt
```

Eşikler: `Line ≥50%, Branch ≥45%`. Detay: [docs/policies/test-coverage.md](policies/test-coverage.md).

---

## Versioning & NuGet

### Yeni versiyon tag'lemeyi unuttum, NuGet'te eski versiyon görünüyor

MinVer tag'e göre versiyon hesaplar. Tag yoksa `-preview.N` suffix ekler. Publish için:

```bash
git tag v2.1.0
git push origin v2.1.0   # CI otomatik NuGet push yapar
```

### Paket readme'si NuGet.org'da görünmüyor

`Directory.Build.props` içinde `<PackageReadmeFile>README.md</PackageReadmeFile>` tanımlı olmalı
(Faz C'den itibaren eklenmiştir). Eski sürümler için paket yeniden yayınlanmalıdır.
