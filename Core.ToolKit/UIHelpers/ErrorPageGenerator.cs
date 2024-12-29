namespace Core.ToolKit.UIHelpers;

public static class ErrorPageGenerator
{
    /// <summary>
    /// Generates a basic HTML error page.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorMessage">The error message to display.</param>
    /// <returns>HTML string representing the error page.</returns>
    public static string Generate(int statusCode, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty.", nameof(errorMessage));

        return $@"<!DOCTYPE html>
<html>
<head>
    <title>Error {statusCode}</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
        h1 {{ font-size: 50px; }}
        p {{ font-size: 20px; }}
    </style>
</head>
<body>
    <h1>Error {statusCode}</h1>
    <p>{errorMessage}</p>
</body>
</html>";
    }
}