namespace Core.Persistence.Dynamic;

public class Sort
{
    public required string Field { get; set; }
    public required string Dir { get; set; }
    public int Priority { get; set; } // Sıralama önceliği

    public Sort() { }

    public Sort(string field, string dir, int priority = 0)
    {
        Field = field;
        Dir = dir;
        Priority = priority;
    }
}