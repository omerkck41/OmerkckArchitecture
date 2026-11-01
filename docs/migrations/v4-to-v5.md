# Migration v4.x → v5.0

**Impact:** Breaking · **Estimated effort:** S (package and registration renames)
**Status:** 5.0 is in preparation; this guide grows with every breaking change until the release.

## Why this major?

APIs announced as `[Obsolete]` during 4.x are removed, and the default packages depend only on permissively licensed
libraries. Image processing moves from SixLabors.ImageSharp to SkiaSharp (MIT): ImageSharp 4 is published under the
Six Labors Split License 1.0, which is not an OSI-approved licence, and asks for a licence key at Release build time
(decision `decisions/2026-10-31-image-library-replacement.md`).

## Affected Packages

| Package | Change type | Impact level |
|---|---|---|
| `Kck.Documents.ImageSharp` | **Removed** — replaced by `Kck.Documents.SkiaSharp` | High |

(Impact: Low = interface implementations only, Medium = call sites, High = configuration / DI level)

## At a glance (TL;DR)

- `Kck.Documents.ImageSharp` → `Kck.Documents.SkiaSharp`; `AddKckDocumentsImageSharp()` → `AddKckDocumentsSkiaSharp()`

## Detailed Changes

### 1. ImageSharp → SkiaSharp

**Reason:** licence constraint of ImageSharp 4 (see above). `Kck.Documents.ImageSharp` is removed in 5.0 without an
intermediate `[Obsolete]` release; `Kck.Documents.SkiaSharp` is available before 5.0, so you can switch early.

**Package:**

```xml
<!-- Before (4.x) -->
<PackageReference Include="Kck.Documents.ImageSharp" />

<!-- After -->
<PackageReference Include="Kck.Documents.SkiaSharp" />
<!-- Linux hosts (glibc and musl/Alpine) also need the native library: -->
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" />
```

**Registration:**

```csharp
// Before (4.x): services.AddKckDocumentsImageSharp();
services.AddKckDocumentsSkiaSharp();
```

`IImageProcessor` and `ImageProcessingOptions` are unchanged; code that injects `IImageProcessor` needs no change.

**Behaviour differences:**

| | ImageSharp (4.x) | SkiaSharp (5.0) |
|---|---|---|
| Output formats | jpeg/jpg, png, webp, gif, bmp | jpeg/jpg, png, webp — `gif` and `bmp` throw `NotSupportedException` |
| Input formats | JPEG, PNG, WebP, GIF, BMP, TIFF, ... | JPEG, PNG, WebP, GIF, BMP, ICO — TIFF throws `InvalidDataException` |
| Empty output format with a GIF/BMP source | GIF/BMP | JPEG |
| Transparent image → JPEG | encoder default | transparent areas become white |
| Image size limit | none | `SkiaSharpOptions.MaxPixels` (default 50,000,000) for the source and the resize target, checked from the header before decoding |
| `GetDimensionsAsync` | decodes the image; returns the stored size (EXIF orientation not applied) | reads the header only; returns the displayed size (orientation applied) |
| Native library | none (fully managed) | Windows/macOS included; Linux needs `SkiaSharp.NativeAssets.Linux.NoDependencies` |
| EXIF orientation | not applied | applied (baked into the pixels) |
| Metadata (EXIF incl. GPS, ICC profile) | preserved in the output | not written; pixels converted to sRGB |
| `Width` / `Height` `null` or 0 | derived from the source ratio | derived from the source ratio (no difference); negative throws `ArgumentOutOfRangeException` |
| `Quality` outside 1–100 | throws | clamped to 1–100 |

## Manual Steps

1. Replace the `Kck.Documents.ImageSharp` package reference with `Kck.Documents.SkiaSharp` and
   `AddKckDocumentsImageSharp()` with `AddKckDocumentsSkiaSharp()`; on Linux add
   `SkiaSharp.NativeAssets.Linux.NoDependencies` to the application.
2. Search for `"gif"` and `"bmp"` output formats (`OutputFormat`, `ConvertFormatAsync`) and switch them to `png` or
   `webp`; if you accept TIFF uploads, convert them before they reach `IImageProcessor`.
3. If you process images above 50 megapixels, raise `SkiaSharpOptions.MaxPixels`.

## References

- `decisions/2026-10-31-image-library-replacement.md`
- [docs/providers/documents.md](../providers/documents.md)
- CHANGELOG.md `[Unreleased]`
- [`docs/policies/deprecation.md`](../policies/deprecation.md)
