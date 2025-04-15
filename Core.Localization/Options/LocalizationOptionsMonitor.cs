using Core.Localization.Models;
using Microsoft.Extensions.Options;

namespace Core.Localization.Options;

/// <summary>
/// Monitors changes to <see cref="LocalizationOptions"/> and triggers events when options change.
/// </summary>
public class LocalizationOptionsMonitor
{
    private LocalizationOptions _currentOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationOptionsMonitor"/> class.
    /// </summary>
    /// <param name="optionsMonitor">The options monitor to track changes.</param>
    public LocalizationOptionsMonitor(IOptionsMonitor<LocalizationOptions> optionsMonitor)
    {
        _currentOptions = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(newOptions =>
        {
            // Trigger actions when new settings arrive.
            _currentOptions = newOptions;
            // For example, clearing cache or restarting resource providers.
            OnOptionsChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    /// <summary>
    /// Gets the current localization options.
    /// </summary>
    public LocalizationOptions CurrentOptions => _currentOptions;

    /// <summary>
    /// Event triggered when localization options change.
    /// </summary>
    public event EventHandler? OnOptionsChanged;
}
