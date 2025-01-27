namespace Core.ToolKit.Utilities.EventManagement;

public static class ToolkitEvents
{
    public static event EventHandler<CustomEventArgs>? ProcessStarted;
    public static event EventHandler<CustomEventArgs>? ProcessCompleted;
    public static event EventHandler<CustomEventArgs>? ProcessFailed;

    public static void TriggerProcessStarted(object sender, string message)
    {
        ProcessStarted?.Invoke(sender, new CustomEventArgs(message));
    }

    public static void TriggerProcessCompleted(object sender, string message)
    {
        ProcessCompleted?.Invoke(sender, new CustomEventArgs(message));
    }

    public static void TriggerProcessFailed(object sender, string message)
    {
        ProcessFailed?.Invoke(sender, new CustomEventArgs(message));
    }

    public static void SubscribeProcessStarted(EventHandler<CustomEventArgs> handler)
    {
        ProcessStarted += handler;
    }

    public static void UnsubscribeProcessStarted(EventHandler<CustomEventArgs> handler)
    {
        ProcessStarted -= handler;
    }
}