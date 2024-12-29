using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Core.ToolKit.ImageProcessing;

public static class ImageHelper
{
    /// <summary>
    /// Resizes an image to the specified width and height, maintaining aspect ratio if desired.
    /// </summary>
    /// <param name="inputPath">Path to the original image.</param>
    /// <param name="outputPath">Path to save the resized image.</param>
    /// <param name="width">Desired width of the resized image.</param>
    /// <param name="height">Desired height of the resized image.</param>
    /// <param name="maintainAspectRatio">Whether to maintain the aspect ratio of the original image.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task ResizeAsync(string inputPath, string outputPath, int width, int height, bool maintainAspectRatio = true)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input image not found at path: {inputPath}");

        try
        {
            using var image = await Image.LoadAsync(inputPath); // Platform bağımsız bir şekilde görüntüyü yükler
            var resizeOptions = new ResizeOptions
            {
                Mode = maintainAspectRatio ? ResizeMode.Max : ResizeMode.Crop,
                Size = new Size(width, height)
            };

            image.Mutate(x => x.Resize(resizeOptions));

            await image.SaveAsync(outputPath); // Platform bağımsız bir şekilde görüntüyü kaydeder
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error occurred while resizing the image.", ex);
        }
    }

    /// <summary>
    /// Crops an image to the specified rectangle with high-quality processing.
    /// </summary>
    /// <param name="inputPath">Path to the original image.</param>
    /// <param name="outputPath">Path to save the cropped image.</param>
    /// <param name="cropArea">Rectangle defining the area to crop.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task CropAsync(string inputPath, string outputPath, Rectangle cropArea)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input image not found at path: {inputPath}");

        try
        {
            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(x => x.Crop(new Rectangle(cropArea.X, cropArea.Y, cropArea.Width, cropArea.Height)));
            await image.SaveAsync(outputPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error occurred while cropping the image.", ex);
        }
    }

    /// <summary>
    /// Converts an image to a specified format (e.g., PNG, JPEG) with high-quality processing.
    /// </summary>
    /// <param name="inputPath">Path to the original image.</param>
    /// <param name="outputPath">Path to save the converted image.</param>
    /// <param name="format">Target image format.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task ConvertFormatAsync(string inputPath, string outputPath, IImageEncoder encoder)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input image not found at path: {inputPath}");

        try
        {
            using var image = await Image.LoadAsync(inputPath); // Görüntüyü yükle
            await using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            await image.SaveAsync(outputStream, encoder); // Encodera göre kaydet
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error occurred while converting the image format.", ex);
        }
    }

    /// <summary>
    /// Reduces the file size of an image by adjusting its quality and maintaining its format.
    /// </summary>
    /// <param name="inputPath">Path to the original image.</param>
    /// <param name="outputPath">Path to save the compressed image.</param>
    /// <param name="quality">Quality level (1-100).</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task CompressAsync(string inputPath, string outputPath, int quality)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input image not found at path: {inputPath}");

        try
        {
            using var image = await Image.LoadAsync(inputPath);
            var encoder = new JpegEncoder { Quality = quality };

            await image.SaveAsync(outputPath, encoder);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error occurred while compressing the image.", ex);
        }
    }
}