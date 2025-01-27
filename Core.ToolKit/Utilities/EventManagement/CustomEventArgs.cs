namespace Core.ToolKit.Utilities.EventManagement;

public class CustomEventArgs : EventArgs
{
    public string Message { get; set; }

    public CustomEventArgs(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public override string ToString()
    {
        return $"Message: {Message}";
    }
}