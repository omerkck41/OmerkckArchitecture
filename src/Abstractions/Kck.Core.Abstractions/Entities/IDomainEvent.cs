namespace Kck.Core.Abstractions.Entities;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
