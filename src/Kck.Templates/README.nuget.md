# Kck.Templates

`dotnet new` templates for Kck-based .NET projects. One command gives you a running API with persistence, domain
events, authentication, health checks and tests — on SQLite, with nothing to set up.

## Installation

```bash
dotnet new install Kck.Templates
```

## Quick start

```bash
dotnet new kck-webapi -n Shop
cd Shop
dotnet run --project src/Shop.Api   # API reference at /scalar, health at /health/ready
dotnet test
```

The generated solution:

- **Persistence** with `Kck.Persistence.EntityFramework`: repositories and `IUnitOfWork`, audit fields, soft delete
  that follows your delete rules, and domain events handled by `IDomainEventHandler<T>` after the data is committed.
- **An order management sample**: placing an order reserves stock in the same transaction — when stock runs out the
  transaction rolls back and the `OrderPlaced` event is discarded, never dispatched. Cancelling is a soft delete of the
  order and its lines.
- **JWT bearer authentication** on `/orders` (local tokens with `dotnet user-jwts create`), ProblemDetails, rate
  limiting, security headers, CORS from configuration, OpenAPI with the Scalar UI in development. No secrets in the
  generated files.
- **Integration tests** with xUnit and `WebApplicationFactory`.

## Options

| Option | Default | Description |
|---|---|---|
| `--database sqlite\|postgres\|sqlserver` | `sqlite` | Database provider |
| `--include-tests` | `true` | Add the integration test project |
| `--include-sample` | `true` | Add the order management sample |

## Resources

- [Documentation](https://omerkck41.github.io/OmerkckArchitecture/)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
