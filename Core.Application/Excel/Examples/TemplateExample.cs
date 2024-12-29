using Core.Application.Excel.Interfaces;

namespace Core.Application.Excel.Examples;

public class TemplateExample
{
    public static void UseTemplate(IExcelTemplateManager templateManager)
    {
        templateManager.LoadTemplate("template.xlsx");
        templateManager.FillTemplateData(new Dictionary<string, object>
            {
                { "A1", "Header" },
                { "A2", "Data Row 1" },
                { "A3", "Data Row 2" }
            });
        templateManager.SaveTemplate("filled_template.xlsx");
    }
}