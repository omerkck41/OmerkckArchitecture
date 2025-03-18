namespace Core.Persistence.Paging;

public abstract class BasePageableModel<T>
{
    public PageMetaData MetaData { get; set; }
    public IList<T> Items { get; set; }

    protected BasePageableModel()
    {
        MetaData = new PageMetaData();
        Items = new List<T>();
    }
}