using AutoMapper;

namespace Core.Persistence.Paging;

public class PaginateToGetListResponseConverter<TSource, TDestination> : ITypeConverter<IPaginate<TSource>, GetListResponse<TDestination>>
{
    public GetListResponse<TDestination> Convert(IPaginate<TSource> source, GetListResponse<TDestination> destination, ResolutionContext context)
    {
        // İlgili elemanları dönüştürmek için AutoMapper'ı kullanıyoruz
        var mappedItems = context.Mapper.Map<IList<TDestination>>(source.Items);

        var response = new GetListResponse<TDestination>
        {
            Items = mappedItems,
            MetaData = new PageMetaData
            {
                Index = source.Index,
                Size = source.Size,
                TotalRecords = source.TotalRecords,
                Pages = source.Pages
            }
        };
        return response;
    }
}