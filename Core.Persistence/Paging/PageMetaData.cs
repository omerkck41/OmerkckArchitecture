namespace Core.Persistence.Paging;

public class PageMetaData
{
    public int Index { get; set; }
    public int Size { get; set; }
    public int TotalRecords { get; set; }
    public int Pages { get; set; }

    // Hesaplanan property'ler
    public bool HasPrevious => Index > 1;
    public bool HasNext => Index < Pages;
    public bool IsFirstPage => Index == 1;
    public bool IsLastPage => Index == Pages;

    // Validasyon metodu (isteğe bağlı)
    public void Validate()
    {
        if (Index < 1)
            throw new ArgumentException("Page index should be at least 1.");
        if (Size < 1)
            throw new ArgumentException("Page size should be at least 1.");
    }
}