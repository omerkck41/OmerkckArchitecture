# Kck.Pipeline.Mediator

Pipeline behaviors (validation, logging, caching, authorization, transaction) for the source-generated [Mediator](https://github.com/martinothamar/Mediator) dispatcher in Kck applications.

## Installation

```bash
dotnet add package Kck.Pipeline.Mediator
dotnet add package Mediator.SourceGenerator    # generates AddMediator() and the dispatcher
```

## Quick Start

```csharp
// Program.cs
builder.Services
    .AddMediator()                       // source-generated dispatcher (Mediator.SourceGenerator)
    .AddKckMediatorPipeline(p => p
        .UseValidationBehavior()         // FluentValidation validators from DI
        .UseLoggingBehavior()            // ILoggableRequest
        .UseCachingBehavior());          // ICachableRequest, IDistributedCache

// Define a command
public record CreateOrderCommand(string ProductId, int Quantity)
    : ICommand<OrderResult>;

// Handle it
public class CreateOrderHandler(IRepository<Order, Guid> orders)
    : ICommandHandler<CreateOrderCommand, OrderResult>
{
    public async ValueTask<OrderResult> Handle(
        CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = new Order(cmd.ProductId, cmd.Quantity);
        await orders.AddAsync(order, ct);
        return new OrderResult(order.Id);
    }
}
```

## Configuration

| Method | Applies to |
|---|---|
| `UseValidationBehavior()` | every message (FluentValidation `IValidator<T>` from DI) |
| `UseLoggingBehavior()` | `ILoggableRequest` |
| `UseCachingBehavior()` / `UseCachingBehavior(o => o.SerializerOptions = ...)` | `ICachableRequest` (source-generated JSON for Native AOT) |
| `UseAuthorizationBehavior()` | `ISecuredRequest` |
| `UseTransactionBehavior()` | `ITransactionalRequest` (`IUnitOfWork`) |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/pipeline.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
