namespace Core.Application.Excel.Models;

public class ExcelRow
{
    public int RowIndex { get; set; }
    public List<ExcelCell> Cells { get; set; } = new();
}