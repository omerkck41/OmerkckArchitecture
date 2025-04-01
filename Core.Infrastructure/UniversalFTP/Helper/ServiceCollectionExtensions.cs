using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Infrastructure.UniversalFTP.Factories;
using Core.Infrastructure.UniversalFTP.Services.Implementations;
using Core.Infrastructure.UniversalFTP.Services.Interfaces;
using Core.Infrastructure.UniversalFTP.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Core.Infrastructure.UniversalFTP.Helper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUniversalFtpServices(this IServiceCollection services, FtpSettings settings)
    {
        if (settings == null)
            throw new CustomArgumentException(nameof(settings), "FtpSettings cannot be null.");

        // FtpSettings'i IOptions<FtpSettings> olarak kaydet
        services.Configure<FtpSettings>(opts =>
        {
            opts.Host = settings.Host;
            opts.Port = settings.Port;
            opts.Username = settings.Username;
            opts.Password = settings.Password;
            opts.UseSsl = settings.UseSsl;
            opts.RetryCount = settings.RetryCount;
            opts.TimeoutInSeconds = settings.TimeoutInSeconds;
        });

        // FtpConnectionPool'u kaydet (FtpSettings'e bağımlı)
        services.AddSingleton<FtpConnectionPool>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<FtpSettings>>();
            return new FtpConnectionPool((IOptions<FtpSettings>)options.Value);
        });

        // TransactionContext'i kaydet (Scoped olarak)
        services.AddScoped<TransactionContext>();

        services.AddScoped<IFtpClientFactory, DefaultFtpClientFactory>();
        services.AddScoped<IFtpService, FluentFtpService>();
        services.AddScoped<IFtpDirectoryService, FluentFtpDirectoryService>();

        return services;
    }

    public static IServiceCollection AddFtpServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetSection("FtpSettings") == null)
            throw new CustomInvalidOperationException("FtpSettings configuration section is missing.");

        // FtpSettings'i JSON'dan oku ve IOptions<FtpSettings> olarak kaydet
        services.Configure<FtpSettings>(configuration.GetSection("FtpSettings"));

        // FtpConnectionPool'u kaydet (FtpSettings'e bağımlı)
        services.AddSingleton<FtpConnectionPool>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<FtpSettings>>().Value;
            return new FtpConnectionPool((IOptions<FtpSettings>)settings);
        });

        // TransactionContext'i kaydet (Scoped olarak)
        services.AddScoped<TransactionContext>();

        services.AddScoped<IFtpClientFactory, DefaultFtpClientFactory>();
        services.AddScoped<IFtpService, FluentFtpService>();
        services.AddScoped<IFtpDirectoryService, FluentFtpDirectoryService>();

        return services;
    }
}