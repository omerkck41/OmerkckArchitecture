﻿namespace Core.Infrastructure.Excel.Models;

public class ExcelCell
{
    public int Row { get; set; }
    public int Column { get; set; }
    public object Value { get; set; } = default!;
    public string Style { get; set; } = default!;
}