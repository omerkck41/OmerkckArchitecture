using Core.Infrastructure.UniversalFTP.Factories;
using Core.Infrastructure.UniversalFTP.Services.Implementations;
using Core.Infrastructure.UniversalFTP.Services.Interfaces;
using Core.Infrastructure.UniversalFTP.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.UniversalFTP.Helper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUniversalFtpServices(this IServiceCollection services, FtpSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings), "FtpSettings cannot be null.");

        services.Configure<FtpSettings>(opts =>
        {
            opts.Host = settings.Host;
            opts.Port = settings.Port;
            opts.Username = settings.Username;
            opts.Password = settings.Password;
        });

        services.AddScoped<FtpConnectionPool>();
        services.AddScoped<IFtpClientFactory, DefaultFtpClientFactory>();
        services.AddScoped<IFtpService, FluentFtpService>();
        services.AddScoped<IFtpDirectoryService, FluentFtpDirectoryService>();

        return services;
    }

    public static IServiceCollection AddFtpServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetSection("FtpSettings") == null)
            throw new InvalidOperationException("FtpSettings configuration section is missing.");

        services.Configure<FtpSettings>(configuration.GetSection("FtpSettings"));

        services.AddScoped<FtpConnectionPool>();
        services.AddScoped<IFtpClientFactory, DefaultFtpClientFactory>();
        services.AddScoped<IFtpService, FluentFtpService>();
        services.AddScoped<IFtpDirectoryService, FluentFtpDirectoryService>();

        return services;
    }
}