# Migration v4.x → v5.0

**Release date:** not released yet (this guide grows while the 5.0 work lands)
**Impact:** Breaking
**Estimated effort:** S (a configuration or broker change per RabbitMQ consumer; a package and registration rename for
image processing)

## Why This Major?

5.0 collects the fixes that cannot be made without changing behaviour an application may rely on. The first one is
the RabbitMQ event bus: in 4.x a failing message was redelivered without limit, because the delivery count was read
from a header that RabbitMQ never sets on a requeued message, and the queues had no dead-letter exchange, so the
"dead-lettered" message would have been dropped. Fixing it means declaring the consumer queues with a dead-letter
argument, and RabbitMQ refuses to re-declare an existing queue with other arguments.

5.0 also makes the default packages depend only on permissively licensed libraries. Image processing moves from
SixLabors.ImageSharp to SkiaSharp (MIT): ImageSharp 4 is published under the Six Labors Split License 1.0, which is not
an OSI-approved licence, and asks for a licence key at Release build time (decision
`decisions/2026-10-31-image-library-replacement.md`).

## Affected Packages

| Package | Change type | Impact level |
|---|---|---|
| `Kck.EventBus.RabbitMq` | Consumer queue arguments (dead-letter exchange) | High |
| `Kck.EventBus.RabbitMq` | Consumer prefetch default 10 | Low |
| `Kck.EventBus.RabbitMq` | Health check reports whether the bus is ready to consume | Low |
| `Kck.EventBus.RabbitMq` | `MaxDeliveryCount` below 1 is rejected at registration | Low |
| `Kck.Messaging.MailKit` | TLS mode default | High |
| `Kck.FileStorage.FluentFtp` | UploadAsync throws on failure/skip | Medium |
| `Kck.Caching.Hybrid` | `ICacheService` resolvable only after the host started | High |
| `Kck.Search.Elasticsearch` | Rejected requests throw instead of returning silently | Medium |
| `Kck.Documents.ImageSharp` | **Removed** — replaced by `Kck.Documents.SkiaSharp` | High |
| `Kck.Exceptions.AspNetCore` | `CustomException.AdditionalData` is now sent to the client for every `CustomException`, including those mapped to a 5xx status: data meant only for logs must not be put there | High |
| `Kck.Authorization.Abstractions` | Removed API: `IAuthorizationService`, `IPermissionChecker` | Medium |
| `Kck.BackgroundJobs.Abstractions` | Removed API: `IRecurringJob` | Medium |
| `Kck.Exceptions.Abstractions` | Removed API: `FilterSecurityException` | Medium |
| `Kck.FeatureFlags.Abstractions` | Removed API: `IFeatureEvaluator`, `IFeatureChangeNotifier`, `FeatureChangeEvent` | Medium |
| `Kck.FileStorage.Abstractions` | Removed API: `IFileConverter`, `IFileMetadataReader`, `PathHelper.IsValidPath` / `Sanitize` | Medium |
| `Kck.Http.Abstractions` | Removed API: `ResilienceOptions.CircuitBreakerFailureThreshold` | High |
| `Kck.Messaging.Abstractions` | Removed API: `IEmailService` | Medium |
| `Kck.Observability.Abstractions` | Removed API: `IHealthCheckRegistrar`, `HealthCheckType`, the `tags` parameter of `IMetricsService.CreateCounter` | Medium |
| `Kck.Persistence.Abstractions` | Removed API: bool-flag `IQuery<T>.Query` overload, `FilterValidator` | Medium |
| `Kck.Persistence.EntityFramework` | Removed API: bool-flag `EfRepository.Query` overload | Medium |
| `Kck.Pipeline.Abstractions` | Removed API: `ICachableRequest.CacheGroupKey` | Medium |
| `Kck.Security.Abstractions` | Removed API: `IEncryptionService` | Medium |
| `Kck.Security.Jwt` | Removed API: `RsaKeySource.SecretsManager` | High |
| `Kck.BackgroundJobs.Hangfire` | Removed API: `HangfireOptions.ConnectionString` | High |
| `Kck.Bundle.WebApi` | Removed API: never-read `Persistence`, `Caching`, `Observability`, `Hashing`, `Algorithm` settings | High |
| `Kck.Bundle.MinimalApi` | Removed API: never-read `Security:Jwt:Algorithm` setting | High |
| `Kck.Bundle.WorkerService` | Removed API: never-read `Observability` setting | High |

(Impact: Low = interface implementations only, Medium = call sites,
High = configuration / DI level)

## At a Glance (TL;DR)

- RabbitMQ consumer queues gain a per-service dead-letter exchange: queues created by 4.x fail with 406 at start-up
- A handler's `OperationCanceledException` (for example an `HttpClient` timeout) now counts as a failed delivery
- Consumer prefetch is now 10 by default
- The RabbitMQ health check no longer opens its own connection and reports Unhealthy when the consumers are stopped
- MailKit derives the TLS mode from the port: `UseSsl = true` on 587 is now STARTTLS; implicit TLS on a custom port
  needs `SecureSocketOptions = SslOnConnect`
- FTP `UploadAsync` throws `IOException` when the server reports a failed upload or when `Overwrite = false` and the
  file exists; 4.x returned silently
- Hybrid cache: resolving `ICacheService` before the host started throws
- Elasticsearch write and search operations throw `InvalidOperationException` when Elasticsearch rejects the request
- `Kck.Documents.ImageSharp` → `Kck.Documents.SkiaSharp`; `AddKckDocumentsImageSharp()` → `AddKckDocumentsSkiaSharp()`
- The 4.x `[Obsolete]` members (KCK0002, KCK0101, KCK0300, KCK0600, KCK0700, KCK0900, KCK0901) and the contracts no
  package implemented or called are removed (section 9). 4.x code that built without an obsolete warning is unaffected
  by the first group, except an own `IQuery<T>`/`IRepository` implementation: it must now implement
  `Query(QueryOptions, …)` (implementing the obsolete overload never warned)

## Detailed Changes

### 1. RabbitMQ consumer queues gain a dead-letter exchange (406 on existing queues)

**Reason:** with `DeadLetterEnabled = true` (the default) the consumer declares a fanout dead-letter exchange
(`{QueuePrefix}.dlx`, one per service, so services sharing `ExchangeName` keep their rejected messages apart) and a
durable dead-letter queue (`{QueuePrefix}.dlq`), and declares every consumer queue `{QueuePrefix}.{EventName}` with
the argument `x-dead-letter-exchange`. After `MaxDeliveryCount` deliveries in total, the first one included, a failing
message is rejected without requeue and the broker moves it to the dead-letter queue.

A queue created by 4.x has no arguments. RabbitMQ answers the re-declaration with `406 PRECONDITION_FAILED`
(inequivalent arg), and 5.0 fails at start-up with an `InvalidOperationException` that names the queue: the consumer
hosted service throws and the host stops (the default `BackgroundServiceExceptionBehavior.StopHost`), so no message is
consumed with the wrong settings. With `BackgroundServiceExceptionBehavior.Ignore` the host stays up without
consumers and the `rabbitmq-eventbus` health check reports Unhealthy.

The remedies below are ordered by risk: C and B lose no message, A can.

**Before (v4):**
```csharp
services.AddKckEventBus(b => b.UseRabbitMq(opt =>
{
    opt.HostName = builder.Configuration["RabbitMq:Host"]!;
    opt.UserName = builder.Configuration["RabbitMq:User"]!;
    opt.Password = builder.Configuration["RabbitMq:Password"]!;
    opt.MaxDeliveryCount = 5;
}));
```

**After (v5), option C — new queue names (no message loss):** set a new `QueuePrefix`. The new queues are declared
with the dead-letter argument and bound to the exchange, so they receive every message published from then on. Keep
the 4.x consumers running on the old queues until those are empty, then stop them and delete the old queues (management
UI or `rabbitmqctl delete_queue <queue>`).

**After (v5), option B — keep the queues and set the dead-letter exchange through a policy:**
```csharp
services.AddKckEventBus(b => b.UseRabbitMq(opt =>
{
    opt.HostName = builder.Configuration["RabbitMq:Host"]!;
    opt.UserName = builder.Configuration["RabbitMq:User"]!;
    opt.Password = builder.Configuration["RabbitMq:Password"]!;
    opt.MaxDeliveryCount = 5;
    opt.DeadLetterEnabled = false; // queues are declared without arguments, as in 4.x
}));
```

With `DeadLetterEnabled = false` Kck declares neither the dead-letter exchange nor the queue, and a rejected message
is dropped unless a policy sets a dead-letter exchange. Declare the exchange and a queue bound to it yourself, then
apply the policy to the consumer queues:

```bash
rabbitmqctl set_policy kck-dlx "^kck\.(?!dlq$)" '{"dead-letter-exchange":"kck.dlx"}' --apply-to queues
```

The policy pattern must not match the dead-letter queue itself (here `kck.dlq`): a dead-letter queue that dead-letters
into its own exchange sends a rejected or expired message around in a loop. Adjust the pattern to your queue names and
check the matched queues in the management UI.

**After (v5), option A — let Kck re-declare the queues:** the code stays the same. Before the first 5.0 deployment,
drain each consumer queue (let the 4.x consumers empty it), then delete it in the management UI or with
`rabbitmqctl delete_queue <queue>`. Messages still in a deleted queue are lost, and messages published between the
deletion and the first 5.0 start-up are dropped by the exchange (no bound queue), so stop the publishers first or
prefer C. 5.0 re-declares the queue with the dead-letter argument.

**Delivery counting.** `MaxDeliveryCount` now counts every delivery, the first one included (`MaxDeliveryCount = 3`
means three handler invocations), and must be at least 1 (`UseRabbitMq` throws `ArgumentOutOfRangeException`
otherwise). The deliveries are counted in the consuming process, keyed by `MessageId`, so `MessageId` must be unique
per message; Kck sets it to `IntegrationEvent.Id`. A message published by other code without a `MessageId` is
delivered at most twice: the broker's Redelivered flag counts as a previous delivery. The in-process count restarts
when the consumer restarts or another instance receives the message.

**Redelivery is immediate.** There is no back-off between deliveries: the `MaxDeliveryCount` attempts happen within
milliseconds, so a transient outage of a dependency can dead-letter every message delivered during it. Monitor the
dead-letter queue; to redrive, inspect its messages in the management UI, fix the cause, and move them back to the
consumer queue with a RabbitMQ shovel (the `rabbitmq_shovel` plugin; define it in the management UI or with
`rabbitmqctl set_parameter shovel`) or by republishing them.

**Handler cancellations.** A handler's `OperationCanceledException`, for example an `HttpClient` timeout, is a failed
delivery: the message is negatively acknowledged and counted. In 4.x it stayed unacknowledged until the channel
closed; with the 5.0 prefetch limit such messages would stall the consumer. Only host shutdown leaves the message
unacknowledged for the broker to redeliver.

### 2. Consumer prefetch is now 10 by default

**Reason:** in 4.x the consumer channel had no `basic.qos`, so the broker pushed every ready message into the
consuming process. `RabbitMqOptions.PrefetchCount` (default `10`) now limits the unacknowledged messages per consumer
channel. To keep the 4.x behaviour:

```csharp
services.AddKckEventBus(b => b.UseRabbitMq(opt =>
{
    opt.HostName = builder.Configuration["RabbitMq:Host"]!;
    opt.PrefetchCount = 0; // unlimited, as in 4.x
}));
```

### 3. RabbitMQ health check reports whether the bus is ready to consume

**Reason:** the 4.x check opened and closed a new connection on every probe. The `rabbitmq-eventbus` check (tag
`ready`) now reports whether the event bus is ready to consume: its own connection is open and, when events are
subscribed, its consumer channel is open. It is Unhealthy until the host has started the consumers and while the
connection recovers, so use it as a readiness check, not as a start-up liveness check. It also stays Unhealthy when a
channel-level error (for example a `consumer_timeout` or a 406) closed the consumer channel, which the client does not
reopen: the consumers are stopped, restart the host.

### 4. MailKit TLS mode is derived from the port

**Reason:** in 4.x `UseSsl = true` (the default) always meant implicit TLS, also on 587, the STARTTLS submission port
and the default `Port`. Standard SMTP servers expect a plain-text greeting there, so the TLS handshake failed and the
default options could not send. 5.0 adds `MailKitOptions.SecureSocketOptions`; while it is `null`, the mode is derived:
with `UseSsl = true` implicit TLS on 465 and mandatory STARTTLS on every other port (TLS stays required, no plaintext
fallback); with `UseSsl = false` opportunistic STARTTLS, used only when the server offers it.

**Before (v4):**
```csharp
services.AddKckMessagingMailKit(opt =>
{
    opt.Host = "smtp.example.com";
    opt.Port = 587;
    opt.UseSsl = true; // implicit TLS on the STARTTLS port: the handshake failed
});
```

**After (v5):** the same options connect with STARTTLS, so no change is needed on 587 or 465. A server that expects
implicit TLS on another port needs the mode set explicitly:

```csharp
using MailKit.Security;

services.AddKckMessagingMailKit(opt =>
{
    opt.Host = "smtp.example.com";
    opt.Port = 2465;
    opt.SecureSocketOptions = SecureSocketOptions.SslOnConnect; // implicit TLS on a custom port
});
```

### 5. FTP upload failures throw

**Reason:** in 4.x `Kck.FileStorage.FluentFtp`'s `UploadAsync` returned normally when the server reported a failed
upload (only a warning was logged) and when `Overwrite = false` and the remote file already existed (the upload was
skipped silently), so the caller could not tell that nothing was stored. 5.0 throws `IOException` in both cases; the
message names the remote path, and a skipped upload's message names `Overwrite`.

**Before (v4):**
```csharp
using Kck.FileStorage.Abstractions;

public class BackupJob(IFtpService ftp)
{
    // Returned normally even when the server rejected the upload or "daily.bak" already existed.
    public Task RunAsync(Stream data, CancellationToken ct) =>
        ftp.UploadAsync(new FileUploadRequest
        {
            Content = data,
            FileName = "daily.bak",
            DestinationPath = "backups",
            Overwrite = false
        }, ct);
}
```

**After (v5):** catch `IOException` where a failed or skipped upload is an expected outcome:

```csharp
using Kck.FileStorage.Abstractions;

public class BackupJob(IFtpService ftp)
{
    public async Task<bool> RunAsync(Stream data, CancellationToken ct)
    {
        try
        {
            await ftp.UploadAsync(new FileUploadRequest
            {
                Content = data,
                FileName = "daily.bak",
                DestinationPath = "backups",
                Overwrite = false
            }, ct);
            return true;
        }
        catch (IOException)
        {
            return false; // the server failed the upload, or the file exists and Overwrite is false
        }
    }
}
```

`IFtpService` and `IFileStorageService` are also registered as singletons now (4.x: transient); both resolve to the
same instance. They are registered with `TryAddSingleton`, so the first registration wins: when another storage
provider registered `IFileStorageService` first, it no longer resolves to FTP (in 4.x the last registration won).
Register FTP first, or resolve `IFtpService` where you need FTP.

### 6. Hybrid cache: `ICacheService` is available only after the host started

**Reason:** in 4.x `Kck.Caching.Hybrid` opened the Redis connection inside DI resolution with a blocking `Connect`.
5.0 opens it asynchronously in a hosted service when the host starts (the Kck.Caching.Redis pattern), so resolving
`ICacheService` or `IConnectionMultiplexer` before the host's `StartAsync` completed throws
`InvalidOperationException`. A hosted service that takes `ICacheService` in its constructor is created before any
hosted service has started, so it must resolve the cache in `StartAsync`/`ExecuteAsync` instead.

**Before (v4):**
```csharp
public class CacheWarmUp(ICacheService cache) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        cache.SetAsync("catalog:version", "1", TimeSpan.FromHours(1), stoppingToken);
}
```

**After (v5):** resolve the cache when the service runs, and register the hosted service after `AddKckCachingHybrid`
(hosted services start in registration order):

```csharp
public class CacheWarmUp(IServiceProvider services) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cache = services.GetRequiredService<ICacheService>();
        return cache.SetAsync("catalog:version", "1", TimeSpan.FromHours(1), stoppingToken);
    }
}
```

A container built without a host (a console tool, a test) must start the registered `IHostedService`s before it
resolves `ICacheService`.

### 7. Elasticsearch: rejected requests throw

**Reason:** in 4.x `Kck.Search.Elasticsearch` ignored most error responses: a rejected write (an invalid index name,
an update of a missing document, a bulk with failed items) returned normally, and `SearchAsync` returned an empty
result for a missing index or a query syntax error. 5.0 throws `InvalidOperationException` when Elasticsearch rejects
a write or search request; the message names the operation, the index, the HTTP status and the server's error type
and reason. Deletes of a missing document or index stay silent, `GetByIdAsync` still returns `null` for a missing
document and `IndexExistsAsync` still returns `false` for a missing index. Wrap the calls that relied on the silent
failure, for example a search over an index that may not exist yet, in a `try`/`catch (InvalidOperationException)`.

### 8. Image processing: ImageSharp → SkiaSharp

**Reason:** licence constraint of ImageSharp 4 (see above). `Kck.Documents.ImageSharp` is removed in 5.0 without an
intermediate `[Obsolete]` release; `Kck.Documents.SkiaSharp` is available before 5.0, so you can switch early.

**Package:**

```xml
<!-- Before (4.x) -->
<PackageReference Include="Kck.Documents.ImageSharp" />

<!-- After -->
<PackageReference Include="Kck.Documents.SkiaSharp" />
<!-- Linux hosts (glibc and musl/Alpine) also need the native library: -->
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" />
```

**Registration:**

```csharp
// Before (4.x): services.AddKckDocumentsImageSharp();
services.AddKckDocumentsSkiaSharp();
```

`IImageProcessor` and `ImageProcessingOptions` are unchanged; code that injects `IImageProcessor` needs no change.

**Behaviour differences:**

| | ImageSharp (4.x) | SkiaSharp (5.0) |
|---|---|---|
| Output formats | jpeg/jpg, png, webp, gif, bmp | jpeg/jpg, png, webp — `gif` and `bmp` throw `NotSupportedException` |
| Input formats | JPEG, PNG, WebP, GIF, BMP, TIFF, ... | JPEG, PNG, WebP, GIF, BMP, ICO — TIFF throws `InvalidDataException` |
| Empty output format with a GIF/BMP source | GIF/BMP | JPEG |
| Transparent image → JPEG | encoder default | transparent areas become white |
| Image size limit | none | `SkiaSharpOptions.MaxPixels` (default 50,000,000) for the source and the resize target, checked from the header before decoding |
| `GetDimensionsAsync` | decodes the image; returns the stored size (EXIF orientation not applied) | reads the header only; returns the displayed size (orientation applied) |
| Native library | none (fully managed) | Windows/macOS included; Linux needs `SkiaSharp.NativeAssets.Linux.NoDependencies` |
| EXIF orientation | not applied | applied (baked into the pixels) |
| Metadata (EXIF incl. GPS, ICC profile) | preserved in the output | not written; pixels converted to sRGB |
| `Width` / `Height` `null` or 0 | derived from the source ratio | derived from the source ratio (no difference); negative throws `ArgumentOutOfRangeException` |
| `Quality` outside 1–100 | throws | clamped to 1–100 |

### 9. Removed API

**Reason:** 4.x shipped members that did nothing (settings that were never read, an enum value that was never
implemented) and contracts that no Kck package implemented or called. The first group was marked `[Obsolete]` in 4.3–4.8
and is removed on schedule ([deprecation policy](../policies/deprecation.md#removed-in-50)); the second group is removed
without an `[Obsolete]` release first, because nothing in Kck ever used it. A removal is a compile error, not a
behaviour change: if your code compiles against 5.0, it behaves as before. One exception to "compile error": an
implicitly implemented `CacheGroupKey` property on your request type still compiles and simply becomes dead code (only
an explicit interface implementation fails), so search for it rather than waiting for an error.

| Symbol | Package | Replacement |
|---|---|---|
| `IQuery<T>.Query(filter, orderBy, bool withDeleted, bool enableTracking)` and the same `EfRepository` overload (KCK0101) | `Kck.Persistence.Abstractions`, `Kck.Persistence.EntityFramework` | `Query(QueryOptions options, filter, orderBy)` with `QueryOptions.None` / `.Tracking` / `.WithDeleted`; an own `IQuery<T>` implementation must now implement this member (the default that forwarded to the bool overload is gone) |
| `KckWebApiDefaults.Persistence`, `.Caching`, `.Observability` (+ `PersistenceDefaults`, `CachingDefaults`, `ObservabilityDefaults`), `SecurityDefaults.Hashing`, `JwtDefaults.Algorithm`; MinimalApi `JwtDefaults.Algorithm`; `KckWorkerServiceDefaults.Observability` (+ `ObservabilityDefaults`) (KCK0002) | `Kck.Bundle.WebApi`, `Kck.Bundle.MinimalApi`, `Kck.Bundle.WorkerService` | Configure those providers in code (`AddKckPersistence`, a caching provider, `AddKckObservability`). The bundles always hash with Argon2 and sign with RS256; leftover `Kck:` keys are ignored |
| `RsaKeySource.SecretsManager` (KCK0300) | `Kck.Security.Jwt` | Read the key yourself (for example from `ISecretsManager`) and set `RsaKeyBase64` with `RsaKeySource.Configuration`. A configuration value `SecretsManager` now fails to bind, and a number that is not a defined value (for example `"KeySource": "2"`) fails options validation at start-up |
| `HangfireOptions.ConnectionString` (KCK0700) | `Kck.BackgroundJobs.Hangfire` | `HangfireOptions.ConfigureStorage`, for example `c => c.UseSqlServerStorage(connectionString)` |
| `ResilienceOptions.CircuitBreakerFailureThreshold` (KCK0900) | `Kck.Http.Abstractions` | `CircuitBreakerFailureRatio` / `CircuitBreakerMinimumThroughput` |
| `IEmailService` (KCK0600) | `Kck.Messaging.Abstractions` | `IEmailProvider` (same `SendAsync`) |
| `IFeatureEvaluator` (KCK0901) | `Kck.FeatureFlags.Abstractions` | `IFeatureFlagService.IsEnabledAsync(name, context)` |
| `IEncryptionService` | `Kck.Security.Abstractions` | none — it had no implementation; keep your own copy if you implemented it |
| `IAuthorizationService`, `IPermissionChecker` | `Kck.Authorization.Abstractions` | none — they had no implementation; keep your own copy if you implemented them (`ICurrentUserProvider` stays) |
| `IFeatureChangeNotifier`, `FeatureChangeEvent` | `Kck.FeatureFlags.Abstractions` | none — it had no implementation; keep your own copy if you implemented it |
| `IFileConverter`, `IFileMetadataReader` | `Kck.FileStorage.Abstractions` | none — they had no implementation; keep your own copy if you implemented them |
| `PathHelper.IsValidPath`, `PathHelper.Sanitize` | `Kck.FileStorage.Abstractions` | `PathHelper.Validate(path)` (throws `ArgumentException` instead of returning `false`; the providers already call it). The rules differ slightly: `Validate` does not reject `~`, and it also rejects `%`, control characters and a leading `/`. `Sanitize` silently rewrote a path; reject it instead |
| `IHealthCheckRegistrar`, `HealthCheckType` | `Kck.Observability.Abstractions` | none — it had no implementation and the enum was only its `AddDependency` parameter; keep your own copy if you used them (`IHealthCheck` stays) |
| `tags` parameter of `IMetricsService.CreateCounter(name, description, tags)` | `Kck.Observability.Abstractions` | Pass tags on each `Increment` call (`counter.Increment(1, new KeyValuePair<string, object?>("tenant", id))`). The parameter took tag keys without values, which OpenTelemetry cannot represent, and the provider ignored it. A call with only `name` and `description` compiles unchanged; an own `IMetricsService` implementation drops the parameter |
| `IRecurringJob` | `Kck.BackgroundJobs.Abstractions` | Implement `IBackgroundJob` and pass the job id and cron expression to `IJobScheduler.AddOrUpdateRecurringAsync<TJob>(jobId, cron)` |
| `FilterSecurityException` | `Kck.Exceptions.Abstractions` | none — nothing threw it (a field outside the dynamic filter whitelist throws `ArgumentException`, as in 4.x); keep your own copy if you threw it |
| `FilterValidator` | `Kck.Persistence.Abstractions` | none — nothing called it; `ToDynamic` throws `ArgumentException` for an unsupported operator or an invalid value |
| `ICachableRequest.CacheGroupKey` | `Kck.Pipeline.Abstractions` | none — the caching behavior never read it; delete the property from your requests |

### Other 5.0 changes

Behaviour fixes that need no code change, but that a client or a caller may notice:

- `Kck.Exceptions.AspNetCore` — a `CustomException`'s `AdditionalData` is now written into the problem body as
  extension members (for example `"paramName": "email"` for `CustomArgumentException`); 4.x dropped it. This applies
  to every `CustomException`, including one mapped to a 5xx status, so search your code for `additionalData` arguments
  and move anything meant only for logs (ids, SQL, stack details) out of them
  ([exceptions](../providers/exceptions.md#additional-data)).
- `Kck.Documents.ClosedXml` — `ReadAsync` returns rows of equal width with `null` for empty cells; 4.x skipped empty
  cells, so values after a gap moved left. Code that compensated for the shift must drop that.
- `Kck.Observability.OpenTelemetry` — `CreateHistogram` applies its `buckets`, and a second `Create*` call with the
  same name returns the first instrument instead of registering another one. Because 4.x ignored `buckets`, unsorted,
  duplicate or non-finite boundaries never failed; 5.0 throws `ArgumentException` for them. Pass strictly increasing,
  finite values.
- `Kck.Persistence.EntityFramework` — `CreatedBy` / `ModifiedBy` / `DeletedBy` are filled when you register the new
  optional `IAuditUserProvider` ([persistence](../providers/persistence.md#audit-user-50)); without it nothing changes.

Other 5.0 changes are added to this guide as the 5.0 work lands.

## Automated Migration

None: the changes are broker topology and configuration, plus one package and registration rename (section 8) that a
search-and-replace covers.

## Manual Steps

1. For every RabbitMQ consumer, choose option C, B or A above before the first 5.0 deployment.
2. If you relied on the consumer taking the whole queue at once, set `PrefetchCount = 0` or a larger value.
3. If a liveness probe uses the `rabbitmq-eventbus` check, move it to readiness.
4. If a MailKit server expects implicit TLS on a port other than 465, set `SecureSocketOptions = SslOnConnect`.
5. Where an FTP upload may fail or hit an existing file with `Overwrite = false`, catch `IOException`.
6. With the hybrid cache, move `ICacheService` out of hosted-service constructors (section 6).
7. Where an Elasticsearch call relied on a silent failure, catch `InvalidOperationException` (section 7).
8. Replace the `Kck.Documents.ImageSharp` package reference with `Kck.Documents.SkiaSharp` and
   `AddKckDocumentsImageSharp()` with `AddKckDocumentsSkiaSharp()`; on Linux add
   `SkiaSharp.NativeAssets.Linux.NoDependencies` to the application.
9. Search for `"gif"` and `"bmp"` output formats (`OutputFormat`, `ConvertFormatAsync`) and switch them to `png` or
   `webp`; if you accept TIFF uploads, convert them before they reach `IImageProcessor`.
10. If you process images above 50 megapixels, raise `SkiaSharpOptions.MaxPixels`.
11. Build against 5.0 and fix each error from section 9: switch to the replacement in the table, delete
    `CacheGroupKey` (and other removed members) from your types, and copy a removed contract into your code if you
    implemented it. Remove leftover `KCK0xxx` entries from `NoWarn` and `#pragma warning disable`.

## References

- CHANGELOG.md `[Unreleased]`
- [Event bus guide](../providers/event-bus.md)
- [Documents guide](../providers/documents.md)
- `decisions/2026-10-31-image-library-replacement.md`
- `docs/policies/versioning.md`
- [`docs/policies/deprecation.md`](../policies/deprecation.md)
