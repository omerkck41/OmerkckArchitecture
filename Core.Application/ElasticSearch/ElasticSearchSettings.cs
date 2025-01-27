namespace Core.Application.ElasticSearch;

public class ElasticSearchSettings
{
    public string ConnectionString { get; }
    public string DefaultIndex { get; }
    public string Username { get; }
    public string Password { get; }

    public ElasticSearchSettings(string connectionString, string defaultIndex, string username, string password)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        DefaultIndex = defaultIndex ?? throw new ArgumentNullException(nameof(defaultIndex));
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }
}