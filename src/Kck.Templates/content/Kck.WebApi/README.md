# TEMPLATE_RAW_NAME

An ASP.NET Core API built on [Kck](https://github.com/omerkck41/OmerkckArchitecture): EF Core persistence with soft
delete, audit fields and domain events dispatched after commit, JWT bearer authentication, health checks,
ProblemDetails and rate limiting.

## Run

```bash
dotnet run --project src/TEMPLATE_RAW_NAME.Api
```

<!--#if (UseSqlite) -->
The SQLite database file is created on first run. In development the browser opens the API reference at `/scalar`.
<!--#else -->
Set the connection string first (stored in your user profile, never in the repository):

```bash
dotnet user-secrets set ConnectionStrings:Default "<your connection string>" --project src/TEMPLATE_RAW_NAME.Api
```

The database and schema are created on first run. In development the browser opens the API reference at `/scalar`.
<!--#endif -->
`/health/ready` answers without a token.

<!--#if (IncludeSample) -->
## Try the order API

The `/orders` endpoints need a token. Create one for local development:

```bash
dotnet user-jwts create --project src/TEMPLATE_RAW_NAME.Api
```

Paste it into [TEMPLATE_RAW_NAME.http](TEMPLATE_RAW_NAME.http) and send the requests in order. What to look for:

- **Place an order** — `201`; the log shows `Order ... placed` from `OrderPlacedHandler`, written after the commit.
- **Not enough stock** — `409`. The order and the stock reservation run in one transaction; it rolls back, nothing is
  saved, and `OrderPlaced` is discarded instead of dispatched.
- **Cancel** — `204`. A soft delete: the order and its lines are marked deleted and hidden from queries
  (`?includeDeleted=true` shows them); the customer is untouched.

Where it happens: `src/TEMPLATE_RAW_NAME.Api/Orders/` (one file per endpoint), `Data/AppDbContext.cs` (relationships
that decide what a soft delete covers) and `Program.cs` (`AddKckPersistence`).

<!--#endif -->
<!--#if (IncludeTests) -->
## Test

```bash
dotnet test
```

<!--#if (UseSqlite) -->
The tests start the API in memory against an in-memory SQLite database; nothing to set up.
<!--#else -->
The tests start the API in memory against a database of their own (the connection string above with a `_test_...`
suffix), created and dropped by each test.
<!--#endif -->

<!--#endif -->
## Next steps

- **Migrations.** The schema is created with `EnsureCreated` in development, which never updates an existing
  database. Before the model starts to change, switch to migrations: `dotnet tool install --global dotnet-ef`,
  `dotnet ef migrations add Initial --project src/TEMPLATE_RAW_NAME.Api`, and replace `EnsureDatabaseCreatedAsync`
  in `Program.cs` with `Database.MigrateAsync()` or a deployment step.
- **Authentication in production.** Configure `Authentication:Schemes:Bearer` (authority / issuer, audiences) for your
  identity provider.
<!--#if (IncludeSample) -->
- **Authorization — not in the sample.** Any authenticated caller can read, place and cancel any order, and the
  customer comes from the request body. Take the customer from the token (for example the `sub` claim) and filter every
  order query by it before you go to production.
<!--#endif -->
- **Behind a proxy or load balancer.** Rate limiting is per client IP; add `app.UseForwardedHeaders()` with your
  proxies in `KnownProxies` / `KnownNetworks`, or every client shares the proxy's limit.
- **Health endpoints** answer without a token and list check names and durations; expose them only to your
  orchestrator's network.
- **CORS.** List allowed origins in `Kck:AspNetCore:CorsOrigins`; none are allowed by default.
<!--#if (!UseSqlServer) -->
- **Concurrency.** `FullEntity.RowVersion` is generated only by SQL Server, so optimistic concurrency checks are off on
  this provider (`AppDbContext` does not map the column).
<!--#endif -->
- **Docs.** [Kck documentation](https://omerkck41.github.io/OmerkckArchitecture/).
