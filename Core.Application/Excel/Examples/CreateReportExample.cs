using Core.Application.Excel.Interfaces;

namespace Core.Application.Excel.Examples;

public class CreateReportExample
{
    public static void GenerateReport(IExcelBuilder builder)
    {
        builder.AddWorksheet("Report");
        builder.SetCellValue(1, 1, "Header 1");
        builder.SetCellValue(1, 2, "Header 2");
        builder.SetRangeValues(2, 1, 10, 2, "Data");
        builder.SaveAs("report.xlsx");
    }
}