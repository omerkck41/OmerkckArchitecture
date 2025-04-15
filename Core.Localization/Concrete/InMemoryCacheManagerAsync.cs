namespace Core.Localization.Concrete;

/// <summary>
/// Basit bir bellek içi önbellek yöneticisi
/// </summary>
public class InMemoryCacheManagerAsync
{
    private readonly Dictionary<string, CacheItem> _cache = [];
    private readonly object _lock = new();

    /// <summary>
    /// Önbellekten veri alır
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    /// <param name="key">Anahtar</param>
    /// <param name="value">Çıkış değeri</param>
    /// <returns>Veri bulundu mu?</returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var item) && !item.IsExpired)
            {
                value = (T)item.Value;
                return true;
            }

            value = default;
            return false;
        }
    }

    /// <summary>
    /// Önbelleğe veri ekler
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    /// <param name="key">Anahtar</param>
    /// <param name="value">Değer</param>
    /// <param name="expirationMinutes">Geçerlilik süresi (dakika)</param>
    public void Set<T>(string key, T value, int expirationMinutes)
    {
        lock (_lock)
        {
            var expirationTime = DateTime.UtcNow.AddMinutes(expirationMinutes);
            _cache[key] = new CacheItem(value!, expirationTime);
        }
    }

    /// <summary>
    /// Önbellekten veriyi siler
    /// </summary>
    /// <param name="key">Anahtar</param>
    public void Remove(string key)
    {
        lock (_lock)
        {
            _cache.Remove(key);
        }
    }

    /// <summary>
    /// Tüm önbelleği temizler
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
        }
    }

    /// <summary>
    /// Belirtilen önekle başlayan tüm anahtarları siler
    /// </summary>
    /// <param name="keyPrefix">Anahtar öneki</param>
    public void RemoveByPrefix(string keyPrefix)
    {
        lock (_lock)
        {
            var keysToRemove = _cache.Keys
                .Where(k => k.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }
    }

    private class CacheItem
    {
        public object Value { get; }
        public DateTime ExpirationTime { get; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTime;

        public CacheItem(object value, DateTime expirationTime)
        {
            Value = value;
            ExpirationTime = expirationTime;
        }
    }
}