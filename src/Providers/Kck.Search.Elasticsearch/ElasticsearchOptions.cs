namespace Kck.Search.Elasticsearch;

public sealed class ElasticsearchOptions
{
    public required string ConnectionString { get; set; }
    public string? DefaultIndex { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int NumberOfShards { get; set; } = 1;
    public int NumberOfReplicas { get; set; } = 1;
}
