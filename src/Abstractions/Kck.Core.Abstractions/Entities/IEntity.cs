namespace Kck.Core.Abstractions.Entities;

public interface IEntity<TId>
{
    TId Id { get; set; }
}
