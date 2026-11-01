# Migration v4.x → v5.0

**Release date:** not released yet (this guide grows while the 5.0 work lands)
**Impact:** Breaking
**Estimated effort:** S (a configuration or broker change per RabbitMQ consumer)

## Why This Major?

5.0 collects the fixes that cannot be made without changing behaviour an application may rely on. The first one is
the RabbitMQ event bus: in 4.x a failing message was redelivered without limit, because the delivery count was read
from a header that RabbitMQ never sets on a requeued message, and the queues had no dead-letter exchange, so the
"dead-lettered" message would have been dropped. Fixing it means declaring the consumer queues with a dead-letter
argument, and RabbitMQ refuses to re-declare an existing queue with other arguments.

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

### Other 5.0 changes

Other 5.0 changes are added to this guide as the 5.0 work lands.

## Automated Migration

None: the changes are broker topology and configuration, not API renames.

## Manual Steps

1. For every RabbitMQ consumer, choose option C, B or A above before the first 5.0 deployment.
2. If you relied on the consumer taking the whole queue at once, set `PrefetchCount = 0` or a larger value.
3. If a liveness probe uses the `rabbitmq-eventbus` check, move it to readiness.
4. If a MailKit server expects implicit TLS on a port other than 465, set `SecureSocketOptions = SslOnConnect`.
5. Where an FTP upload may fail or hit an existing file with `Overwrite = false`, catch `IOException`.
6. With the hybrid cache, move `ICacheService` out of hosted-service constructors (section 6).
7. Where an Elasticsearch call relied on a silent failure, catch `InvalidOperationException` (section 7).

## References

- CHANGELOG.md `[Unreleased]`
- [Event bus guide](../providers/event-bus.md)
- `docs/policies/versioning.md`
