﻿using Core.Application.Excel.Interfaces;

namespace Core.Application.Excel.Examples;

public class DataExportExample
{
    public static void ExportData(IExcelBuilder builder)
    {
        builder.AddWorksheet("Data");
        for (int i = 1; i <= 100; i++)
        {
            builder.SetCellValue(i, 1, $"Row {i}");
        }
        builder.SaveAsCsv("data.csv");
    }
}