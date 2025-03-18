
using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Persistence.Paging;

public class Paginate<T> : IPaginate<T>
{
    public Paginate(IEnumerable<T> source, int index, int size, int from)
    {
        if (from > index)
            throw new CustomException($"indexFrom: {from} > pageIndex: {index}, must indexFrom <= pageIndex");

        Index = index;
        Size = size;
        From = from;



        if (source is IQueryable<T> queryable)
        {
            Count = queryable.Count();
            Items = queryable.Skip((Index - From) * Size).Take(Size).ToList();
        }
        else
        {
            T[] enumerable = source as T[] ?? source.ToArray();
            Count = enumerable.Count();
            Items = enumerable.Skip((Index - From) * Size).Take(Size).ToList();
        }

        TotalRecords = Count;
        Pages = (int)Math.Ceiling(Count / (double)Size);
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
    public int TotalRecords { get; set; }
    public bool HasPrevious => Index - From > 0;
    public bool HasNext => Index - From < Pages - 1;
    public bool IsFirstPage => Index - From == 0;
    public bool IsLastPage => Index - From == Pages - 1;
}