using Core.BackgroundJobs.Interfaces;
using Core.BackgroundJobs.Services;
using Hangfire;
using Hangfire.MySql;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;

namespace Core.BackgroundJobs.Extensions;

public static class BackgroundJobExtensions
{
    public static IServiceCollection AddHangfireJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var storageType = configuration["Hangfire:StorageType"];
        var connectionString = configuration.GetConnectionString("HangfireConnection");

        var storage = HangfireStorageFactory.CreateStorage(storageType, connectionString);
        services.AddHangfire(config => config.UseStorage(storage));

        services.AddHangfireServer();
        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();
        return services;
    }

    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddSingleton<IScheduler>(provider =>
        {
            var schedulerFactory = new StdSchedulerFactory();
            return schedulerFactory.GetScheduler().Result;
        });
        services.AddSingleton<IJobScheduler, QuartzJobScheduler>();
        return services;
    }
}
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