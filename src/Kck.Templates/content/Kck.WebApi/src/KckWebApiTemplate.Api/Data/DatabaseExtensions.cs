using Microsoft.EntityFrameworkCore;

namespace KckWebApiTemplate.Api.Data;

public static class DatabaseExtensions
{
    /// <summary>
    /// Creates the database and schema if they do not exist (development only). It does not update an existing schema;
    /// switch to EF Core migrations when the model starts to change (README.md, "Migrations").
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
