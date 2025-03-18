namespace Core.Persistence.Paging;

public class PageMetaData
{
    public int Index { get; set; }
    public int Size { get; set; }
    public int TotalRecords { get; set; }
    public int Pages { get; set; }

    public int From { get; set; }

    // Hesaplanan property'ler
    public bool HasPrevious => Index - From > 0;
    public bool HasNext => Index - From < Pages - 1;
    public bool IsFirstPage => Index - From == 0;
    public bool IsLastPage => Index - From == Pages - 1;

    // Validasyon metodu (isteğe bağlı)
    public void Validate()
    {
        if (Index < 1)
            throw new ArgumentException("Page index should be at least 1.");
        if (Size < 1)
            throw new ArgumentException("Page size should be at least 1.");
    }
}