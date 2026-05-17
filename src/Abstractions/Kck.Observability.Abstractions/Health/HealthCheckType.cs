namespace Kck.Observability.Abstractions;

/// <summary>
/// Classifies the dependency category that a health check monitors, enabling targeted probe configuration (e.g. liveness vs readiness).
/// </summary>
public enum HealthCheckType
{
    /// <summary>Monitors a database connection or query capability.</summary>
    Database,

    /// <summary>Monitors a distributed or in-process cache.</summary>
    Cache,

    /// <summary>Monitors a message broker such as RabbitMQ or Azure Service Bus.</summary>
    MessageBroker,

    /// <summary>Monitors connectivity to an external HTTP/gRPC API.</summary>
    ExternalApi,

    /// <summary>Monitors access to a file system or blob storage.</summary>
    FileSystem,

    /// <summary>A user-defined health check that does not fit a predefined category.</summary>
    Custom
}
