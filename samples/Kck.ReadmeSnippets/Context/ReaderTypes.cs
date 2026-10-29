// The reader's own types each README block assumes (one namespace per generated block). Kept minimal: only what the
// block needs to compile. Stand-ins replace packages a reader adds but this repository does not reference
// (Microsoft.EntityFrameworkCore.SqlServer, the Mediator source generator).
using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace ReadmeSnippets.Readme_2
{
    public sealed record CreateUserCommand(string Email) : IRequest<Guid>;

    internal static class MediatorSourceGeneratorStandIn
    {
        public static IServiceCollection AddMediator(this IServiceCollection services) => services;
    }
}

namespace ReadmeSnippets.Readme_3
{
    public sealed class EmailDigestJob : IBackgroundJob
    {
        public Task ExecuteAsync(CancellationToken ct = default) => Task.CompletedTask;
    }
}

namespace ReadmeSnippets.BundleMinimalApi_1
{
    public sealed record Product(int Id);

    public interface IProductService
    {
        Task<Product?> GetAsync(int id, CancellationToken ct);
    }
}

namespace ReadmeSnippets.BundleWorkerService_1
{
    public interface IReportService
    {
        Task GenerateDailyAsync(CancellationToken ct);
    }
}

namespace ReadmeSnippets.PersistenceAbstractions_1
{
    public sealed class Order : FullEntity<Guid>
    {
        public decimal Total { get; set; }
    }

    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);

    internal static class SqlServerStandIn
    {
        public static DbContextOptionsBuilder UseSqlServer(this DbContextOptionsBuilder builder, string? connectionString) => builder;
    }
}

namespace ReadmeSnippets.PersistenceEntityFramework_1
{
    public sealed class Order : FullEntity<Guid>;

    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);

    internal static class SqlServerStandIn
    {
        public static DbContextOptionsBuilder UseSqlServer(this DbContextOptionsBuilder builder, string? connectionString) => builder;
    }
}

namespace ReadmeSnippets.PipelineMediator_1
{
    public sealed class Order : FullEntity<Guid>
    {
        public Order(string productId, int quantity) => (ProductId, Quantity) = (productId, quantity);

        public string ProductId { get; }
        public int Quantity { get; }
    }

    public sealed record OrderResult(Guid OrderId);

    internal static class MediatorSourceGeneratorStandIn
    {
        public static IServiceCollection AddMediator(this IServiceCollection services) => services;
    }
}

namespace ReadmeSnippets.CachingInMemory_1
{
    public sealed class Category
    {
        public int Id { get; set; }
    }

    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Category> Categories => Set<Category>();
    }
}
