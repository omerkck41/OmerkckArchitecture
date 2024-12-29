
namespace Core.Persistence.Paging;

public class Paginate<T> : IPaginate<T>
{
    public Paginate(IEnumerable<T> source, int index, int size, int from)
    {
        if (from > index)
            throw new ArgumentException($"indexFrom: {from} > pageIndex: {index}, must indexFrom <= pageIndex");

        var queryable = source as IQueryable<T> ?? source.AsQueryable();

        Index = index;
        Size = size;
        From = from;
        Count = queryable.Count();
        Pages = (int)Math.Ceiling(Count / (double)Size);
        TotalRecords = Count;

        Items = queryable.Skip((Index - From) * Size).Take(Size).ToList();
    }

    public Paginate()
    {
        Items = [];
    }

    public int From { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public IList<T> Items { get; set; }
    public bool HasPrevious => Index - From > 0;
    public bool HasNext => Index - From + 1 < Pages;
    public int TotalRecords { get; set; }
    public bool IsFirstPage => Index == 1;
    public bool IsLastPage => Index == Pages;
}