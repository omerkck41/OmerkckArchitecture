# ADR-0030 — Built-in Domain Event Handlers for Kck.Persistence.EntityFramework

**Status:** Accepted  
**Date:** 2026-10-30  
**Deciders:** @omerkck41  
**Breaking:** No (additive API, 4.6.0)

---

## Context

`Kck.Persistence.EntityFramework` dispatches domain events with the guarantees that make it the hero module
(ADR-0029): events raised during a transaction wait for its commit, are discarded on rollback, and go back onto the
entity when dispatch fails, so the next save retries them (at-least-once). The interceptor hands the events to an
`IDomainEventDispatcher`:

```csharp
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
```

The library ships no implementation. Every user writes one before the first event is handled, usually a Mediator or
reflection-based bridge, and the docs show a type that does not exist (`MediatorDomainEventDispatcher`). The template
redesign (plan F1) needs a handler model that works with one line of setup.

---

## Decision

| # | Decision |
|---|---|
| K1 | `Kck.Core.Abstractions` adds `public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent { Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken); }` in namespace `Kck.Core.Abstractions.Entities`, next to `IDomainEvent`. |
| K2 | `KckPersistenceBuilder.AddDomainEventHandlers(params Assembly[] assemblies)` registers every concrete, non-generic type in the assemblies that implements `IDomainEventHandler<T>` as a **scoped** `IDomainEventHandler<T>`, and wires an internal `ServiceProviderDomainEventDispatcher` through the existing `AddDomainEventDispatch` path. No assembly → `ArgumentException`. |
| K3 | Dispatch: events in the order the interceptor passes them; for each event, every handler registered for its **exact** runtime type, resolved from the dispatcher's scope. Handler order is not part of the contract. Handlers for a base type or interface of the event are not invoked. An event with no handler is not an error. |
| K4 | The first handler exception stops dispatch and propagates; the interceptor's existing semantics apply (data is saved, events go back onto the entities, the next save retries them). Handlers that already ran for a retried event run again: handlers must be idempotent (same rule as today, documented). |
| K5 | Mixing the two models is an error: `AddDomainEventHandlers` after `AddDomainEventDispatch<T>` with a different dispatcher, or the reverse, throws `InvalidOperationException` naming both registrations. Calling `AddDomainEventHandlers` twice adds the handlers of both calls; a handler type registered twice (same assembly passed again) is registered once. |
| K6 | Pooled contexts (`UseEntityFrameworkPool`) need no change: the pooled interceptor already resolves the dispatcher from a new scope per dispatch, so handlers get a fresh scope. |
| K7 | The handler-type lookup per event type is cached; the invoker uses a generic method created once per event type. The package is not marked `IsAotCompatible` today (EF Core), so the reflection used here adds no new AOT restriction; `Kck.Core.Abstractions` stays AOT-compatible (K1 is an interface). |

---

## Alternatives considered

- **Mediator bridge in `Kck.Pipeline.Mediator`** (events as Mediator notifications): that package references only
  `Kck.Persistence.Abstractions`, not the EF provider where `IDomainEventDispatcher` lives; a new reference would
  invert the dependency direction, and users without Mediator would still have to write a dispatcher. Rejected.
- **Move `IDomainEventDispatcher` to `Kck.Persistence.Abstractions`** so other packages can implement it: breaking
  (namespace and package change). Deferred to the v5 list.
- **Keep the dispatcher in the template only:** every user outside the template still writes plumbing. Rejected.
- **Handlers for base types / polymorphic dispatch:** surprising double handling and ordering questions. Rejected for
  exact-type matching; can be added later without breaking.
- **Singleton handlers:** handlers typically need scoped services (DbContext, repositories). Rejected.

---

## Consequences

- (+) One line enables domain events: `.AddDomainEventHandlers(typeof(Program).Assembly)`.
- (+) The docs and template show a real, compiled type.
- (−) Two ways to dispatch (custom `IDomainEventDispatcher` or handlers); K5 makes mixing them an explicit error.
- (−) Reflection-based assembly scanning at startup (once).
- Tests (plan G1.0): both handlers run, base-type handler not invoked, no handler succeeds, handler failure puts events
  back, pooled context resolves handlers from a new scope, mixing models throws.

---

## Related

- ADR-0029 (adoption plan), `docs/plans/2026-10-30-dx-hero-module-plan.md` G1.0
- `docs/providers/persistence.md` (domain event semantics)
