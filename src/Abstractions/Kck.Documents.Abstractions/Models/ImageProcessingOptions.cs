namespace Kck.Documents.Abstractions;

public sealed class ImageProcessingOptions
{
    public int? Width { get; init; }
    public int? Height { get; init; }
    public bool MaintainAspectRatio { get; init; } = true;
    public string? OutputFormat { get; init; }
    public int Quality { get; init; } = 80;
}
