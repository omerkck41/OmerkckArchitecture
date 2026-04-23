using Kck.Search.Abstractions;
using Kck.Search.Elasticsearch;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSearchElasticsearchServiceCollectionExtensions
{
    public static IServiceCollection AddKckSearchElasticsearch<T>(
        this IServiceCollection services,
        Action<ElasticsearchOptions> configure) where T : class
    {
        services.Configure(configure);
        services.TryAddSingleton<ISearchService<T>, ElasticsearchSearchService<T>>();
        return services;
    }
}
