namespace Kck.Observability.Abstractions;

public enum HealthCheckType
{
    Database,
    Cache,
    MessageBroker,
    ExternalApi,
    FileSystem,
    Custom
}
