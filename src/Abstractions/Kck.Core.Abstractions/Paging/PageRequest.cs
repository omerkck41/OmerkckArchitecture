using System.Diagnostics;

namespace Kck.Core.Abstractions.Paging;

[DebuggerDisplay("Page {Index}, Size: {Size}")]
public sealed record PageRequest(int Index = 0, int Size = 10);
