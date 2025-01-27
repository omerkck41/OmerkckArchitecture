using Core.Application.Excel.Builders;
using Core.Application.Excel.Interfaces;
using Core.Application.Excel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Excel.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExcelServices(this IServiceCollection services)
    {
        // Excel ile ilgili servisler
        services.AddScoped<IExcelBuilder, ExcelBuilder>();
        services.AddScoped<IExcelBuilderAsync, ExcelBuilderAsync>();
        services.AddScoped<IExcelTemplateManager, ExcelTemplateManager>();

        // Gerekirse daha fazla servis eklenebilir
        return services;
    }
}