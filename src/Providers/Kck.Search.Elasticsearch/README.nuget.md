# Kck.Search.Elasticsearch

Elasticsearch 8 provider for `ISearchService<T>` — indexing, full-text search, and aggregation with strongly-typed document mapping.

## Installation

```bash
dotnet add package Kck.Search.Elasticsearch
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckSearchElasticsearch<ProductDocument>(options =>
{
    options.ConnectionString = "https://localhost:9200";
    options.Username = "elastic";
    options.Password = Environment.GetEnvironmentVariable("ES_PASSWORD")!;
    options.DefaultIndex = "products";
});

// Index and search
public class ProductSearchService(ISearchService<ProductDocument> search)
{
    public async Task IndexAsync(ProductDocument doc, CancellationToken ct)
        => await search.IndexAsync(doc, ct);

    public async Task<IReadOnlyList<ProductDocument>> SearchAsync(
        string query, CancellationToken ct)
        => await search.SearchAsync(q => q.Match(m => m.Field(f => f.Name).Query(query)), ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | Elasticsearch node URL | required |
| `Username` | Elasticsearch username | `"elastic"` |
| `Password` | Elasticsearch password | required |
| `DefaultIndex` | Default index for the document type | required |
| `NumberOfShards` | Index shard count | `1` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/search.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
