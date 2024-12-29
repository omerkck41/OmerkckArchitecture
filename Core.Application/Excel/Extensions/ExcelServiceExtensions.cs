using Core.Application.Excel.Builders;
using Core.Application.Excel.Interfaces;
using Core.Application.Excel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Excel.Extensions;

public static class ExcelServiceExtensions
{
    public static IServiceCollection AddExcelServices(this IServiceCollection services)
    {
        services.AddScoped<IExcelBuilder, ExcelBuilder>();
        services.AddScoped<IExcelTemplateManager, ExcelTemplateManager>();
        return services;
    }
}