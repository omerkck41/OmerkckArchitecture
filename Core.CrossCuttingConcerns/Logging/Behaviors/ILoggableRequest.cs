namespace Core.CrossCuttingConcerns.Logging.Behaviors;

public interface ILoggableRequest
{
    string RequestDetails { get; }
    Dictionary<string, object>? Metadata { get; }
}