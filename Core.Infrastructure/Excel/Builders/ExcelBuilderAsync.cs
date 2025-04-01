using ClosedXML.Excel;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Infrastructure.Excel.Interfaces;
using System.Text;

namespace Core.Infrastructure.Excel.Builders;

public class ExcelBuilderAsync : IExcelBuilderAsync
{
    private readonly XLWorkbook _workbook;
    private IXLWorksheet _currentWorksheet;

    public ExcelBuilderAsync()
    {
        _workbook = new XLWorkbook();
        _currentWorksheet = _workbook.AddWorksheet("Sheet1"); // Varsayılan bir çalışma sayfası ekleniyor
    }

    public Task AddWorksheetAsync(string sheetName) =>
        Task.Run(() => _currentWorksheet = _workbook.Worksheets.Add(sheetName));

    public Task SelectWorksheetAsync(string sheetName) =>
        Task.Run(() => _currentWorksheet = _workbook.Worksheet(sheetName));

    public Task SetCellValueAsync(int row, int column, object value) =>
    Task.Run(async () =>
    {
        await EnsureWorksheetSelectedAsync();
        _currentWorksheet.Cell(row, column).Value = (XLCellValue)value;
    });

    public Task CopyCellAsync(int sourceRow, int sourceColumn, int targetRow, int targetColumn, bool includeFormatting = true) =>
    Task.Run(async () =>
    {
        await EnsureWorksheetSelectedAsync();
        var sourceCell = _currentWorksheet.Cell(sourceRow, sourceColumn);
        var targetCell = _currentWorksheet.Cell(targetRow, targetColumn);

        targetCell.Value = sourceCell.Value;
        if (includeFormatting) targetCell.Style = sourceCell.Style;
    });

    public Task SetRangeValuesAsync(int startRow, int startColumn, int endRow, int endColumn, object value)
    {
        return Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    _currentWorksheet.Cell(i, j).Value = (XLCellValue)value;
                }
            }
        });
    }

    public Task CopyRowAsync(int sourceRow, int targetRow, bool includeFormatting = true) =>
        Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            var source = _currentWorksheet.Row(sourceRow);
            var target = _currentWorksheet.Row(targetRow);

            foreach (var sourceCell in source.CellsUsed())
            {
                var targetCell = target.Cell(sourceCell.Address.ColumnNumber);
                targetCell.Value = sourceCell.Value;
                if (includeFormatting) targetCell.Style = sourceCell.Style;
            }
        });

    public Task CopyRangeAsync(int startRow, int startColumn, int endRow, int endColumn, int targetRow, int targetColumn, bool includeFormatting = true) =>
        Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            var range = _currentWorksheet.Range(startRow, startColumn, endRow, endColumn);
            range.CopyTo(_currentWorksheet.Cell(targetRow, targetColumn));
        });

    public Task SetColumnWidthAsync(int column, double width) =>
    Task.Run(async () =>
    {
        await EnsureWorksheetSelectedAsync();
        _currentWorksheet.Column(column).Width = width;
    });

    public Task SetPageOrientationAsync(bool isLandscape) =>
    Task.Run(async () =>
    {
        await EnsureWorksheetSelectedAsync();
        _currentWorksheet.PageSetup.PageOrientation = isLandscape ? XLPageOrientation.Landscape : XLPageOrientation.Portrait;
    });

    public Task SetPageMarginsAsync(double top, double bottom, double left, double right) =>
        Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            _currentWorksheet.PageSetup.Margins.Top = top;
            _currentWorksheet.PageSetup.Margins.Bottom = bottom;
            _currentWorksheet.PageSetup.Margins.Left = left;
            _currentWorksheet.PageSetup.Margins.Right = right;
        });
    public Task SetPageSizeAsync(string pageSize) =>
        Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            _currentWorksheet.PageSetup.PaperSize = pageSize.ToLower() switch
            {
                "a4" => XLPaperSize.A4Paper,
                "letter" => XLPaperSize.LetterPaper,
                _ => XLPaperSize.A4Paper
            };
        });

    public Task ClearContentAsync(string? range) =>
        Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            if (!string.IsNullOrWhiteSpace(range))
                _currentWorksheet.Range(range).Clear(XLClearOptions.Contents);
            else
                _currentWorksheet.Clear(XLClearOptions.Contents);
        });

    public Task BatchInsertColumnsAsync(int startColumn, int numberOfColumns, object defaultValue) =>
    Task.Run(async () =>
    {
        await EnsureWorksheetSelectedAsync();

        _currentWorksheet.Column(startColumn).InsertColumnsBefore(numberOfColumns);
        for (int i = 0; i < numberOfColumns; i++)
        {
            foreach (var cell in _currentWorksheet.Column(startColumn + i).CellsUsed())
            {
                cell.Value = (XLCellValue)defaultValue;
            }
        }
    });
    public Task BatchInsertDataAsync<T>(string startCell, IEnumerable<T> data, int batchSize = 1000) =>
    Task.Run(async () =>
    {
        await EnsureWorksheetSelectedAsync();

        var properties = typeof(T).GetProperties();
        int currentRow = _currentWorksheet.Cell(startCell).Address.RowNumber;
        int startColumn = _currentWorksheet.Cell(startCell).Address.ColumnNumber;

        foreach (var batch in data.Batch(batchSize)) // Batch extension kullanılacak
        {
            foreach (var item in batch)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(item) ?? string.Empty;
                    _currentWorksheet.Cell(currentRow, startColumn + i).Value = (XLCellValue)value;
                }
                currentRow++;
            }
        }
    });
    public Task BatchInsertRowsAsync(int startRow, int numberOfRows, object defaultValue)
    {
        return Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            _currentWorksheet.Row(startRow).InsertRowsAbove(numberOfRows);
            for (int i = startRow; i < startRow + numberOfRows; i++)
            {
                for (int j = 1; j <= _currentWorksheet.LastColumnUsed()?.ColumnNumber(); j++)
                {
                    _currentWorksheet.Cell(i, j).Value = (XLCellValue)defaultValue;
                }
            }
        });
    }

    public Task AddImageAsync(string imagePath, int startRow, int startColumn, int endRow, int endColumn)
    {
        return Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            var picture = _currentWorksheet.AddPicture(imagePath)
                                           .MoveTo(_currentWorksheet.Cell(startRow, startColumn));

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

            picture.Width = (int)(totalWidth * 7); // Yaklaşık piksel dönüştürme
            picture.Height = (int)(totalHeight * 1.5);
        });
    }
    public Task AddImageScaledAsync(string imagePath, int startRow, int startColumn, int scalePercentage) =>
        Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            var picture = _currentWorksheet.AddPicture(imagePath).MoveTo(_currentWorksheet.Cell(startRow, startColumn));
            picture.Scale(scalePercentage / 100.0);
        });
    public Task RemoveImageAsync(string imageName) =>
        Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            var picture = _currentWorksheet.Pictures.FirstOrDefault(p => p.Name == imageName);
            picture?.Delete();
        });


    public Task SetDataListAsync<T>(string startCell, List<T> data) =>
    Task.Run(async () =>
    {
        await EnsureWorksheetSelectedAsync();
        _currentWorksheet.Cell(startCell).InsertData(data);
    });
    public Task SetDataListAsync<T>(int startRow, int startColumn, List<T> data)
    {
        return Task.Run(async () =>
        {
            await EnsureWorksheetSelectedAsync();

            var properties = typeof(T).GetProperties();
            int currentRow = startRow;

            foreach (var item in data)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(item) ?? string.Empty; // Null kontrolü ve varsayılan değer
                    _currentWorksheet.Cell(currentRow, startColumn + i).Value = (XLCellValue)value;
                }
                currentRow++;
            }
        });
    }


    public Task ExportToExcelAsync<T>(string filePath, List<T> data)
    {
        return Task.Run(async () =>
        {
            if (string.IsNullOrEmpty(filePath))
                throw new CustomArgumentException("File path cannot be empty or null.", nameof(filePath));

            if (data == null || data.Count == 0)
                throw new CustomArgumentException("Data cannot be empty or null.", nameof(data));

            await AddWorksheetAsync("Export");

            var properties = typeof(T).GetProperties();

            // Header Row
            for (int columnIndex = 1; columnIndex <= properties.Length; columnIndex++)
            {
                var property = properties[columnIndex - 1];
                await SetCellValueAsync(1, columnIndex, property.Name);

                var headerCell = _currentWorksheet.Cell(1, columnIndex);
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Data Rows
            for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
            {
                var item = data[rowIndex];
                for (int columnIndex = 1; columnIndex <= properties.Length; columnIndex++)
                {
                    var property = properties[columnIndex - 1];
                    var value = property.GetValue(item) ?? string.Empty; // Null kontrolü ekleniyor

                    var cell = _currentWorksheet.Cell(rowIndex + 2, columnIndex);
                    await SetCellValueAsync(rowIndex + 2, columnIndex, value);

                    // Conditional formatting for numbers and dates
                    if (value is DateTime)
                        cell.Style.DateFormat.Format = "yyyy-MM-dd";
                    else if (value is double || value is int || value is decimal)
                        cell.Style.NumberFormat.Format = "#,##0.00";
                }
            }


            // AutoFit Columns for Better Readability
            await Task.Run(() => _currentWorksheet.Columns().AdjustToContents());

            // Save the File
            await SaveAsAsync(filePath);
        });
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
    private static void SetPropertyValue<T>(T entity, System.Reflection.PropertyInfo property, object cellValue)
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



    public Task SaveAsAsync(string filePath, string format = "xlsx", bool exportToPdf = false) =>
    Task.Run(async () =>
    {
        await EnsureWorksheetSelectedAsync();
        if (format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
        {
            _workbook.SaveAs(filePath);
        }
        else if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            await SaveAsCsvAsync(filePath);
        }
        else
        {
            throw new CustomException($"The format '{format}' is not supported.");
        }

        if (exportToPdf)
        {
            var pdfFilePath = Path.ChangeExtension(filePath, ".pdf");
            await ExportToPdfAsync(pdfFilePath);
        }
    });
    public Task ExportToPdfAsync(string pdfFilePath)
    {
        return Task.Run(() =>
        {
            EnsureWorksheetSelectedAsync();

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
        });
    }

    public Task SaveAsCsvAsync(string filePath)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(filePath))
                throw new CustomArgumentException("File path cannot be empty or null.", nameof(filePath));

            EnsureWorksheetSelectedAsync();
            var csvContent = new StringBuilder();

            foreach (var row in _currentWorksheet.RowsUsed())
            {
                var cells = row.CellsUsed().Select(cell => SanitizeForCsv(cell.GetString()));
                csvContent.AppendLine(string.Join(",", cells));
            }

            File.WriteAllText(filePath, csvContent.ToString());
        });
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
    private Task EnsureWorksheetSelectedAsync()
    {
        return Task.Run(() =>
        {
            if (_currentWorksheet == null)
            {
                throw new CustomInvalidOperationException("No worksheet is currently selected. Use AddWorksheetAsync or SelectWorksheetAsync first.");
            }
        });
    }
}

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        if (size <= 0) throw new CustomArgumentException("Batch size must be greater than 0.", nameof(size));

        List<T> batch = new(size);
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new List<T>(size);
            }
        }
        if (batch.Count > 0)
            yield return batch;
    }
}