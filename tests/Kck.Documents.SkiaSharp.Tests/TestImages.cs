using System.Buffers.Binary;
using SkiaSharp;

namespace Kck.Documents.SkiaSharp.Tests;

/// <summary>Builds small test images in memory (SkiaSharp for JPEG/PNG/WebP, hand-written bytes for GIF/BMP).</summary>
internal static class TestImages
{
    public static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];

    public static byte[] Encode(int width, int height, SKEncodedImageFormat format, SKColor? color = null)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(color ?? SKColors.CornflowerBlue);
        using var data = bitmap.Encode(format, 90);
        return data.ToArray();
    }

    public static MemoryStream Png(int width, int height, SKColor? color = null) =>
        new(Encode(width, height, SKEncodedImageFormat.Png, color));

    // Opaque RGBA noise (fixed seed): compresses badly, so PNG data is large and JPEG quality changes the size.
    public static SKBitmap NoisyBitmap(int width, int height, int seed = 42)
    {
        var random = new Random(seed);
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                bitmap.SetPixel(x, y, new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
        }

        return bitmap;
    }

    public static byte[] NoisyPng(int width, int height)
    {
        using var bitmap = NoisyBitmap(width, height);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    // A SkiaSharp JPEG with an APP1/EXIF segment inserted right after SOI: little-endian TIFF header and one IFD0 entry,
    // Orientation (0x0112, SHORT, count 1) = orientation, no next IFD.
    // With quadrants the stored image is red (top-left), green (top-right), blue (bottom-left), yellow (bottom-right).
    public static MemoryStream JpegWithExifOrientation(int width, int height, ushort orientation, bool quadrants = false)
    {
        var jpeg = quadrants ? QuadrantJpeg(width, height) : Encode(width, height, SKEncodedImageFormat.Jpeg);
        return InsertExifOrientation(jpeg, orientation);
    }

    // A single-channel (grayscale) JPEG: the decoder reports Gray8, a colour type the orientation canvas must handle.
    public static MemoryStream GrayJpegWithExifOrientation(int width, int height, ushort orientation)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque));
        bitmap.Erase(SKColors.Gray);
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        return InsertExifOrientation(data.ToArray(), orientation);
    }

    private static MemoryStream InsertExifOrientation(byte[] jpeg, ushort orientation)
    {
        byte[] payload =
        [
            (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0x00, 0x00,
            (byte)'I', (byte)'I', 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00, // TIFF header, IFD0 at offset 8
            0x01, 0x00,                                               // one entry
            0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00,           // tag 0x0112, SHORT, count 1
            (byte)orientation, (byte)(orientation >> 8), 0x00, 0x00,  // value
            0x00, 0x00, 0x00, 0x00                                    // no next IFD
        ];
        var segmentLength = payload.Length + 2; // the length field counts itself
        byte[] app1 = [0xFF, 0xE1, (byte)(segmentLength >> 8), (byte)segmentLength, .. payload];
        return new MemoryStream([.. jpeg.AsSpan(0, 2), .. app1, .. jpeg.AsSpan(2)]);
    }

    private static byte[] QuadrantJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            using var paint = new SKPaint();
            (SKColor Color, SKRect Rect)[] quadrants =
            [
                (SKColors.Red, new SKRect(0, 0, width / 2f, height / 2f)),
                (SKColors.Lime, new SKRect(width / 2f, 0, width, height / 2f)),
                (SKColors.Blue, new SKRect(0, height / 2f, width / 2f, height)),
                (SKColors.Yellow, new SKRect(width / 2f, height / 2f, width, height))
            ];
            foreach (var (color, rect) in quadrants)
            {
                paint.Color = color;
                canvas.DrawRect(rect, paint);
            }
        }

        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 95);
        return data.ToArray();
    }

    // A 4×4 PNG tagged with Display P3 (sRGB transfer function, P3 gamut) whose every pixel holds the given RGBA bytes,
    // and the same colour converted to sRGB by Skia (the expected decode result).
    public static (byte[] Png, SKColor ExpectedSrgb) DisplayP3Png(byte[] rgba)
    {
        using var p3 = SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Srgb, SKColorSpaceXyz.DisplayP3);
        using var source = new SKBitmap(new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul, p3));
        var pixels = Enumerable.Repeat(rgba, 16).SelectMany(p => p).ToArray();
        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, source.GetPixels(), pixels.Length);
        using var data = source.Encode(SKEncodedImageFormat.Png, 100);

        using var srgb = new SKBitmap(
            new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        using (var sourcePixmap = source.PeekPixels())
        using (var srgbPixmap = srgb.PeekPixels())
        {
            if (!sourcePixmap.ReadPixels(srgbPixmap))
                throw new InvalidOperationException("Skia could not convert the P3 pixels to sRGB.");
        }

        var converted = new byte[4];
        System.Runtime.InteropServices.Marshal.Copy(srgb.GetPixels(), converted, 0, 4);
        return (data.ToArray(), new SKColor(converted[0], converted[1], converted[2], converted[3]));
    }

    // ICO with one directory entry whose image is an embedded PNG (Vista-style icon).
    public static MemoryStream IcoWithPng(int size)
    {
        var png = Encode(size, size, SKEncodedImageFormat.Png);
        var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            w.Write((short)0);           // reserved
            w.Write((short)1);           // type: icon
            w.Write((short)1);           // image count
            w.Write((byte)size);         // width (0 = 256)
            w.Write((byte)size);         // height
            w.Write((byte)0);            // palette size
            w.Write((byte)0);            // reserved
            w.Write((short)1);           // colour planes
            w.Write((short)32);          // bits per pixel
            w.Write(png.Length);         // image size
            w.Write(6 + 16);             // image offset
            w.Write(png);
        }

        ms.Position = 0;
        return ms;
    }

    // GIF89a, 1×1, global colour table of 2 entries (red, black), one LZW-coded pixel (index 0), no extensions.
    public static MemoryStream Gif1x1() => new(
    [
        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, // "GIF89a"
        0x01, 0x00, 0x01, 0x00,             // logical screen 1×1
        0x80, 0x00, 0x00,                   // global colour table present, 2 entries
        0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, // red, black
        0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, // image descriptor 1×1
        0x02, 0x02, 0x44, 0x01, 0x00,       // LZW min code size 2: clear, 0, end
        0x3B                                // trailer
    ]);

    // Uncompressed 24-bit BMP (BITMAPINFOHEADER), bottom-up rows padded to 4 bytes.
    public static MemoryStream Bmp24(int width, int height)
    {
        var rowSize = (width * 3 + 3) / 4 * 4;
        var pixelBytes = rowSize * height;
        var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            w.Write((byte)'B');
            w.Write((byte)'M');
            w.Write(54 + pixelBytes); // file size
            w.Write(0);               // reserved
            w.Write(54);              // pixel data offset
            w.Write(40);              // BITMAPINFOHEADER size
            w.Write(width);
            w.Write(height);
            w.Write((short)1);        // planes
            w.Write((short)24);       // bits per pixel
            w.Write(0);               // BI_RGB
            w.Write(pixelBytes);
            w.Write(2835);            // 72 dpi
            w.Write(2835);
            w.Write(0);
            w.Write(0);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    w.Write((byte)0x20); // B
                    w.Write((byte)0x80); // G
                    w.Write((byte)0xE0); // R
                }

                for (var p = width * 3; p < rowSize; p++)
                    w.Write((byte)0);
            }
        }

        ms.Position = 0;
        return ms;
    }

    // A real small PNG whose IHDR claims width×height (CRC recomputed); the pixel data stays that of the small image,
    // so only a header-only reader can report the claimed size without failing or allocating the claimed pixels.
    public static MemoryStream PngWithPatchedHeader(int width, int height)
    {
        var bytes = Encode(4, 4, SKEncodedImageFormat.Png);
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(16, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(20, 4), height);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(29, 4), Crc32(bytes.AsSpan(12, 17))); // "IHDR" + 13 data bytes
        return new MemoryStream(bytes);
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc ^= b;
            for (var k = 0; k < 8; k++)
                crc = (crc & 1) != 0 ? 0xEDB88320u ^ (crc >> 1) : crc >> 1;
        }

        return crc ^ 0xFFFFFFFFu;
    }
}

/// <summary>A non-seekable stream that fails on any read: proves a request is rejected before the input is read.</summary>
internal sealed class UnreadableStream : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) =>
        throw new InvalidOperationException("The stream must not be read.");
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

/// <summary>A seekable read-only wrapper that cancels <paramref name="cts"/> after the first read (the header).</summary>
internal sealed class CancelOnReadStream(Stream inner, CancellationTokenSource cts) : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => inner.Length;
    public override long Position { get => inner.Position; set => inner.Position = value; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = inner.Read(buffer, offset, count);
        cts.Cancel();
        return read;
    }

    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            inner.Dispose();
        base.Dispose(disposing);
    }
}

/// <summary>A read-only, forward-only wrapper (like an HTTP request body).</summary>
internal sealed class NonSeekableStream(Stream inner) : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
