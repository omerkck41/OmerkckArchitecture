using Core.BackgroundJobs.Interfaces;
using Core.BackgroundJobs.Services;
using Hangfire;
using Hangfire.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.BackgroundJobs.Extensions;

public static class BackgroundJobExtensions
{
    public static IServiceCollection AddHangfireJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var storageType = configuration["Hangfire:StorageType"];

        if (storageType == "SqlServer")
        {
            services.AddHangfire(config => config.UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));
        }
        else if (storageType == "MySql")
        {
            services.AddHangfire(config => config.UseStorage(new MySqlStorage(configuration.GetConnectionString("HangfireConnection"), new MySqlStorageOptions())));
        }

        services.AddHangfireServer();
        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();
        return services;
    }

    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddSingleton<IJobScheduler, QuartzJobScheduler>();
        return services;
    }
}