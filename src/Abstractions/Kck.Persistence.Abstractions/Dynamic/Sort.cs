namespace Kck.Persistence.Abstractions.Dynamic;

/// <summary>
/// Describes a single sort directive in a <see cref="DynamicQuery"/>, specifying the target field, direction, and relative priority.
/// </summary>
public sealed class Sort
{
    /// <summary>
    /// The name of the entity property to sort by.
    /// </summary>
    public required string Field { get; set; }

    /// <summary>
    /// Sort direction: <c>"asc"</c> for ascending or <c>"desc"</c> for descending.
    /// </summary>
    public required string Dir { get; set; }

    /// <summary>
    /// Zero-based priority used to resolve the order when multiple sort directives are present.
    /// Lower values are applied first.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Initializes an empty instance; <see cref="Field"/> and <see cref="Dir"/> must be set before use.
    /// </summary>
    public Sort() { }

    /// <summary>
    /// Initializes a new instance with the specified field, direction, and optional priority.
    /// </summary>
    /// <param name="field">The entity property name to sort by.</param>
    /// <param name="dir">Sort direction (<c>"asc"</c> or <c>"desc"</c>).</param>
    /// <param name="priority">Sort priority; defaults to <c>0</c>.</param>
    public Sort(string field, string dir, int priority = 0)
    {
        Field = field;
        Dir = dir;
        Priority = priority;
    }
}
