using Core.BackgroundJobs.Factories;
using Core.BackgroundJobs.Interfaces;
using Core.BackgroundJobs.Services;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;

namespace Core.BackgroundJobs.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHangfireJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var storageType = configuration["Hangfire:StorageType"];
        if (string.IsNullOrWhiteSpace(storageType))
            throw new NotFoundException("Hangfire:StorageType setting not found.");

        var connectionString = configuration.GetConnectionString("HangfireConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new NotFoundException("HangfireConnection connection string not found.");

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
