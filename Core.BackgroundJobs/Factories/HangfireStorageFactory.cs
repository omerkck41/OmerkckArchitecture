using Hangfire;
using Hangfire.MySql;
using Hangfire.SqlServer;

namespace Core.BackgroundJobs.Factories;
public static class HangfireStorageFactory
{
    public static JobStorage CreateStorage(string storageType, string connectionString)
    {
        return storageType switch
        {
            "SqlServer" => new SqlServerStorage(connectionString),
            "MySql" => new MySqlStorage(connectionString, new MySqlStorageOptions()),
            _ => throw new NotSupportedException($"Storage type '{storageType}' is not supported.")
        };
    }
}