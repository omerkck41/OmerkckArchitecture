using Kck.Persistence.EntityFramework.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering EF Core health checks.
/// </summary>
public static class EfCoreHealthChecksBuilderExtensions
{
    /// <summary>
    /// Adds a health check that verifies the database connection for <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> type registered in DI.</typeparam>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="name">Health check name. Defaults to the context type name.</param>
    /// <param name="tags">Tags applied to the check (e.g., "ready").</param>
    public static IHealthChecksBuilder AddKckEfCoreCheck<TContext>(
        this IHealthChecksBuilder builder,
        string? name = null,
        IEnumerable<string>? tags = null)
        where TContext : DbContext
    {
        var checkName = name ?? typeof(TContext).Name.ToLowerInvariant();
        return builder.AddCheck<EfCoreHealthCheck<TContext>>(checkName, tags: tags ?? ["ready"]);
    }
}
