﻿namespace Core.Application.Excel.Models;

public class ExcelWorksheetModel
{
    public string WorksheetName { get; set; }
    public List<ExcelRow> Rows { get; set; } = [];
}