namespace Core.Application.Excel.Models;

public class ExcelCell
{
    public int Row { get; set; }
    public int Column { get; set; }
    public object Value { get; set; }
    public string Style { get; set; }
}