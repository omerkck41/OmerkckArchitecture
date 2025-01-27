namespace Core.ToolKit.UIHelpers;

public static class ErrorPageGenerator
{
    public static async Task<string> GenerateAsync(int statusCode, string errorMessage, string? customCss = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty.", nameof(errorMessage));

        if (statusCode < 400 || statusCode > 599)
            throw new ArgumentException("Invalid status code. Status code must be between 400 and 599.", nameof(statusCode));

        var css = string.IsNullOrWhiteSpace(customCss) ?
            "<style>body { font-family: Arial, sans-serif; text-align: center; padding: 50px; } h1 { font-size: 50px; } p { font-size: 20px; }</style>" :
            customCss;

        return await Task.FromResult($@"<!DOCTYPE html>
                                    <html>
                                    <head>
                                        <title>Error {statusCode}</title>
                                        {css}
                                    </head>
                                    <body>
                                        <h1>Error {statusCode}</h1>
                                        <p>{errorMessage}</p>
                                    </body>
                                    </html>");
    }
}