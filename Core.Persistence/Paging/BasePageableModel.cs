namespace Core.Persistence.Paging;

public class BasePageableModel
{
    public MetaData metaData { get; set; } = new MetaData();
    public IEnumerable<object> Items { get; set; } = [];

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