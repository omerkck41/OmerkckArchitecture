using Core.Application.ElasticSearch.Interfaces;
using Core.Application.ElasticSearch.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.ElasticSearch;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticSearchSettings>(configuration.GetSection("ElasticSearchSettings"));
        services.AddSingleton<IElasticSearchService, ElasticSearchService>();
        return services;
    }
}