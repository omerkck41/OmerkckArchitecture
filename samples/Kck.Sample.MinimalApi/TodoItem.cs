using Kck.Core.Abstractions.Entities;

namespace Kck.Sample.MinimalApi;

public sealed class TodoItem : Entity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}
