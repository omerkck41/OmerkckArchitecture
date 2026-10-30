# Kck.Templates

`dotnet new` templates for Kck-based .NET projects.

## Install

```bash
dotnet new install Kck.Templates
```

## `kck-webapi` — Kck Web API

A solution (`.slnx`) with an ASP.NET Core API and an integration test project:

| Area | What you get |
|---|---|
| Persistence | `Kck.Persistence.EntityFramework`: repositories, `IUnitOfWork`, audit fields, soft delete that follows your delete rules, domain events dispatched after commit (`IDomainEventHandler<T>`) |
| Database | SQLite by default (no setup); `--database postgres` or `--database sqlserver` |
| Sample | Order management: place (stock reserved in the same transaction), get, list (paged), cancel (soft delete) |
| API | Minimal APIs, ProblemDetails, rate limiting, security headers, CORS from configuration, OpenAPI + Scalar UI in development |
| Security | JWT bearer authentication on `/orders`; local tokens with `dotnet user-jwts`; no secrets in the generated files |
| Health | `/health`, `/health/live`, `/health/ready` (database check), `/health/startup` |
| Tests | xUnit, `WebApplicationFactory`, in-memory SQLite (or a per-test database for PostgreSQL / SQL Server) |

```bash
dotnet new kck-webapi -n Shop
cd Shop
dotnet run --project src/Shop.Api        # opens the API reference at /scalar
dotnet test
```

| Option | Default | Description |
|---|---|---|
| `--database sqlite\|postgres\|sqlserver` | `sqlite` | Database provider |
| `--include-tests` | `true` | Add the integration test project |
| `--include-sample` | `true` | Add the order management sample |
| `-f, --framework` | `net10.0` | Target framework |

Since 4.6.0 the output is a solution with `src/` and `tests/` instead of a single controller project; to keep the old
output, install an earlier version: `dotnet new install Kck.Templates::4.5.0`.

## Uninstall

```bash
dotnet new uninstall Kck.Templates
```
