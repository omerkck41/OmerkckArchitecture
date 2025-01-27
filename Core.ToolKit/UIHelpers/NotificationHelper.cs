using System.Text;

namespace Core.ToolKit.UIHelpers;

public static class NotificationHelper
{
    /// <summary>
    /// Generates a standardized success message.
    /// </summary>
    /// <param name="message">The main message content.</param>
    /// <param name="details">Optional additional details.</param>
    /// <returns>Formatted success message string.</returns>
    public static string Success(string message, string? details = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        var builder = new StringBuilder();
        builder.AppendLine("[SUCCESS]");
        builder.AppendLine(message);

        if (!string.IsNullOrWhiteSpace(details))
        {
            builder.AppendLine("Details:");
            builder.AppendLine(details);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Generates a standardized error message.
    /// </summary>
    /// <param name="message">The main message content.</param>
    /// <param name="details">Optional additional details or stack trace.</param>
    /// <returns>Formatted error message string.</returns>
    public static string Error(string message, string? details = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        var builder = new StringBuilder();
        builder.AppendLine("[ERROR]");
        builder.AppendLine(message);

        if (!string.IsNullOrWhiteSpace(details))
        {
            builder.AppendLine("Details:");
            builder.AppendLine(details);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Generates a standardized warning message.
    /// </summary>
    /// <param name="message">The main message content.</param>
    /// <param name="details">Optional additional details.</param>
    /// <returns>Formatted warning message string.</returns>
    public static string Warning(string message, string? details = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        var builder = new StringBuilder();
        builder.AppendLine("[WARNING]");
        builder.AppendLine(message);

        if (!string.IsNullOrWhiteSpace(details))
        {
            builder.AppendLine("Details:");
            builder.AppendLine(details);
        }

        return builder.ToString();
    }

    public static async Task LogAsync(string message, LogLevel level = LogLevel.Info, string? details = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        if (!string.IsNullOrWhiteSpace(details))
        {
            logMessage += $"\nDetails: {details}";
        }

        await Task.Run(() => Console.WriteLine(logMessage));
        // Burada log mesajını dosyaya, veritabanına veya harici bir servise gönderme işlemi yapılabilir.
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
}