using System.Globalization;

namespace Core.ToolKit.Localization;

public static class DateTimeFormatter
{
    /// <summary>
    /// Formats a date according to the default culture with optional custom format.
    /// </summary>
    /// <param name="dateTime">DateTime to format.</param>
    /// <param name="customFormat">Optional custom format string.</param>
    /// <returns>Formatted date string.</returns>
    public static string FormatDate(DateTime dateTime, string? customFormat = null, CultureInfo? cultureInfo = null)
    {
        cultureInfo ??= new CultureInfo(LocalizationHelper.DefaultCulture);
        return customFormat == null
            ? dateTime.ToString(cultureInfo.DateTimeFormat.ShortDatePattern, cultureInfo)
            : dateTime.ToString(customFormat, cultureInfo);
    }

    /// <summary>
    /// Formats a time according to the default culture with optional custom format.
    /// </summary>
    /// <param name="dateTime">DateTime to format.</param>
    /// <param name="customFormat">Optional custom format string.</param>
    /// <returns>Formatted time string.</returns>
    public static string FormatTime(DateTime dateTime, string? customFormat = null, CultureInfo? cultureInfo = null)
    {
        cultureInfo ??= new CultureInfo(LocalizationHelper.DefaultCulture);
        return customFormat == null
            ? dateTime.ToString(cultureInfo.DateTimeFormat.ShortTimePattern, cultureInfo)
            : dateTime.ToString(customFormat, cultureInfo);
    }

    /// <summary>
    /// Formats a DateTime as both date and time according to the default culture with optional custom format.
    /// </summary>
    /// <param name="dateTime">DateTime to format.</param>
    /// <param name="customFormat">Optional custom format string.</param>
    /// <returns>Formatted date and time string.</returns>
    public static string FormatDateTime(DateTime dateTime, string? customFormat = null, CultureInfo? cultureInfo = null)
    {
        cultureInfo ??= new CultureInfo(LocalizationHelper.DefaultCulture);
        return customFormat == null
            ? dateTime.ToString(cultureInfo)
            : dateTime.ToString(customFormat, cultureInfo);
    }

    public static DateTime ParseDate(string dateString, CultureInfo? cultureInfo = null)
    {
        cultureInfo ??= new CultureInfo(LocalizationHelper.DefaultCulture);
        return DateTime.Parse(dateString, cultureInfo);
    }

    public static DateTime ParseDateTime(string dateTimeString, CultureInfo? cultureInfo = null)
    {
        cultureInfo ??= new CultureInfo(LocalizationHelper.DefaultCulture);
        return DateTime.Parse(dateTimeString, cultureInfo);
    }
}