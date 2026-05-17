# Kck.Pipeline.Mediator

Source-generator-based CQRS mediator with zero-reflection dispatch, validation, logging, and performance behaviors for Kck applications.

## Installation

```bash
dotnet add package Kck.Pipeline.Mediator
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckMediator(options =>
{
    options.RegisterServicesFromAssembly(typeof(Program).Assembly);
    options.EnableValidationBehavior = true;
    options.EnableLoggingBehavior = true;
    options.EnablePerformanceBehavior = true;
});

// Define a command
public record CreateOrderCommand(string ProductId, int Quantity)
    : ICommand<OrderResult>;

// Handle it
public class CreateOrderHandler(IRepository<Order> orders)
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

| Property | Description | Default |
|---|---|---|
| `EnableValidationBehavior` | Run FluentValidation before handlers | `true` |
| `EnableLoggingBehavior` | Structured request/response logging | `true` |
| `EnablePerformanceBehavior` | Warn when handler exceeds threshold | `false` |
| `PerformanceThresholdMs` | Handler duration warning threshold | `500` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/pipeline.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
