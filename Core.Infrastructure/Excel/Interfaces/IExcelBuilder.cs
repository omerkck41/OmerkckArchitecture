namespace Core.Infrastructure.Excel.Interfaces;

public interface IExcelBuilder
{
    void AddWorksheet(string sheetName);
    void SelectWorksheet(string sheetName);
    void SetCellValue(int row, int column, object value);
    void CopyCell(int sourceRow, int sourceColumn, int targetRow, int targetColumn, bool includeFormatting = true);
    void SetRangeValues(int startRow, int startColumn, int endRow, int endColumn, object value);
    void CopyRow(int sourceRow, int targetRow, bool includeFormatting = true);
    void CopyRange(int startRow, int startColumn, int endRow, int endColumn, int targetRow, int targetColumn, bool includeFormatting = true);
    void SetColumnWidth(int column, double width);
    void SetPageOrientation(bool isLandscape);
    void SetPageMargins(double top, double bottom, double left, double right);
    void SetPageSize(string pageSize);
    void ClearContent(string? range);

    void BatchInsertRows(int startRow, int numberOfRows, object defaultValue);
    void BatchInsertColumns(int startColumn, int numberOfColumns, object defaultValue);
    void BatchInsertData<T>(string startCell, IEnumerable<T> data, int batchSize = 1000);

    void AddImage(string imagePath, int startRow, int startColumn, int endRow, int endColumn);
    void AddImageScaled(string imagePath, int startRow, int startColumn, int scalePercentage);
    void RemoveImage(string imageName);

    void SetDataList<T>(string startCell, List<T> data);
    void SetDataList<T>(int startRow, int startColumn, List<T> data);

    void ExportToExcel<T>(string filePath, List<T> data);
    Task<List<T>> ImportToEntityAsync<T>(string filePath);

    void SaveAs(string filePath, string format = "xlsx", bool exportToPdf = false);
    void SaveAsCsv(string filePath);
}