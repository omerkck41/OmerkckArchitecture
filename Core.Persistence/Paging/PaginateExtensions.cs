namespace Core.Persistence.Paging;

public static class PaginateExtensions
{
    public static GetListResponse<T> ToGetListResponse<T>(this IPaginate<T> paginate)
    {
        return new GetListResponse<T>(paginate);
    }
}