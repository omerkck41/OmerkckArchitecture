using Core.Localization.Abstract;
using Core.Localization.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Localization.Services;

/// <summary>
/// Lokalizasyon servislerini başlatan hosted servis
/// </summary>
public class LocalizationInitializationServiceAsync : IHostedService
{
    private readonly IEnumerable<ILocalizationSourceAsync> _sources;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LocalizationInitializationServiceAsync> _logger;

    /// <summary>
    /// LocalizationInitializationService constructor
    /// </summary>
    public LocalizationInitializationServiceAsync(
        IEnumerable<ILocalizationSourceAsync> sources,
        IServiceProvider serviceProvider,
        ILogger<LocalizationInitializationServiceAsync> logger)
    {
        _sources = sources;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting localization initialization");

        // Kaynakları başlat
        foreach (var source in _sources)
        {
            try
            {
                _logger.LogDebug("Initializing localization source: {SourceName}", source.Name);
                var result = await source.InitializeAsync();

                if (result)
                {
                    _logger.LogInformation("Successfully initialized localization source: {SourceName}", source.Name);
                }
                else
                {
                    _logger.LogWarning("Failed to initialize localization source: {SourceName}", source.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing localization source: {SourceName}", source.Name);
            }
        }

        // String uzantıları için lokalizasyon servisini ayarla
        try
        {
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationServiceAsync>();
            StringExtensions.SetLocalizationService(localizationService);
            _logger.LogDebug("Set localization service for string extensions");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set localization service for string extensions");
        }

        _logger.LogInformation("Localization initialization completed");
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping localization services");
        await Task.CompletedTask;
    }
}
