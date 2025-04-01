using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Application.ElasticSearch;

public class ElasticSearchSettings
{
    public string ConnectionString { get; }
    public string DefaultIndex { get; }
    public string Username { get; }
    public string Password { get; }

    public ElasticSearchSettings(string connectionString, string defaultIndex, string username, string password)
    {
        ConnectionString = connectionString ?? throw new CustomArgumentException(nameof(connectionString));
        DefaultIndex = defaultIndex ?? throw new CustomArgumentException(nameof(defaultIndex));
        Username = username ?? throw new CustomArgumentException(nameof(username));
        Password = password ?? throw new CustomArgumentException(nameof(password));
    }
}