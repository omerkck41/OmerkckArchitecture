namespace Kck.Persistence.Abstractions.Dynamic;

public sealed class Sort
{
    public required string Field { get; set; }
    public required string Dir { get; set; }
    public int Priority { get; set; }

    public Sort() { }

    public Sort(string field, string dir, int priority = 0)
    {
        Field = field;
        Dir = dir;
        Priority = priority;
    }
}
