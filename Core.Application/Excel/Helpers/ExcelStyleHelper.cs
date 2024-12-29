using ClosedXML.Excel;

namespace Core.Application.Excel.Helpers;

public static class ExcelStyleHelper
{
    public static void ApplyHeaderStyle(IXLWorksheet worksheet, int row, int startColumn, int endColumn)
    {
        var range = worksheet.Range(row, startColumn, row, endColumn);
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.LightGray;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }
}