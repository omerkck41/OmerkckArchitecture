namespace Core.Application.Excel.Interfaces;

public interface IExcelBuilderAsync
{
    Task AddWorksheetAsync(string sheetName);
    Task SelectWorksheetAsync(string sheetName);
    Task SetCellValueAsync(int row, int column, object value);
    Task CopyCellAsync(int sourceRow, int sourceColumn, int targetRow, int targetColumn, bool includeFormatting = true);
    Task SetRangeValuesAsync(int startRow, int startColumn, int endRow, int endColumn, object value);
    Task CopyRowAsync(int sourceRow, int targetRow, bool includeFormatting = true);
    Task CopyRangeAsync(int startRow, int startColumn, int endRow, int endColumn, int targetRow, int targetColumn, bool includeFormatting = true);
    Task SetColumnWidthAsync(int column, double width);
    Task SetPageOrientationAsync(bool isLandscape);
    Task SetPageMarginsAsync(double top, double bottom, double left, double right);
    Task SetPageSizeAsync(string pageSize);
    Task ClearContentAsync(string? range);

    Task BatchInsertRowsAsync(int startRow, int numberOfRows, object defaultValue);
    Task BatchInsertColumnsAsync(int startColumn, int numberOfColumns, object defaultValue);
    Task BatchInsertDataAsync<T>(string startCell, IEnumerable<T> data, int batchSize = 1000);

    Task AddImageAsync(string imagePath, int startRow, int startColumn, int endRow, int endColumn);
    Task AddImageScaledAsync(string imagePath, int startRow, int startColumn, int scalePercentage);
    Task RemoveImageAsync(string imageName);

    Task SetDataListAsync<T>(string startCell, List<T> data);
    Task SetDataListAsync<T>(int startRow, int startColumn, List<T> data);

    Task ExportToExcelAsync<T>(string filePath, List<T> data);
    Task<List<T>> ImportToEntityAsync<T>(string filePath);

    Task SaveAsAsync(string filePath, string format = "xlsx", bool exportToPdf = false);
    Task SaveAsCsvAsync(string filePath);
}