using Microsoft.EntityFrameworkCore;

namespace Kck.Sample.Orders.Data;

public static class DatabaseExtensions
{
    /// <summary>
    /// Selects the database from <c>Database:Provider</c> (<c>Sqlite</c>, the default, or <c>Postgres</c>) and the
    /// connection string <c>ConnectionStrings:Orders</c>. Without a connection string, development uses a local SQLite
    /// file; any other environment must configure one.
    /// </summary>
    public static void UseOrdersDatabase(this DbContextOptionsBuilder options, IConfiguration configuration, IHostEnvironment environment)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Orders");

        if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            options.UseNpgsql(connectionString
                ?? throw new InvalidOperationException(
                    "Connection string 'Orders' is not configured. Set it with `dotnet user-secrets set ConnectionStrings:Orders \"...\"`."));
            return;
        }

        if (!provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Database:Provider '{provider}' is not supported; use Sqlite or Postgres.");

        options.UseSqlite(connectionString
            ?? (environment.IsDevelopment()
                ? $"Data Source={Path.Combine(environment.ContentRootPath, "orders.db")}"
                : throw new InvalidOperationException("Connection string 'Orders' is not configured.")));
    }

    /// <summary>
    /// Creates the database and schema if they do not exist (development only). It does not update an existing schema;
    /// use EF Core migrations once the model starts to change.
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.EnsureCreatedAsync();
    }
}
