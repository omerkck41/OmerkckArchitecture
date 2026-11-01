namespace Kck.Persistence.Abstractions.Auditing;

/// <summary>
/// Supplies the user that the audit fields record (<c>CreatedBy</c>, <c>ModifiedBy</c>, <c>DeletedBy</c>).
/// Optional: when no implementation is registered, or <see cref="CurrentUser"/> is <see langword="null"/> or empty,
/// the audit fields that name a user are left as they are.
/// </summary>
/// <remarks>
/// The Entity Framework provider resolves it on every save from the service provider that created the DbContext, so a
/// scoped (per-request) implementation works with a regular (non-pooled) context. A pooled context is created from the
/// root provider: register a singleton implementation there (for example one that reads <c>IHttpContextAccessor</c>).
/// </remarks>
public interface IAuditUserProvider
{
    /// <summary>
    /// Name or identifier of the current user; <see langword="null"/> or empty when there is no user to record
    /// (for example a background job).
    /// </summary>
    string? CurrentUser { get; }
}
