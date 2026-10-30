# Kck.Search.Elasticsearch

Elasticsearch 8 provider for `ISearchService<T>` — indexing, full-text search, and aggregation with strongly-typed document mapping.

## Installation

```bash
dotnet add package Kck.Search.Elasticsearch
```

## Quick Start

```csharp
using Kck.Search.Abstractions;

// Program.cs - one registration per document type
builder.Services.AddKckSearchElasticsearch<ProductDocument>(options =>
{
    options.ConnectionString = "https://localhost:9200";
    options.Username = "elastic";
    options.Password = Environment.GetEnvironmentVariable("ES_PASSWORD");
    options.DefaultIndex = "products";
});

public sealed record ProductDocument(string Id, string Name);

// Index and search (query-string syntax, e.g. "name:phone")
public class ProductSearchService(ISearchService<ProductDocument> search)
{
    public Task IndexAsync(ProductDocument doc, CancellationToken ct) =>
        search.IndexDocumentAsync("products", doc.Id, doc, ct);

    public async Task<IReadOnlyList<ProductDocument>> SearchAsync(string query, CancellationToken ct) =>
        (await search.SearchAsync(new SearchRequest { Query = query }, ct)).Items;
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | Elasticsearch node URL | required |
| `DefaultIndex` | Index used when `SearchRequest.IndexName` is not set | `null` |
| `Username` | Basic authentication user | `null` |
| `Password` | Basic authentication password | `null` |
| `NumberOfShards` | Shards of an index created by `CreateIndexAsync` | `1` |
| `NumberOfReplicas` | Replicas of an index created by `CreateIndexAsync` | `1` |
| `DisableSslVerification` | Skip certificate validation (local clusters only, never production) | `false` |

The options are validated at startup.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/search.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
