using Kck.Documents.Abstractions;
using Kck.Documents.SkiaSharp;
using Kck.Documents.SkiaSharp.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the SkiaSharp-backed <see cref="IImageProcessor"/>.
/// </summary>
public static class KckDocumentsSkiaSharpServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="SkiaSharpImageProcessor"/> as the singleton <see cref="IImageProcessor"/> and validates
    /// <see cref="SkiaSharpOptions"/> at startup. On Linux the application must also reference
    /// <c>SkiaSharp.NativeAssets.Linux.NoDependencies</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional options callback (for example to change <see cref="SkiaSharpOptions.MaxPixels"/>).</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddKckDocumentsSkiaSharp(
        this IServiceCollection services,
        Action<SkiaSharpOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure(configure ?? (_ => { }));
        services.TryAddSingleton<IValidateOptions<SkiaSharpOptions>, SkiaSharpOptionsValidator>();
        services.AddOptions<SkiaSharpOptions>().ValidateOnStart();
        services.TryAddSingleton<IImageProcessor, SkiaSharpImageProcessor>();
        return services;
    }
}
