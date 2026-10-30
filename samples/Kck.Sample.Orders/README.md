# Kck.Sample.Orders

The reference application: what `dotnet new kck-webapi` creates, grown into a small order service. Every feature is a
vertical slice under `Features/`, and every behaviour below is covered by
[`tests/Kck.Sample.Orders.Tests`](../../tests/Kck.Sample.Orders.Tests).

| Shows | Where |
|---|---|
| Order and stock reservation in one transaction; 409 and nothing saved when stock runs out | `Features/Orders/PlaceOrder.cs` |
| Status changes Placed → Paid → Shipped, 409 for any other move | `Domain/Order.cs`, `Features/Orders/ChangeOrderStatus.cs` |
| Soft delete that follows the delete rules: an order takes its lines along (`Cascade`); a customer with orders cannot be deleted (`Restrict`) | `Data/OrdersDbContext.cs`, `Features/Orders/CancelOrder.cs`, `Features/Customers/CustomerEndpoints.cs` |
| A domain event dispatched after the commit, turned into an integration event on the in-memory bus | `Features/Orders/OrderPlacedHandler.cs` |
| Dynamic search limited to whitelisted fields | `Features/Orders/SearchOrders.cs` |
| Paging | `Features/Orders/ListOrders.cs` |
| A cached product catalogue (`ICacheService`) | `Features/Products/ListProducts.cs` |
| OpenTelemetry traces and metrics, exported when `OTEL_EXPORTER_OTLP_ENDPOINT` is set | `Program.cs` |
| Health checks including the database | `Program.cs` (`/health/ready`) |

## Run

```bash
dotnet user-jwts create --project samples/Kck.Sample.Orders
dotnet run --project samples/Kck.Sample.Orders
```

Development uses a SQLite file (`orders.db`) created on first run with two products and a demo customer. Send the
requests in `Kck.Sample.Orders.http` with the token from the first command; the interactive API reference is at
`/scalar`.

PostgreSQL instead of SQLite:

```bash
dotnet user-secrets set ConnectionStrings:Orders "Host=localhost;Database=orders;Username=...;Password=..." --project samples/Kck.Sample.Orders
dotnet run --project samples/Kck.Sample.Orders -- --Database:Provider Postgres
```

## Test

```bash
dotnet test tests/Kck.Sample.Orders.Tests --filter "Category!=Integration"
dotnet test tests/Kck.Sample.Orders.Tests --filter "Category=Integration"
```

The first runs against SQLite in memory; the second starts PostgreSQL in Docker (Testcontainers).

## Limits

- Domain events are delivered at least once and only while the request runs: a handler that fails after the commit
  returns an error to the caller, and the event is retried only by a later save in the same request. Durable delivery
  needs an outbox (see the [persistence guide](../../docs/providers/persistence.md#interceptors)).
- No ownership rules: any authenticated caller can read or change any order.
- The schema is created with `EnsureCreated`; use EF Core migrations once the model changes.
