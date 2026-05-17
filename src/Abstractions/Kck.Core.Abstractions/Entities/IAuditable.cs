namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Audit trail contract. Automatically populated by <c>AuditInterceptor</c> on EF Core operations.
/// </summary>
public interface IAuditable
{
    /// <summary>Username or identifier of the creator.</summary>
    string CreatedBy { get; set; }

    /// <summary>UTC timestamp when the entity was created.</summary>
    DateTime CreatedDate { get; set; }

    /// <summary>Username or identifier of the last modifier. <see langword="null"/> if never modified.</summary>
    string? ModifiedBy { get; set; }

    /// <summary>UTC timestamp of the last modification. <see langword="null"/> if never modified.</summary>
    DateTime? ModifiedDate { get; set; }
}
