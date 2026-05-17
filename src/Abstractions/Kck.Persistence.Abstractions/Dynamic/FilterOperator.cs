namespace Kck.Persistence.Abstractions.Dynamic;

/// <summary>
/// Enumerates the comparison and string-matching operators available for dynamic filter predicates.
/// </summary>
public enum FilterOperator
{
    /// <summary>Equality check: field == value.</summary>
    Eq,

    /// <summary>Inequality check: field != value.</summary>
    Neq,

    /// <summary>Less-than comparison: field &lt; value.</summary>
    Lt,

    /// <summary>Less-than-or-equal comparison: field &lt;= value.</summary>
    Lte,

    /// <summary>Greater-than comparison: field &gt; value.</summary>
    Gt,

    /// <summary>Greater-than-or-equal comparison: field &gt;= value.</summary>
    Gte,

    /// <summary>String contains check: field.Contains(value).</summary>
    Contains,

    /// <summary>String prefix check: field.StartsWith(value).</summary>
    StartsWith,

    /// <summary>String suffix check: field.EndsWith(value).</summary>
    EndsWith,

    /// <summary>Null check: field == null.</summary>
    IsNull,

    /// <summary>Not-null check: field != null.</summary>
    IsNotNull
}
