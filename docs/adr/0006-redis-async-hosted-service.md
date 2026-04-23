# ADR-0006: Redis Connection — ConnectAsync + IHostedService

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

`Kck.Caching.Redis` iki anti-pattern barindiriyor:

1. **Blocking connect:**
   ```csharp
   services.AddSingleton<IConnectionMultiplexer>(sp =>
       ConnectionMultiplexer.Connect(redisOptions.Configuration));
   ```
   Ilk DI cozumlemesinde senkron blocking I/O yapilir. HTTP ilk istegi
   aniden yuzlerce ms gecikebilir.

2. **Sync-over-async:**
   ```csharp
   services.AddSingleton(sp =>
       Task.Run(() => redisOptions.ConnectionMultiplexerFactory())
           .GetAwaiter().GetResult());
   ```
   Thread pool starvation + potansiyel deadlock + hata maskeleme riski.

Audit raporu (2026-04-20, HIGH-6) bu iki kaydi isaretledi.

## Karar

Redis baglantisi artik `IHostedService` uzerinden async olarak kuruluyor.

Yeni bilesenler (`Kck.Caching.Redis.DependencyInjection`):

- **`RedisConnectionHolder`** (singleton): `IConnectionMultiplexer` tutucusu.
  Baglanti kurulmadan once `Multiplexer` getter'i `InvalidOperationException`
  atar. Uygulama shutdown'unda multiplexer'i dispose eder.
- **`RedisConnectionFactory`** (singleton): Async connector delegesi
  (`Func<CancellationToken, Task<IConnectionMultiplexer>>`) sarmalar.
- **`RedisConnectionHostedService`** (`IHostedService`): `StartAsync` icinde
  `factory.ConnectAsync()` cagirir, sonucu holder'a yerlestirir.

`AddKckCachingRedis` artik:
- Kullanici `ConnectionMultiplexerFactory` verdi ise onu async cagirir
- `Configuration` string veya `ConfigurationOptions` verdi ise
  `ConnectionMultiplexer.ConnectAsync(...)` kullanir
- Hicbiri yoksa `InvalidOperationException` — net hata mesaji
- `IConnectionMultiplexer` DI cozumlemesi holder'dan okur:
  `sp.GetRequiredService<RedisConnectionHolder>().Multiplexer`

## Alternatifler Degerlendirildi

- **Lazy<Task<IConnectionMultiplexer>>**: Tuketici await etmeli, `IConnectionMultiplexer`
  imzasi senkron. API kontrati bozulur. Reddedildi.
- **AsyncLazy<T>**: Ekstra paket bagimliligi; ayni amaci hosted service
  temiz cozuyor. Reddedildi.
- **Distributed cache'in kendi connection'ina guven**: `Microsoft.Extensions.Caching.StackExchangeRedis`
  kendi multiplexer'ini ayri kurar — bizim ayrica `IConnectionMultiplexer`
  tuketicilerimiz var (`RemoveByPrefixAsync` icin server iteration). Reddedildi.

## Sonuclar

**Olumlu:**
- Redis baglantisi startup sirasinda kurulur — ilk istek gecikmez
- Async yol: thread pool starvation yok, deadlock riski yok
- Baglanti hatasi startup'ta net sekilde yuzeye cikar (health check fail-fast)
- Dispose akisi tanimli (uygulama kapanirken multiplexer dogru kapanir)

**Olumsuz:**
- `AddKckCachingRedis` artik `IHostedService` kaydediyor — test host'unda
  genellikle sorun degil, ama pure unit test'te host kurmak gerekiyor
- `IConnectionMultiplexer` startup'tan once cozumlenirse `InvalidOperationException`
  atar — bu istenen davranis (fail-fast), ancak `IHostedService` oncesi cozum
  yapilan ozel durumlarda farkindalik gerektirir
- `Microsoft.Extensions.Caching.StackExchangeRedis`'in kendi connection'i hala
  ayri — tam birlestirme icin kullanici `RedisCacheOptions.ConnectionMultiplexerFactory`'i
  holder'imiza baglayabilir (ileriki bir iyilestirme)

## Dogrulama

- `dotnet build`: 0 hata
- `Kck.Caching.Redis.Tests` (7 test) yesil
- Tum test suite: 0 failure
