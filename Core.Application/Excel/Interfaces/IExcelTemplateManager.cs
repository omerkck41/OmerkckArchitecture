namespace Core.Application.Excel.Interfaces;

public interface IExcelTemplateManager
{
    void LoadTemplate(string templatePath);
    void SaveTemplate(string destinationPath);
    void FillTemplateData(Dictionary<string, object> data);
}