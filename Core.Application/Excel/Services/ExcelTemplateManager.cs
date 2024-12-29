using ClosedXML.Excel;
using Core.Application.Excel.Helpers;
using Core.Application.Excel.Interfaces;

namespace Core.Application.Excel.Services;

public class ExcelTemplateManager : IExcelTemplateManager
{
    private XLWorkbook? _workbook;
    private IXLWorksheet? _templateWorksheet;

    public void LoadTemplate(string templatePath)
    {
        _workbook = new XLWorkbook(templatePath);
        _templateWorksheet = _workbook.Worksheet(1);
    }

    public void SaveTemplate(string destinationPath)
    {
        if (_workbook == null) throw new InvalidOperationException("Template is not loaded.");
        _workbook.SaveAs(destinationPath);
    }

    public void FillTemplateData(Dictionary<string, object> data)
    {
        if (_templateWorksheet == null) throw new InvalidOperationException("Template is not loaded.");
        ExcelTemplateHelper.FillTemplate(_templateWorksheet, data);
    }
}