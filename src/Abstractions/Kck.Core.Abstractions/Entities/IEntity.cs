namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Defines the primary identifier contract for all domain entities.
/// </summary>
/// <typeparam name="TId">The type of the entity's primary key (e.g. <see cref="Guid"/>, <see langword="int"/>).</typeparam>
public interface IEntity<TId>
{
    /// <summary>Gets or sets the primary key of the entity.</summary>
    TId Id { get; set; }
}
