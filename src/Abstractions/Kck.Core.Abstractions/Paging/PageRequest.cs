using System.Diagnostics;

namespace Kck.Core.Abstractions.Paging;

/// <summary>Immutable page descriptor used to request a specific slice of a result set.</summary>
/// <param name="Index">Zero-based page index (default 0).</param>
/// <param name="Size">Maximum number of items per page (default 10).</param>
[DebuggerDisplay("Page {Index}, Size: {Size}")]
public sealed record PageRequest(int Index = 0, int Size = 10);
