using Core.Localization.Abstractions;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Core.Localization.Services;

/// <summary>
/// Assembly konumuna göre lokalizasyon kaynakları bulan ve yükleyen servis
/// </summary>
public class AssemblyBasedLocalizationManager : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _resources;
    private readonly ILogger<AssemblyBasedLocalizationManager> _logger;
    private readonly CultureInfo _defaultCulture = CultureInfo.GetCultureInfo("tr-TR");
    private readonly CultureInfo _fallbackCulture = CultureInfo.GetCultureInfo("en-US");

    /// <summary>
    /// Assembly bazlı lokalizasyon yöneticisi oluşturur
    /// </summary>
    public AssemblyBasedLocalizationManager(
        ILogger<AssemblyBasedLocalizationManager> logger,
        params Assembly[] assemblies)
    {
        _logger = logger;
        _resources = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        // Assemblyleri belirtilmediyse çalışan uygulamanın assembly'sini kullan
        var assembliesToScan = assemblies.Length > 0
            ? assemblies
            : new[] { Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly() };

        // Tüm assemblyleri tara
        foreach (var assembly in assembliesToScan)
        {
            try
            {
                LoadResourcesFromAssembly(assembly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Assembly taranırken hata oluştu: {Assembly}", assembly.FullName);
            }
        }
    }

    private void LoadResourcesFromAssembly(Assembly assembly)
    {
        // Assembly konumunu al
        var assemblyLocation = Path.GetDirectoryName(assembly.Location);
        if (string.IsNullOrEmpty(assemblyLocation))
        {
            _logger.LogWarning("Assembly konumu bulunamadı: {Assembly}", assembly.FullName);
            return;
        }

        _logger.LogInformation("Assembly taranıyor: {Assembly} konum: {Location}",
            assembly.GetName().Name, assemblyLocation);

        // Features klasörünü bul
        var featuresPath = Path.Combine(assemblyLocation, "Features");
        if (!Directory.Exists(featuresPath))
        {
            _logger.LogWarning("Features klasörü bulunamadı: {Path}", featuresPath);
            return;
        }

        // Features içindeki tüm alt klasörleri al
        var featureDirectories = Directory.GetDirectories(featuresPath);
        foreach (var featureDir in featureDirectories)
        {
            var featureName = Path.GetFileName(featureDir);
            var localesPath = Path.Combine(featureDir, "Resources", "Locales");

            if (Directory.Exists(localesPath))
            {
                LoadResourcesFromDirectory(localesPath, featureName);
            }
            else
            {
                // Alternatif yapıları ara
                var resourcesPath = Path.Combine(featureDir, "Resources");
                if (Directory.Exists(resourcesPath))
                {
                    var potentialLocalesDirs = Directory.GetDirectories(resourcesPath)
                        .Where(d => Path.GetFileName(d).Equals("Locales", StringComparison.OrdinalIgnoreCase) ||
                                   Path.GetFileName(d).Equals("Translations", StringComparison.OrdinalIgnoreCase) ||
                                   Path.GetFileName(d).Equals("i18n", StringComparison.OrdinalIgnoreCase));

                    foreach (var locDir in potentialLocalesDirs)
                    {
                        LoadResourcesFromDirectory(locDir, featureName);
                    }
                }
            }
        }
    }

    private void LoadResourcesFromDirectory(string directory, string featureName)
    {
        _logger.LogInformation("Dizinden kaynaklar yükleniyor: {Directory} özellik: {Feature}",
            directory, featureName);

        // YAML ve JSON dosyalarını ara
        var resourceFiles = Directory.GetFiles(directory, "*.yaml")
            .Concat(Directory.GetFiles(directory, "*.yml"))
            .Concat(Directory.GetFiles(directory, "*.json"));

        foreach (var file in resourceFiles)
        {
            try
            {
                ProcessResourceFile(file, featureName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kaynak dosyası işlenirken hata: {File}", file);
            }
        }
    }

    private void ProcessResourceFile(string filePath, string featureName)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var parts = fileName.Split('.');

        // Format: section.culture.extension (örn: users.tr.yml)
        if (parts.Length < 2)
        {
            _logger.LogWarning("Geçersiz dosya adı formatı: {FileName}. Beklenen format: section.culture.extension", fileName);
            return;
        }

        // İlk parça bölüm, ikinci parça kültür
        var sectionName = parts[0];
        var cultureName = parts[1];

        _logger.LogInformation("Kaynak dosyası işleniyor: {File} bölüm: {Section} kültür: {Culture}",
            filePath, sectionName, cultureName);

        // Dosya içeriğini oku
        var fileContent = File.ReadAllText(filePath);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        // Dosya içeriğini ayrıştır (YAML veya JSON)
        Dictionary<string, object> resourceDict;
        try
        {
            if (extension == ".json")
            {
                resourceDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    fileContent,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new Dictionary<string, object>();
            }
            else // .yaml or .yml
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                resourceDict = deserializer.Deserialize<Dictionary<string, object>>(fileContent);
            }

            // Dictionary'yi düzleştir ve kaynaklara ekle
            AddResourcesFromDictionary(cultureName, featureName, sectionName, resourceDict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dosya ayrıştırılırken hata: {File}", filePath);
        }
    }

    private void AddResourcesFromDictionary(
        string cultureName,
        string featureName,
        string sectionName,
        Dictionary<string, object> resourceDict,
        string prefix = "")
    {
        // Kültür için dictionary oluştur
        if (!_resources.ContainsKey(cultureName))
        {
            _resources[cultureName] = new Dictionary<string, Dictionary<string, string>>();
        }

        // Feature için dictionary oluştur
        if (!_resources[cultureName].ContainsKey(featureName))
        {
            _resources[cultureName][featureName] = new Dictionary<string, string>();
        }

        foreach (var pair in resourceDict)
        {
            var key = string.IsNullOrEmpty(prefix) ? pair.Key : $"{prefix}.{pair.Key}";

            if (pair.Value is Dictionary<object, object> nestedDict)
            {
                // İç içe dictionary'yi recurse et
                var convertedDict = nestedDict.ToDictionary(
                    k => k.Key.ToString()!,
                    v => v.Value);

                AddResourcesFromDictionary(cultureName, featureName, sectionName, convertedDict, key);
            }
            else if (pair.Value is IDictionary<string, object> typedDict)
            {
                // İç içe dictionary'yi recurse et
                var convertedDict = typedDict.ToDictionary(
                    k => k.Key,
                    v => v.Value);

                AddResourcesFromDictionary(cultureName, featureName, sectionName, convertedDict, key);
            }
            else
            {
                // Değeri string'e çevir
                var value = pair.Value?.ToString() ?? string.Empty;

                // Section prefix'i ekleyerek anahtarı kaydet
                var fullKey = $"{sectionName}.{key}";
                _resources[cultureName][featureName][fullKey] = value;

                _logger.LogDebug("Kaynak eklendi: Kültür={Culture}, Feature={Feature}, Anahtar={Key}, Değer={Value}",
                    cultureName, featureName, fullKey, value);
            }
        }
    }

    public async Task<string> GetStringAsync(string key, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _defaultCulture;
        var cultureName = culture.Name;

        // Önce belirtilen kültürde ara
        if (TryGetString(key, cultureName, out var result))
        {
            return result;
        }

        // Dil kodunda ara (en-US -> en)
        var languageCode = culture.TwoLetterISOLanguageName;
        if (TryGetString(key, languageCode, out result))
        {
            return result;
        }

        // Fallback kültüründe ara
        if (!culture.Equals(_fallbackCulture) && TryGetString(key, _fallbackCulture.Name, out result))
        {
            return result;
        }

        // Bulunamadıysa anahtar değerini döndür
        return key;
    }

    public async Task<string> GetStringAsync(string key, string section, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _defaultCulture;
        var fullKey = $"{section}.{key}";

        return await GetStringAsync(fullKey, culture, cancellationToken);
    }

    private bool TryGetString(string key, string cultureName, out string result)
    {
        result = key;

        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            foreach (var featureResources in cultureResources.Values)
            {
                if (featureResources.TryGetValue(key, out var value))
                {
                    result = value;
                    return true;
                }
            }
        }

        return false;
    }

    // ILocalizationService interface implementations
    public async Task<string> GetStringAsync(string key, params object[] args)
    {
        var format = await GetStringAsync(key);
        return string.Format(format, args);
    }

    public async Task<string> GetStringAsync(string key, CultureInfo culture, params object[] args)
    {
        var format = await GetStringAsync(key, culture);
        return string.Format(culture, format, args);
    }

    public async Task<string> GetStringAsync(string key, string section, params object[] args)
    {
        var format = await GetStringAsync(key, section);
        return string.Format(format, args);
    }

    public async Task<string> GetStringAsync(string key, string section, CultureInfo culture, params object[] args)
    {
        var format = await GetStringAsync(key, section, culture);
        return string.Format(culture, format, args);
    }

    public async Task<(bool success, string? value)> TryGetStringAsync(string key, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _defaultCulture;
        var cultureName = culture.Name;

        if (TryGetString(key, cultureName, out var result))
        {
            return (true, result);
        }

        var languageCode = culture.TwoLetterISOLanguageName;
        if (TryGetString(key, languageCode, out result))
        {
            return (true, result);
        }

        if (!culture.Equals(_fallbackCulture) && TryGetString(key, _fallbackCulture.Name, out result))
        {
            return (true, result);
        }

        return (false, null);
    }

    public async Task<(bool success, string? value)> TryGetStringAsync(string key, string section, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        var fullKey = $"{section}.{key}";
        return await TryGetStringAsync(fullKey, culture, cancellationToken);
    }

    public async Task<IDictionary<CultureInfo, string>> GetAllStringsAsync(string key, string? section = null, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<CultureInfo, string>();
        var fullKey = section == null ? key : $"{section}.{key}";

        foreach (var cultureName in _resources.Keys)
        {
            if (TryGetString(fullKey, cultureName, out var value))
            {
                result[CultureInfo.GetCultureInfo(cultureName)] = value;
            }
        }

        return result;
    }

    public async Task<IEnumerable<string>> GetAllKeysAsync(string? section = null, CancellationToken cancellationToken = default)
    {
        return await GetAllKeysAsync(_defaultCulture, section, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var cultureName = culture.Name;
        var keys = new HashSet<string>();

        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            foreach (var featureResources in cultureResources.Values)
            {
                foreach (var key in featureResources.Keys)
                {
                    if (section == null || key.StartsWith($"{section}."))
                    {
                        keys.Add(key);
                    }
                }
            }
        }

        return keys;
    }

    public async Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _defaultCulture;
        var cultureName = culture.Name;
        var sections = new HashSet<string>();

        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            foreach (var featureResources in cultureResources.Values)
            {
                foreach (var key in featureResources.Keys)
                {
                    var dotIndex = key.IndexOf('.');
                    if (dotIndex > 0)
                    {
                        var section = key.Substring(0, dotIndex);
                        sections.Add(section);
                    }
                }
            }
        }

        return sections;
    }

    public async Task<IEnumerable<CultureInfo>> GetSupportedCulturesAsync(CancellationToken cancellationToken = default)
    {
        return _resources.Keys.Select(c => CultureInfo.GetCultureInfo(c)).ToList();
    }

    public async Task<string> GetLocalizedAsync(string key, string section, CultureInfo? culture = null)
    {
        return await GetStringAsync(key, section, culture);
    }
}
