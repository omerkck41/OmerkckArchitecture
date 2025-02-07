using Core.Infrastructure.Excel.Builders;
using Core.Infrastructure.Excel.Interfaces;
using Core.Infrastructure.Excel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Excel.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExcelServices(this IServiceCollection services)
    {
        // Excel ile ilgili servisler
        services.AddScoped<IExcelBuilder, ExcelBuilder>();
        services.AddScoped<IExcelBuilderAsync, ExcelBuilderAsync>();
        services.AddScoped<IExcelTemplateManager, ExcelTemplateManager>();

        return services;
    }
}