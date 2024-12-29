using Core.Application.UniversalFTP.Factories;
using Core.Application.UniversalFTP.Services.Implementations;
using Core.Application.UniversalFTP.Services.Interfaces;
using Core.Application.UniversalFTP.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Core.Application.UniversalFTP.Helper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUniversalFtpServices(this IServiceCollection services, FtpSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings), "FtpSettings cannot be null.");

        // IOptions<FtpSettings> olarak ayarlanıyor
        services.AddSingleton<IOptions<FtpSettings>>(Options.Create(settings));
        services.AddSingleton<FtpConnectionPool>();
        services.AddScoped<IFtpClientFactory, DefaultFtpClientFactory>();
        services.AddScoped<FluentFtpService>();
        services.AddScoped<FluentFtpDirectoryService>();

        return services;
    }

    public static IServiceCollection AddFtpServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetSection("FtpSettings") == null)
            throw new InvalidOperationException("FtpSettings configuration section is missing.");

        services.Configure<FtpSettings>(configuration.GetSection("FtpSettings"));
        services.AddSingleton<FtpConnectionPool>();
        services.AddSingleton<IFtpClientFactory, DefaultFtpClientFactory>();
        services.AddTransient<IFtpService, FluentFtpService>();
        services.AddTransient<IFtpDirectoryService, FluentFtpDirectoryService>();

        return services;
    }

}