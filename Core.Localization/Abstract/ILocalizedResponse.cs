namespace Core.Localization.Abstract;

/// <summary>
/// Represents a localized response containing a message key and its localized message.
/// </summary>
public interface ILocalizedResponse
{
    /// <summary>
    /// Gets or sets the localization key (e.g., "UserAuth.LoginSuccess").
    /// </summary>
    string? MessageKey { get; set; }

    /// <summary>
    /// Gets or sets the localized message.
    /// </summary>
    string? Message { get; set; }
}
