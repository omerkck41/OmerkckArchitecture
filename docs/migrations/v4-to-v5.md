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

(Impact: Low = interface implementations only, Medium = call sites,
High = configuration / DI level)

## At a Glance (TL;DR)

- RabbitMQ consumer queues gain a per-service dead-letter exchange: queues created by 4.x fail with 406 at start-up
- A handler's `OperationCanceledException` (for example an `HttpClient` timeout) now counts as a failed delivery
- Consumer prefetch is now 10 by default
- The RabbitMQ health check no longer opens its own connection and reports Unhealthy when the consumers are stopped

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

### Other 5.0 changes

Other 5.0 changes are added to this guide as the 5.0 work lands.

## Automated Migration

None: the changes are broker topology and configuration, not API renames.

## Manual Steps

1. For every RabbitMQ consumer, choose option C, B or A above before the first 5.0 deployment.
2. If you relied on the consumer taking the whole queue at once, set `PrefetchCount = 0` or a larger value.
3. If a liveness probe uses the `rabbitmq-eventbus` check, move it to readiness.

## References

- CHANGELOG.md `[Unreleased]`
- [Event bus guide](../providers/event-bus.md)
- `docs/policies/versioning.md`
