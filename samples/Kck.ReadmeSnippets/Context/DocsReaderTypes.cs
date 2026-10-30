// The reader's own types the docs/ guides use without defining (plan G2.1 b). Imported by every generated block
// through GlobalUsings.cs; a block that declares a type of the same name uses its own (its namespace wins).
using Kck.Core.Abstractions.Entities;
using Kck.EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ReadmeSnippets.Reader;

// The reader's top-level Program class (typeof(Program).Assembly).
public sealed class Program;

public sealed class EmailCleanupJob : Kck.BackgroundJobs.Abstractions.IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public sealed record OrderCreatedEvent(Guid OrderId, decimal Amount) : IntegrationEvent;

public sealed class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent eventData, CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class Order : FullEntity<Guid>
{
    public string TenantId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}

public sealed record Product(int Id, string Name, decimal Price);

public interface IProductRepository
{
    Task<Product?> FindAsync(int id, CancellationToken ct);
}

public sealed record Sale(int Id, decimal Amount, DateOnly Date);

public sealed record User(Guid Id, string Email);

public sealed record UserDto(Guid Id);

public sealed record PaymentRequest(Guid OrderId, decimal Amount);

public interface ITenantProvider
{
    string TenantId { get; }
}

// Stand-ins for packages a reader adds but this repository does not reference.
internal static class ReaderPackageStandIns
{
    // The Mediator source generator emits AddMediator() into the reader's assembly.
    public static IServiceCollection AddMediator(this IServiceCollection services) => services;

    // Microsoft.EntityFrameworkCore.SqlServer
    public static DbContextOptionsBuilder UseSqlServer(this DbContextOptionsBuilder builder, string? connectionString) =>
        builder;
}
