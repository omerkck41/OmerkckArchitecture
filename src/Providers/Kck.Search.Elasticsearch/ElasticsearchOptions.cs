namespace Kck.Search.Elasticsearch;

public sealed class ElasticsearchOptions
{
    public required string ConnectionString { get; set; }
    public string? DefaultIndex { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int NumberOfShards { get; set; } = 1;
    public int NumberOfReplicas { get; set; } = 1;
    /// <summary>
    /// Disable TLS certificate verification. Use only in development/testing.
    /// </summary>
    public bool DisableSslVerification { get; set; }

    /// <summary>
    /// Runs <c>SearchRequest.Query</c> as a <c>simple_query_string</c> query, which never fails on syntax errors and
    /// ignores unsupported operators. Recommended when the query text comes from end users. The default
    /// (<see langword="false"/>) keeps <c>query_string</c> with the full Lucene syntax.
    /// </summary>
    public bool UseSimpleQueryString { get; set; }
}
