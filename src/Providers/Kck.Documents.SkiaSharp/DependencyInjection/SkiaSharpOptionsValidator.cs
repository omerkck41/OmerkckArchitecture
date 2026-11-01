using Microsoft.Extensions.Options;

namespace Kck.Documents.SkiaSharp.DependencyInjection;

/// <summary>
/// Validates <see cref="SkiaSharpOptions"/> at startup (registered with <c>ValidateOnStart</c>).
/// </summary>
public sealed class SkiaSharpOptionsValidator : IValidateOptions<SkiaSharpOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SkiaSharpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxPixels >= 1)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Documents.SkiaSharp] SkiaSharpOptions is invalid:
              • MaxPixels: {options.MaxPixels} is invalid (must be at least 1)
                → Fix: opt.MaxPixels = 50_000_000   (the default)
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/documents.md
            """);
    }
}
