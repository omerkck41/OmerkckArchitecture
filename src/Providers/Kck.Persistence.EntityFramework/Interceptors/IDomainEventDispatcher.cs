using Kck.Core.Abstractions.Entities;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// Dispatches domain events collected from entities after persistence operations.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all collected domain events.
    /// </summary>
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
