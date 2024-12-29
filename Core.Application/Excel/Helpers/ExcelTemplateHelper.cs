using ClosedXML.Excel;

namespace Core.Application.Excel.Helpers;

public static class ExcelTemplateHelper
{
    public static void FillTemplate(IXLWorksheet worksheet, Dictionary<string, object> data)
    {
        foreach (var entry in data)
        {
            worksheet.Cell(entry.Key).Value = (XLCellValue)entry.Value;
        }
    }
}