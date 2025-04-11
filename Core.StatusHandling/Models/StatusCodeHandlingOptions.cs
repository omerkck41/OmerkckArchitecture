namespace Core.StatusHandling.Models;

/// <summary>
/// StatusCodeHandlingMiddleware için yapılandırma seçenekleri.
/// </summary>
public class StatusCodeHandlingOptions
{
    /// <summary>
    /// Belirli durum kodları için özel yönlendirme yolları.
    /// Key: Durum Kodu (int), Value: Yönlendirilecek Yol (string).
    /// Örn: { 404, "/NotFound" }, { 401, "/Login" }
    /// </summary>
    public Dictionary<int, string> RedirectPaths { get; set; } = new Dictionary<int, string>();

    /// <summary>
    /// İşleyicilerin UI bildirimleri (örn: Toast) tetikleyip tetiklemeyeceği.
    /// Gerçek bildirim mekanizması uygulamaya özel olacaktır.
    /// Bu ayar, handler'ların bildirimle ilgili işaretler (örn: cookie, header) bırakıp bırakmayacağını kontrol edebilir.
    /// </summary>
    public bool EnableNotifications { get; set; } = true; // Varsayılan olarak aktif

    // Gelecekte başka yapılandırma seçenekleri eklenebilir.
    // Örn: Loglama seviyeleri, belirli handler'ları devre dışı bırakma vb.
}