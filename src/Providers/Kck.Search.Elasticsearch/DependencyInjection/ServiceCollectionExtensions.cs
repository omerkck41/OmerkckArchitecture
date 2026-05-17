using Kck.Search.Abstractions;
using Kck.Search.Elasticsearch;
using Kck.Search.Elasticsearch.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSearchElasticsearchServiceCollectionExtensions
{
    public static IServiceCollection AddKckSearchElasticsearch<T>(
        this IServiceCollection services,
        Action<ElasticsearchOptions> configure) where T : class
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<ElasticsearchOptions>, ElasticsearchOptionsValidator>();
        services.AddOptions<ElasticsearchOptions>().ValidateOnStart();
        services.TryAddSingleton<ISearchService<T>, ElasticsearchSearchService<T>>();
        return services;
    }
}
