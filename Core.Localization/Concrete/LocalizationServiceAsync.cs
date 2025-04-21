using Core.Localization.Abstract;
using Core.Localization.Models;
using Core.Localization.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Core.Localization.Concrete;

/// <summary>
/// Yerelleştirme servisi asenkron implementasyonu.
/// Thread-safe kültür yönetimi için AsyncLocal kullanır.
/// </summary>
public class LocalizationServiceAsync : ILocalizationServiceAsync
{
    private readonly LocalizationSourceManagerAsync _sourceManager;
    private readonly IOptionsMonitor<LocalizationOptions> _optionsMonitor;
    private readonly ILogger<LocalizationServiceAsync> _logger;
    private readonly List<CultureInfo> _supportedCultures;

    // Her async call context için ayrı saklama
    private static readonly AsyncLocal<CultureInfo> _ambientCulture = new();

    public LocalizationServiceAsync(
        LocalizationSourceManagerAsync sourceManager,
        IOptionsMonitor<LocalizationOptions> optionsMonitor,
        ILogger<LocalizationServiceAsync> logger)
    {
        _sourceManager = sourceManager;
        _optionsMonitor = optionsMonitor;
        _logger = logger;

        // Desteklenen kültürleri yükle
        _supportedCultures = _optionsMonitor.CurrentValue.SupportedCultures
            .Select(c => new CultureInfo(c))
            .ToList();

        // Başlangıç kültürünü AsyncLocal'a atıyoruz
        var defaultCulture = new CultureInfo(_optionsMonitor.CurrentValue.DefaultCulture);
        _ambientCulture.Value = defaultCulture;

        _logger.LogInformation(
            "LocalizationServiceAsync initialized with default culture: {Culture}",
            defaultCulture.Name);
    }

    public async Task<string> GetStringAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Empty localization key requested");
            return string.Empty;
        }

        var culture = GetCurrentCulture();
        var translations = await _sourceManager.LoadTranslationsForCultureAsync(culture);

        if (translations.TryGetValue(key, out var translation) && !string.IsNullOrEmpty(translation))
        {
            return translation;
        }

        _logger.LogDebug(
            "Translation not found for key: {Key} in culture: {Culture}",
            key, culture.Name);

        return _optionsMonitor.CurrentValue.ReturnKeyIfNotFound
            ? key
            : string.Empty;
    }

    public async Task<string> GetStringAsync(string key, params object[] args)
    {
        var format = await GetStringAsync(key);
        if (string.IsNullOrEmpty(format) || args.Length == 0)
            return format;

        try
        {
            var culture = GetCurrentCulture();
            return string.Format(culture, format, args);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Failed to format localized string with key: {Key}", key);
            return format;
        }
    }

    public CultureInfo GetCurrentCulture()
    {
        // Mevcut AsyncLocal değeri yoksa, varsayılan kültürü döndür
        return _ambientCulture.Value
               ?? new CultureInfo(_optionsMonitor.CurrentValue.DefaultCulture);
    }

    public void SetCurrentCulture(CultureInfo culture)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        if (!_supportedCultures.Any(c => c.Name.Equals(culture.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Unsupported culture requested: {Culture}", culture.Name);
            throw new ArgumentException($"Culture '{culture.Name}' is not supported.");
        }

        _ambientCulture.Value = culture;
        _logger.LogDebug("Current culture changed to: {Culture}", culture.Name);
    }

    public void SetCurrentCulture(string cultureName)
    {
        if (string.IsNullOrEmpty(cultureName))
            throw new ArgumentNullException(nameof(cultureName));

        try
        {
            var culture = new CultureInfo(cultureName);
            SetCurrentCulture(culture);
        }
        catch (CultureNotFoundException ex)
        {
            _logger.LogError(ex, "Invalid culture name: {CultureName}", cultureName);
            throw;
        }
    }

    public IEnumerable<CultureInfo> GetSupportedCultures() => _supportedCultures;
}