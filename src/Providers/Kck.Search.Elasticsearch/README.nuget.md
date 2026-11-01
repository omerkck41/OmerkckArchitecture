# Kck.Search.Elasticsearch

Elasticsearch 9.x provider for `ISearchService<T>` — indexing, full-text search, and aggregation with strongly-typed document mapping.

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
| `UseSimpleQueryString` | Run `SearchRequest.Query` as `simple_query_string`, which never throws on syntax errors; recommended for end-user input | `false` (`query_string`) |

The options are validated at startup.

## Errors

`CreateIndexAsync`, `IndexDocumentAsync`, `BulkIndexAsync`, `UpdateDocumentAsync`, `DeleteDocumentAsync`,
`DeleteIndexAsync` and `SearchAsync` throw `InvalidOperationException` when Elasticsearch rejects the request (a bulk
request also when only some items fail; a search on a missing index or with a query-string syntax error too). The
message carries the HTTP status and the server's error type and reason. Deleting a missing document or index does
not throw, `GetByIdAsync` returns `null` for a missing document and `IndexExistsAsync` returns `false` for a missing
index. Before 5.0, `IndexDocumentAsync` and `BulkIndexAsync` only logged a warning, the other writes ignored the
response and `SearchAsync` returned an empty result.

`BulkIndexAsync` with no documents sends no request. A bulk request is not atomic: the items accepted before a failure
stay indexed, and re-running the batch overwrites documents with the same id. `UseSimpleQueryString` enables only the
AND, OR, NOT, phrase, grouping, whitespace and escape operators (no `*` prefix, `~N` fuzzy or slop).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/search.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
