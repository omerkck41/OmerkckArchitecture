namespace Core.Persistence.Paging;

public class GetListResponse<T> : BasePageableModel<T>
{
    public new IList<T> Items
    {
        get => _items ??= new List<T>();
        set => _items = value;
    }

    private IList<T>? _items;
}