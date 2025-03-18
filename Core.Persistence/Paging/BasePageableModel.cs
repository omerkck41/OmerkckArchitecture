namespace Core.Persistence.Paging;

public abstract class BasePageableModel<T>
{
    public MetaData metaData { get; set; } = new MetaData();
    public IList<T> Items { get; set; } = [];

    public class MetaData
    {
        public int Index { get; set; }
        public int Size { get; set; }
        public int Count { get; set; }
        public int Pages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public int TotalRecords { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
    }
}