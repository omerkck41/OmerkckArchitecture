using FluentAssertions;
using Kck.Search.Abstractions;
using Kck.Search.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Search.Elasticsearch.Tests;

public sealed class ElasticsearchSearchServiceTests
{
    [Fact]
    public void AddKckSearchElasticsearch_RegistersService()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckSearchElasticsearch<TestDocument>(opts =>
        {
            opts.ConnectionString = "http://localhost:9200";
            opts.DefaultIndex = "test";
        });

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<ISearchService<TestDocument>>();
        service.Should().NotBeNull();
        service.Should().BeOfType<ElasticsearchSearchService<TestDocument>>();
    }

    [Fact]
    public void SearchRequest_DefaultValues_AreCorrect()
    {
        var request = new SearchRequest { Query = "test" };
        request.Size.Should().Be(20);
        request.From.Should().Be(0);
        request.SortAscending.Should().BeTrue();
        request.SortField.Should().BeNull();
    }

    private sealed class TestDocument
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
