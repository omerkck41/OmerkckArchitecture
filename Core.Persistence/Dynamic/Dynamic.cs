namespace Core.Persistence.Dynamic;

public class Dynamic
{
    public IEnumerable<Sort> Sort { get; set; } = Enumerable.Empty<Sort>();
    public Filter? Filter { get; set; }

    public Dynamic() { }

    public Dynamic(IEnumerable<Sort>? sort, Filter? filter)
    {
        Sort = sort ?? Enumerable.Empty<Sort>();
        Filter = filter;
    }
}