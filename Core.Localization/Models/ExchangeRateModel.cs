namespace Core.Localization.Models;

/// <summary>
/// Döviz kuru bilgisi
/// </summary>
public class ExchangeRateModel
{
    /// <summary>
    /// Kaynak para birimi
    /// </summary>
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Döviz kurları
    /// </summary>
    public Dictionary<string, decimal> Rates { get; set; } = [];

    /// <summary>
    /// Son güncelleme tarihi
    /// </summary>
    public DateTime LastUpdated { get; set; }
}