using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provider-agnostic registration helpers for <see cref="IBackgroundJob"/> implementations.
/// Works with both Hangfire and Quartz providers.
/// </summary>
public static class BackgroundJobRegistrationExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TJob"/> as a scoped service so the scheduler can
    /// resolve a fresh instance (with scoped dependencies such as <c>DbContext</c>) per execution.
    /// </summary>
    public static IServiceCollection AddKckJob<TJob>(this IServiceCollection services)
        where TJob : class, IBackgroundJob
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<TJob>();
        return services;
    }
}
