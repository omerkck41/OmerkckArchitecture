namespace Core.Localization.Abstract;

public interface ICultureProvider
{
    /// <summary>
    /// O anki isteğe ait kültür bilgisini döner (örn. "tr-TR").
    /// </summary>
    string GetRequestCulture();
}