using Kck.Persistence.Abstractions.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// Reads the optional <see cref="IAuditUserProvider"/> from the application service provider the context was created
/// from (the request scope for <c>AddDbContext</c>, the root provider for a pooled context).
/// </summary>
internal static class AuditUser
{
    /// <summary>Resolves the user to stamp on audited entities of <paramref name="context"/>'s current save.</summary>
    /// <returns>The current user, or <see langword="null"/> when no provider is registered or it has no user.</returns>
    public static string? Resolve(DbContext context)
    {
        var services = context.GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()?
            .ApplicationServiceProvider;
        var user = services?.GetService<IAuditUserProvider>()?.CurrentUser;
        return string.IsNullOrEmpty(user) ? null : user;
    }
}
