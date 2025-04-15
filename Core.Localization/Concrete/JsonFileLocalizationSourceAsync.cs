using Core.Localization.Abstract;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace Core.Localization.Concrete;

/// <summary>
/// JSON dosyalarından çeviri verilerini asenkron şekilde yükleyen ve yöneten kaynak sağlayıcı.
/// ILocalizationSource arayüzündeki tüm metodları implement eder.
/// </summary>
public class JsonFileLocalizationSourceAsync : ILocalizationSourceAsync
{
    private readonly string _resourceDirectory;
    private readonly ILogger<JsonFileLocalizationSourceAsync> _logger;
    private readonly Dictionary<string, Dictionary<string, string>> _translations;

    public JsonFileLocalizationSourceAsync(string resourceDirectory, ILogger<JsonFileLocalizationSourceAsync> logger)
    {
        _resourceDirectory = resourceDirectory;
        _logger = logger;
        _translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kaynağın adı (örneğin, "JsonFileLocalizationSource").
    /// </summary>
    public string Name => "JsonFileLocalizationSource";

    /// <summary>
    /// Belirtilen kültür için çeviri anahtar-değer çiftlerini asenkron olarak getirir.
    /// </summary>
    public Task<IDictionary<string, string>> GetTranslationsAsync(CultureInfo culture)
    {
        if (_translations.TryGetValue(culture.Name, out var translations))
        {
            return Task.FromResult<IDictionary<string, string>>(translations);
        }

        // Eğer belirtilen kültüre ait çeviriler bulunamazsa boş bir sözlük döndür.
        return Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string>());
    }

    /// <summary>
    /// Kaynağı başlatır; kaynak dizinindeki tüm JSON dosyalarını asenkron olarak yükler.
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            if (!Directory.Exists(_resourceDirectory))
            {
                _logger.LogWarning("Resource directory not found: {ResourceDirectory}", _resourceDirectory);
                return false;
            }

            var files = Directory.GetFiles(_resourceDirectory, "*.json");

            // Her dosya için çevirileri asenkron şekilde yükle.
            var tasks = new List<Task>();
            foreach (var file in files)
            {
                tasks.Add(LoadTranslationsFromFileAsync(file));
            }
            await Task.WhenAll(tasks);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing JsonFileLocalizationSource");
            return false;
        }
    }

    /// <summary>
    /// Belirtilen dosyadan çevirileri yükler.
    /// Beklenen dosya adlandırma: {culture}.json (örn: en-US.json)
    /// </summary>
    private async Task LoadTranslationsFromFileAsync(string filePath)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            // Dosya ismi kültür kodunu temsil ediyor kabul ediyoruz.
            var culture = new CultureInfo(fileName);

            var json = await File.ReadAllTextAsync(filePath);
            var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (translations != null)
            {
                _translations[culture.Name] = translations;
                _logger.LogInformation("Loaded translations for culture: {Culture}", culture.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading translations from file: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Kaynak çevirilerini yeniler; mevcut verileri temizleyip yeniden yükleme yapar.
    /// </summary>
    public async Task RefreshAsync()
    {
        try
        {
            _translations.Clear();
            await InitializeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing translations in JsonFileLocalizationSource");
        }
    }

    /// <summary>
    /// Desteklenen kültürleri döndürür; yüklenmiş olan tüm kültür kodlarını CultureInfo nesnesine çevirir.
    /// </summary>
    public IEnumerable<CultureInfo> GetSupportedCultures()
    {
        foreach (var cultureName in _translations.Keys)
        {
            yield return new CultureInfo(cultureName);
        }
    }
}