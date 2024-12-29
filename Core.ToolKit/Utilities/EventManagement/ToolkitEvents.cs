namespace Core.ToolKit.Utilities.EventManagement;

public static class ToolkitEvents
{
    /// <summary>
    /// Event triggered when a process starts, including metadata.
    /// </summary>
    public static event EventHandler<CustomEventArgs>? ProcessStarted;

    /// <summary>
    /// Event triggered when a process completes, including metadata.
    /// </summary>
    public static event EventHandler<CustomEventArgs>? ProcessCompleted;

    /// <summary>
    /// Triggers the ProcessStarted event with a message.
    /// </summary>
    public static void TriggerProcessStarted(string message)
    {
        ProcessStarted?.Invoke(null, new CustomEventArgs(message));
    }

    /// <summary>
    /// Triggers the ProcessCompleted event with a message.
    /// </summary>
    public static void TriggerProcessCompleted(string message)
    {
        ProcessCompleted?.Invoke(null, new CustomEventArgs(message));
    }
}