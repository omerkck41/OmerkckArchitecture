namespace Core.Infrastructure.Excel.Interfaces;

public interface IExcelStyler
{
    void ApplyHeaderStyle(string sheetName, int row, int startColumn, int endColumn);
    void ApplyCellStyle(string sheetName, int row, int column, string styleName);
    void SetCellFont(string sheetName, int row, int column, string fontName, int fontSize, bool isBold = false);
}