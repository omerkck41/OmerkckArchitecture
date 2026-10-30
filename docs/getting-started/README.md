# Get started: your first Kck API in five minutes

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). Nothing else: the API runs on SQLite.

## 1. Create and run the API

```bash
dotnet new install Kck.Templates
dotnet new kck-webapi -n Shop
cd Shop
dotnet run --project src/Shop.Api
```

The browser opens the API reference at `http://localhost:5080/scalar`. The health endpoint needs no token:

```bash
curl http://localhost:5080/health/ready
```

```json
{"status":"Healthy","checks":[{"name":"appdbcontext","status":"Healthy","description":"Database 'AppDbContext' is reachable.","duration":48.6}],"totalDuration":51.8}
```

## 2. Get a token

The order endpoints require a JWT. In development, create one with the .NET SDK (in a second terminal, from the
`Shop` folder):

```bash
dotnet user-jwts create --project src/Shop.Api
```

The signing key is stored in your user profile, never in the repository. Without a token, `/orders` answers `401`.
The rest of this guide uses `Shop.http` — open it in Visual Studio, Rider or VS Code (REST Client) and paste the token
— or `curl` with `-H "Authorization: Bearer <token>"`.

## 3. Place an order

```http
POST http://localhost:5080/orders
Authorization: Bearer <token>
Content-Type: application/json

{
  "customerId": "0199f0a0-0000-7000-8000-000000000001",
  "lines": [{ "sku": "A-1", "quantity": 2 }, { "sku": "B-2", "quantity": 1 }]
}
```

`201 Created`, with prices taken from the catalogue:

```json
{"id":"01a18cdd-...","customerId":"0199f0a0-...","total":45.5,"isDeleted":false,"lines":[{"sku":"A-1","quantity":2,"unitPrice":10.0,"isDeleted":false},{"sku":"B-2","quantity":1,"unitPrice":25.5,"isDeleted":false}]}
```

The API log shows the domain event, handled **after the order was committed**:

```text
[15:51:06 INF] Order 01a18cdd-... placed by customer 0199f0a0-..., total 45.5
```

That line comes from `Orders/OrderPlacedHandler.cs`, an `IDomainEventHandler<OrderPlaced>`. `Order.Place` raises the
event; `Kck.Persistence.EntityFramework` holds it until the transaction commits.

## 4. See a rollback discard the event

Order more than the stock (B-2 has 5):

```http
POST http://localhost:5080/orders
Authorization: Bearer <token>
Content-Type: application/json

{ "customerId": "0199f0a0-0000-7000-8000-000000000001", "lines": [{ "sku": "B-2", "quantity": 50 }] }
```

```json
{"title":"Insufficient stock","status":409,"detail":"Not enough stock for B-2."}
```

`Orders/PlaceOrder.cs` saves the order and reserves the stock in one transaction. The reservation fails, the
transaction rolls back: no order is saved, and the `OrderPlaced` event raised for it is discarded — no log line, no
handler call. You did not write any code for that; it is how Kck dispatches domain events.

## 5. Cancel the order: a soft delete

```http
DELETE http://localhost:5080/orders/{id}
```

`204`. Now `GET /orders/{id}` answers `404`, and `GET /orders/{id}?includeDeleted=true` still finds it:

```json
{"id":"01a18cdd-...","total":45.5,"isDeleted":true,"deletedDate":"2026-10-30T12:51:06.77","lines":[{"sku":"B-2","quantity":1,"unitPrice":25.5,"isDeleted":true},{"sku":"A-1","quantity":2,"unitPrice":10.0,"isDeleted":true}]}
```

The order **and its lines** were soft-deleted — the lines follow the `Cascade` relationship in `Data/AppDbContext.cs`
— while the customer, reached through a `Restrict` relationship, was not touched. Soft-deleted rows are hidden from
every query until a query asks for them (`QueryOptions.WithDeleted`).

## 6. Run the tests

```bash
dotnet test
```

The integration tests start the API in memory against an in-memory SQLite database and check what you just did by
hand: the event arrives only after the commit, a shortage saves nothing, cancelling soft-deletes the lines too.

## Next

- **Make it yours:** add an entity — a class deriving from `FullEntity<Guid>` (audit fields and soft delete included),
  a `DbSet` in `AppDbContext`, and an endpoint that uses `IUnitOfWork.Repository<T, Guid>()`. Raise a domain event with
  `AddDomainEvent(...)` and handle it in an `IDomainEventHandler<T>`; `AddDomainEventHandlers` in `Program.cs` already
  picks it up.
- **Other databases:** `dotnet new kck-webapi -n Shop --database postgres` (or `sqlserver`), then
  `dotnet user-secrets set ConnectionStrings:Default "..." --project src/Shop.Api`.
- **Before production:** the generated `README.md` lists what to change — migrations instead of `EnsureCreated`, your
  identity provider, authorization rules (the sample has none: any authenticated caller can read and cancel any order),
  forwarded headers behind a proxy.
- **Reference:** [persistence guide](../providers/persistence.md), [all provider guides](../providers/README.md),
  [architecture decisions](../adr/README.md).
