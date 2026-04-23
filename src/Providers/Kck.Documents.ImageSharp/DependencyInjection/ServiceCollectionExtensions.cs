using Kck.Documents.Abstractions;
using Kck.Documents.ImageSharp;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckDocumentsImageSharpServiceCollectionExtensions
{
    public static IServiceCollection AddKckDocumentsImageSharp(this IServiceCollection services)
    {
        services.TryAddSingleton<IImageProcessor, ImageSharpProcessor>();
        return services;
    }
}
