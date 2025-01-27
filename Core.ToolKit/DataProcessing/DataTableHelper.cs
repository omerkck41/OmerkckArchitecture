using System.Data;
using System.Reflection;

namespace Core.ToolKit.DataProcessing;

public static class DataTableHelper
{
    /// <summary>
    /// Converts a DataTable to a list of objects with enhanced error handling and flexibility.
    /// </summary>
    /// <typeparam name="T">Type of objects to convert to.</typeparam>
    /// <param name="dataTable">DataTable to convert.</param>
    /// <returns>List of objects of type T.</returns>
    public static async Task<List<T>> ToListAsync<T>(DataTable dataTable) where T : new()
    {
        if (dataTable == null)
            throw new ArgumentNullException(nameof(dataTable), "DataTable cannot be null.");

        var properties = typeof(T).GetProperties();
        var result = new List<T>();

        await Task.Run(() =>
        {
            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(CreateItemFromRow<T>(row, properties));
            }
        });

        return result;
    }

    private static T CreateItemFromRow<T>(DataRow row, PropertyInfo[] properties) where T : new()
    {
        var obj = new T();

        foreach (var property in properties)
        {
            if (row.Table.Columns.Contains(property.Name) && row[property.Name] != DBNull.Value)
            {
                property.SetValue(obj, Convert.ChangeType(row[property.Name], property.PropertyType));
            }
        }

        return obj;
    }

    /// <summary>
    /// Converts a list of objects to a DataTable with enhanced structure and metadata support.
    /// </summary>
    /// <typeparam name="T">Type of objects in the list.</typeparam>
    /// <param name="data">List of objects to convert.</param>
    /// <returns>DataTable representing the list of objects.</returns>
    public static DataTable ToDataTable<T>(IEnumerable<T> data)
    {
        if (data == null || !data.Any())
            throw new ArgumentNullException(nameof(data), "Data cannot be null or empty.");

        var dataTable = new DataTable(typeof(T).Name);
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
        }

        foreach (var item in data)
        {
            var row = dataTable.NewRow();
            foreach (var property in properties)
            {
                row[property.Name] = property.GetValue(item) ?? DBNull.Value;
            }
            dataTable.Rows.Add(row);
        }

        return dataTable;
    }
}