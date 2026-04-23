using Kck.Documents.Abstractions;
using Kck.Documents.ClosedXml;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckDocumentsClosedXmlServiceCollectionExtensions
{
    public static IServiceCollection AddKckDocumentsClosedXml(this IServiceCollection services)
    {
        services.TryAddSingleton<IExcelService, ClosedXmlExcelService>();
        services.TryAddSingleton<ICsvExporter, ClosedXmlCsvExporter>();
        return services;
    }
}
