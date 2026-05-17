# Kck.Search.Abstractions

Full-text search abstractions with index management — `ISearchService<T>` for create/delete index, document CRUD, bulk indexing, and query operations backed by the Elasticsearch provider.

## Installation

```bash
dotnet add package Kck.Search.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (e.g. Kck.Search.Elasticsearch)
builder.Services.AddKckElasticsearch(builder.Configuration);

// Use ISearchService<T> in a service
public class ProductSearchService(ISearchService<ProductDocument> search)
{
    public async Task IndexProductAsync(Product product, CancellationToken ct)
    {
        var doc = new ProductDocument(product.Id, product.Name, product.Description);
        await search.IndexAsync(doc, indexName: "products", ct);
    }

    public async Task<IReadOnlyList<ProductDocument>> SearchAsync(
        string query, CancellationToken ct)
    {
        var request = new SearchRequest
        {
            Query     = query,
            IndexName = "products",
            PageSize  = 20,
            PageIndex = 1
        };
        var result = await search.SearchAsync(request, ct);
        return result.Items;
    }

    public async Task BulkIndexAsync(
        IEnumerable<ProductDocument> docs, CancellationToken ct)
        => await search.BulkIndexAsync(docs, indexName: "products", ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Search:Elasticsearch:Uri` | Elasticsearch node URI | `"http://localhost:9200"` |
| `Search:Elasticsearch:DefaultIndex` | Fallback index name | `"default"` |
| `Search:Elasticsearch:NumberOfShards` | Shards per created index | `1` |
| `Search:Elasticsearch:NumberOfReplicas` | Replicas per created index | `1` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/search.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
