namespace Core.Persistence.Paging;

public class GetListResponse<T> : BasePageableModel<T>
{
    // Parametresiz yapıcı (opsiyonel kullanım için)
    public GetListResponse() : base() { }

    // IPaginate'den gelen veriyi alan yapıcı
    public GetListResponse(IPaginate<T> paginate) : base()
    {
        Items = paginate.Items;
        MetaData.Index = paginate.Index;
        MetaData.Size = paginate.Size;
        MetaData.TotalRecords = paginate.TotalRecords;
        MetaData.Pages = paginate.Pages;
        // İsteğe bağlı olarak validasyonu çalıştırabilirsiniz
        MetaData.Validate();
    }
}