using ClosedXML.Excel;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Infrastructure.Excel.Interfaces;
using System.Reflection;
using System.Text;

namespace Core.Infrastructure.Excel.Builders;

public class ExcelBuilder : IExcelBuilder
{
    private readonly XLWorkbook _workbook;
    private IXLWorksheet _currentWorksheet;

    public ExcelBuilder()
    {
        _workbook = new XLWorkbook();
        _currentWorksheet = _workbook.AddWorksheet("Sheet1"); // Varsayılan bir çalışma sayfası ekleniyor
    }

    public void AddWorksheet(string sheetName)
    {
        _currentWorksheet = _workbook.Worksheets.Add(sheetName);
    }

    public void SelectWorksheet(string sheetName)
    {
        _currentWorksheet = _workbook.Worksheet(sheetName);
    }

    public void SetCellValue(int row, int column, object value)
    {
        EnsureWorksheetSelected();
        _currentWorksheet.Cell(row, column).Value = (XLCellValue)value;
    }

    public void CopyCell(int sourceRow, int sourceColumn, int targetRow, int targetColumn, bool includeFormatting = true)
    {
        EnsureWorksheetSelected();
        var source = _currentWorksheet.Cell(sourceRow, sourceColumn);
        var target = _currentWorksheet.Cell(targetRow, targetColumn);
        target.Value = source.Value;
        if (includeFormatting)
            target.Style = source.Style;
    }

    public void SetRangeValues(int startRow, int startColumn, int endRow, int endColumn, object value)
    {
        EnsureWorksheetSelected();
        for (int i = startRow; i <= endRow; i++)
        {
            for (int j = startColumn; j <= endColumn; j++)
            {
                _currentWorksheet.Cell(i, j).Value = (XLCellValue)value;
            }
        }
    }

    public void CopyRow(int sourceRow, int targetRow, bool includeFormatting = true)
    {
        EnsureWorksheetSelected();

        var source = _currentWorksheet.Row(sourceRow);
        var target = _currentWorksheet.Row(targetRow);

        foreach (var sourceCell in source.CellsUsed())
        {
            var targetCell = target.Cell(sourceCell.Address.ColumnNumber);
            targetCell.Value = sourceCell.Value;

            if (includeFormatting)
            {
                targetCell.Style = sourceCell.Style;
            }
        }
    }

    public void CopyRange(int startRow, int startColumn, int endRow, int endColumn, int targetRow, int targetColumn, bool includeFormatting = true)
    {
        EnsureWorksheetSelected();
        var range = _currentWorksheet.Range(startRow, startColumn, endRow, endColumn);
        range.CopyTo(_currentWorksheet.Cell(targetRow, targetColumn));
    }

    public void SetColumnWidth(int column, double width)
    {
        EnsureWorksheetSelected();
        _currentWorksheet.Column(column).Width = width;
    }

    public void SetPageOrientation(bool isLandscape)
    {
        EnsureWorksheetSelected();
        _currentWorksheet.PageSetup.PageOrientation = isLandscape ? XLPageOrientation.Landscape : XLPageOrientation.Portrait;
    }

    public void SetPageMargins(double top, double bottom, double left, double right)
    {
        EnsureWorksheetSelected();
        _currentWorksheet.PageSetup.Margins.Top = top;
        _currentWorksheet.PageSetup.Margins.Bottom = bottom;
        _currentWorksheet.PageSetup.Margins.Left = left;
        _currentWorksheet.PageSetup.Margins.Right = right;
    }

    public void SetPageSize(string pageSize)
    {
        EnsureWorksheetSelected();
        _currentWorksheet.PageSetup.PaperSize = pageSize.ToLower() switch
        {
            "a4" => XLPaperSize.A4Paper,
            "letter" => XLPaperSize.LetterPaper,
            _ => XLPaperSize.A4Paper
        };
    }

    public void ClearContent(string? range)
    {
        EnsureWorksheetSelected();
        if (!string.IsNullOrWhiteSpace(range))
        {
            _currentWorksheet.Range(range).Clear(XLClearOptions.Contents);
        }
        else
        {
            _currentWorksheet.Clear(XLClearOptions.Contents);
        }
    }

    public void BatchInsertRows(int startRow, int numberOfRows, object defaultValue)
    {
        EnsureWorksheetSelected();

        if (_currentWorksheet == null)
            throw new CustomInvalidOperationException("Worksheet is not selected.");

        var row = _currentWorksheet.Row(startRow) ?? throw new CustomInvalidOperationException($"Row {startRow} does not exist in the worksheet.");
        row.InsertRowsAbove(numberOfRows);

        var lastColumn = _currentWorksheet.LastColumnUsed()?.ColumnNumber();
        if (lastColumn != null)
            SetRangeValues(startRow, 1, startRow + numberOfRows - 1, lastColumn.Value, defaultValue);
        else
            throw new CustomInvalidOperationException("No columns found in the worksheet.");
    }

    public void BatchInsertColumns(int startColumn, int numberOfColumns, object defaultValue)
    {
        EnsureWorksheetSelected();

        if (_currentWorksheet == null)
            throw new CustomInvalidOperationException("Worksheet is not selected.");

        var column = _currentWorksheet.Column(startColumn) ?? throw new CustomInvalidOperationException($"Column {startColumn} does not exist in the worksheet.");
        column.InsertColumnsBefore(numberOfColumns);

        var lastRow = _currentWorksheet.LastRowUsed()?.RowNumber();
        if (lastRow != null)
            SetRangeValues(1, startColumn, lastRow.Value, startColumn + numberOfColumns - 1, defaultValue);
        else
            throw new CustomInvalidOperationException("No rows found in the worksheet.");
    }
    public void BatchInsertData<T>(string startCell, IEnumerable<T> data, int batchSize = 1000)
    {
        EnsureWorksheetSelected();
        var properties = typeof(T).GetProperties();
        var dataChunks = data.Select((item, index) => new { item, index })
                             .GroupBy(x => x.index / batchSize);

        int currentRow = _currentWorksheet.Cell(startCell).Address.RowNumber;
        int startColumn = _currentWorksheet.Cell(startCell).Address.ColumnNumber;

        foreach (var chunk in dataChunks)
        {
            foreach (var dataItem in chunk)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(dataItem.item) ?? string.Empty; // Null kontrolü
                    _currentWorksheet.Cell(currentRow, startColumn + i).Value = (XLCellValue)value;
                }
                currentRow++;
            }
        }
    }



    public void AddImage(string imagePath, int startRow, int startColumn, int endRow, int endColumn)
    {
        EnsureWorksheetSelected();

        // Resmi ekle ve başlangıç hücresine taşı
        var image = _currentWorksheet.AddPicture(imagePath)
            .MoveTo(_currentWorksheet.Cell(startRow, startColumn));

        // Toplam genişlik ve yükseklik hesapla
        double totalWidth = 0;
        for (int col = startColumn; col <= endColumn; col++)
        {
            totalWidth += _currentWorksheet.Column(col).Width;
        }

        double totalHeight = 0;
        for (int row = startRow; row <= endRow; row++)
        {
            totalHeight += _currentWorksheet.Row(row).Height;
        }

        // Genişlik ve yüksekliği resme uygula
        image.Width = (int)(totalWidth * 7); // Width birimi 'px', hücre genişliğini piksele dönüştürmek için yaklaşık çarpan
        image.Height = (int)(totalHeight * 1.5); // Height birimi 'px', hücre yüksekliği için yaklaşık çarpan
    }
    public void AddImageScaled(string imagePath, int startRow, int startColumn, int scalePercentage)
    {
        EnsureWorksheetSelected();
        var picture = _currentWorksheet.AddPicture(imagePath)
                                       .MoveTo(_currentWorksheet.Cell(startRow, startColumn));

        picture.Scale(scalePercentage / 100.0);
    }
    public void RemoveAllImages()
    {
        EnsureWorksheetSelected();
        var pictures = _currentWorksheet.Pictures.ToList();
        foreach (var picture in pictures)
        {
            picture.Delete();
        }
    }
    public void RemoveImage(string imageName)
    {
        EnsureWorksheetSelected();
        var picture = _currentWorksheet.Pictures.FirstOrDefault(p => p.Name == imageName);
        picture?.Delete();
    }

    public void SetDataList<T>(string startCell, List<T> data)
    {
        EnsureWorksheetSelected();
        _currentWorksheet.Cell(startCell).InsertData(data);
    }

    public void SetDataList<T>(int startRow, int startColumn, List<T> data)
    {
        EnsureWorksheetSelected();
        _currentWorksheet.Cell(startRow, startColumn).InsertData(data);
    }


    public void ExportToExcel<T>(string filePath, List<T> data)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new CustomArgumentException("File path cannot be empty or null.", nameof(filePath));
        if (data == null || data.Count == 0)
            throw new CustomArgumentException("Data cannot be empty or null.", nameof(data));

        AddWorksheet("Export");
        var properties = typeof(T).GetProperties();

        // Header Row
        for (int columnIndex = 1; columnIndex <= properties.Length; columnIndex++)
        {
            var property = properties[columnIndex - 1];
            _currentWorksheet.Cell(1, columnIndex).Value = property.Name;
            _currentWorksheet.Cell(1, columnIndex).Style.Font.Bold = true;
            _currentWorksheet.Cell(1, columnIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            _currentWorksheet.Cell(1, columnIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Data Rows
        for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
        {
            var item = data[rowIndex];
            for (int columnIndex = 1; columnIndex <= properties.Length; columnIndex++)
            {
                var property = properties[columnIndex - 1];
                var value = property.GetValue(item) ?? string.Empty;
                var cell = _currentWorksheet.Cell(rowIndex + 2, columnIndex);
                cell.Value = (XLCellValue)value;

                // Conditional formatting for numbers and dates
                if (value is DateTime)
                    cell.Style.DateFormat.Format = "yyyy-MM-dd";
                else if (value is double || value is int || value is decimal)
                    cell.Style.NumberFormat.Format = "#,##0.00";
            }
        }

        // AutoFit Columns for Better Readability
        _currentWorksheet.Columns().AdjustToContents();

        // Save the File
        SaveAs(filePath);
    }

    public Task<List<T>> ImportToEntityAsync<T>(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new CustomArgumentException("File path cannot be empty or null.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new NotFoundException("The specified file was not found.", filePath);

        var result = new List<T>();
        var properties = typeof(T).GetProperties();

        return Task.Run(() =>
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1) ?? throw new CustomInvalidOperationException("No worksheets found in the Excel file.");
            var rows = worksheet.RowsUsed().ToList();
            var headerRow = rows.First();
            var headers = headerRow.CellsUsed()
                .Select((cell, index) => new { Name = cell.Value.ToString(), Index = index + 1 })
                .ToDictionary(h => h.Name, h => h.Index);

            foreach (var row in rows.Skip(1))
            {
                var entity = Activator.CreateInstance<T>();
                foreach (var property in properties)
                {
                    if (headers.TryGetValue(property.Name, out int columnIndex))
                    {
                        var cellValue = row.Cell(columnIndex).Value;
                        SetPropertyValue(entity, property, cellValue);
                    }
                }
                result.Add(entity);
            }

            return result;
        });
    }

    private static void SetPropertyValue<T>(T entity, PropertyInfo property, object cellValue)
    {
        if (cellValue == null || cellValue.ToString() == string.Empty)
        {
            if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                property.SetValue(entity, null);
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var convertedValue = Convert.ChangeType(cellValue, targetType);
        property.SetValue(entity, convertedValue);
    }



    public void SaveAs(string filePath, string format = "xlsx", bool exportToPdf = false)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new CustomArgumentException("File path cannot be empty or null.", nameof(filePath));

        if (format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
        {
            _workbook.SaveAs(filePath);
        }
        else if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            SaveAsCsv(filePath);
        }
        else
        {
            throw new CustomException($"The format '{format}' is not supported.");
        }

        // Optionally export to PDF
        if (exportToPdf)
        {
            var pdfFilePath = Path.ChangeExtension(filePath, ".pdf");
            ExportToPdf(pdfFilePath);
        }
    }

    public void SaveAsCsv(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new CustomArgumentException("File path cannot be empty or null.", nameof(filePath));

        EnsureWorksheetSelected();
        var csvContent = new StringBuilder();

        // Iterate over rows
        foreach (var row in _currentWorksheet.RowsUsed())
        {
            var cells = row.CellsUsed().Select(cell => SanitizeForCsv(cell.GetString()));
            csvContent.AppendLine(string.Join(",", cells));
        }

        // Write to file asynchronously for better performance
        File.WriteAllTextAsync(filePath, csvContent.ToString()).ConfigureAwait(false);
    }

    private void ExportToPdf(string pdfFilePath)
    {
        EnsureWorksheetSelected();

        var tempExcelPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");

        try
        {
            // Excel dosyasını geçici bir dosya olarak kaydet
            _workbook.SaveAs(tempExcelPath);

            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "soffice"; // LibreOffice veya OpenOffice
            process.StartInfo.Arguments = $"--headless --convert-to pdf --outdir \"{Path.GetDirectoryName(pdfFilePath)}\" \"{tempExcelPath}\"";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;

            // Process başlat ve sonucu kontrol et
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new CustomInvalidOperationException("Error occurred during PDF export process.");
            }

            // PDF'nin başarıyla oluşup oluşmadığını kontrol et
            if (!File.Exists(pdfFilePath))
            {
                throw new NotFoundException("PDF export failed. Output file not created.", pdfFilePath);
            }
        }
        catch (Exception ex)
        {
            throw new CustomInvalidOperationException($"Failed to export Excel to PDF: {ex.Message}", ex);
        }
        finally
        {
            // Geçici Excel dosyasını sil
            if (File.Exists(tempExcelPath))
            {
                File.Delete(tempExcelPath);
            }
        }
    }


    private static string SanitizeForCsv(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        input = input.Replace("\"", "\"\""); // Escape double quotes

        if (input.Contains(',') || input.Contains('\n') || input.Contains('"'))
        {
            input = $"\"{input}\""; // Wrap in double quotes
        }

        return input;
    }

    private void EnsureWorksheetSelected()
    {
        if (_currentWorksheet == null)
        {
            throw new CustomInvalidOperationException("No worksheet is currently selected. Use AddWorksheet or SelectWorksheet first.");
        }
    }
}