namespace Core.ToolKit.Utilities.EventManagement;

public class CustomEventArgs : EventArgs
{
    /// <summary>
    /// Message associated with the event.
    /// </summary>
    public string Message { get; }

    public CustomEventArgs(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}