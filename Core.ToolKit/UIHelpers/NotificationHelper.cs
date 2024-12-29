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

    /// <summary>
    /// Logs a notification message to the console.
    /// </summary>
    /// <param name="message">The notification message.</param>
    public static void Log(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }
}