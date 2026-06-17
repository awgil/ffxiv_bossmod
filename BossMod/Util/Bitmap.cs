using System.IO;
using TerraFX.Interop.Windows;
using System.Runtime.InteropServices;

namespace BossMod;

// utility for working with 2d 1bpp bitmaps
// some notes:
// - supports only BITMAPINFOHEADER (could've been BITMAPCOREHEADER, but bottom-up bitmaps don't make sense with FF coordinate system)
// - supports only 1bpp bitmaps without compression; per bitmap spec, first pixel is highest bit, etc.
// - supports only top-down bitmaps (with negative height)
// - horizontal/vertical resolution is equal and is 'pixels per 1024 world units'
// - per bitmap spec, rows are padded to 4 byte alignment
public sealed class Bitmap
{
    public record struct Rect(int Left, int Top, int Right, int Bottom) // bottom/right exclusive
    {
        public readonly int Width => Right - Left;
        public readonly int Height => Bottom - Top;

        // return a rect that, if offset by (dx,dy), is clamped to clip rect
        public readonly Rect Clamped(in Rect c, int dx = 0, int dy = 0) => new
        (
            Math.Max(Left, c.Left - dx),
            Math.Max(Top, c.Top - dy),
            Math.Min(Right, c.Right - dx),
            Math.Min(Bottom, c.Bottom - dy)
        );
    }

    public record struct Region(Bitmap Bitmap, Rect Rect)
    {
        // copy region into destination bitmap, starting at specified coordinates
        // any out-of-range accesses are ignored: actual copied region is intersection of source data, copy rect and dest rect
        public readonly void CopyTo(Bitmap dest, int destX, int destY)
        {
            var offX = destX - Rect.Left;
            var offY = destY - Rect.Top;
            var r = Rect.Clamped(Bitmap.FullRect).Clamped(dest.FullRect, offX, offY);
            if (r.Width <= 0)
                return; // nothing to copy

            // note: this could be optimized if needed...
            for (int y = r.Top; y < r.Bottom; ++y)
            {
                for (int x = r.Left; x < r.Right; ++x)
                {
                    dest[x + offX, y + offY] = Bitmap[x, y];
                }
            }
        }

        // upsample a region into destination bitmap: each source pixel is converted into 2x2 destination pixels of same value
        public readonly void UpsampleTo(Bitmap dest, int destX, int destY)
        {
            // work in lower-resolution coordinates
            var subX = destX & 1;
            var subY = destY & 1;
            destX >>= 1;
            destY >>= 1;
            var offX = destX - Rect.Left;
            var offY = destY - Rect.Top;
            var r = Rect.Clamped(Bitmap.FullRect).Clamped(new(0, 0, (dest.Width - subX) >> 1, (dest.Height - subY) >> 1), offX, offY);
            if (r.Width <= 0)
                return; // nothing to copy

            // note: this could be optimized if needed...
            for (int y = r.Top, dy = ((r.Top + offY) << 1) + subY; y < r.Bottom; ++y, dy += 2)
            {
                for (int x = r.Left, dx = ((r.Left + offX) << 1) + subX; x < r.Right; ++x, dx += 2)
                {
                    dest[dx, dy] = dest[dx + 1, dy] = dest[dx, dy + 1] = dest[dx + 1, dy + 1] = Bitmap[x, y];
                }
            }
        }

        // downsample a region into destination bitmap; each 2x2 group of source pixels are copied into destination pixel if equal, otherwise destination is set to fallback value
        // if this region's width or height are odd, last column/row is ignored
        public readonly void DownsampleTo(Bitmap dest, int destX, int destY, bool fallback)
        {
            // work in lower-resolution coordinates
            var subX = Rect.Left & 1;
            var subY = Rect.Top & 1;
            var r = new Rect(Rect.Left >> 1, Rect.Top >> 1, Rect.Right >> 1, Rect.Bottom >> 1);
            var offX = destX - r.Left;
            var offY = destY - r.Top;
            r = r.Clamped(new(0, 0, (Bitmap.Width - subX) >> 1, (Bitmap.Height - subY) >> 1)).Clamped(dest.FullRect, offX, offY);
            if (r.Width <= 0)
                return; // nothing to copy

            // note: this could be optimized if needed...
            for (int y = r.Top, sy = (r.Top << 1) + subY; y < r.Bottom; ++y, sy += 2)
            {
                for (int x = r.Left, sx = (r.Left << 1) + subX; x < r.Right; ++x, sx += 2)
                {
                    var v = Bitmap[sx, sy];
                    if (v != Bitmap[sx + 1, sy] || v != Bitmap[sx, sy + 1] || v != Bitmap[sx + 1, sy + 1])
                        v = fallback;
                    dest[x + offX, y + offY] = v;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 14)]
    public struct FileHeader
    {
        [FieldOffset(0)] public ushort Type; // 0x4D42 'BM'
        [FieldOffset(2)] public int Size; // size of the file in bytes
        [FieldOffset(6)] public uint Reserved;
        [FieldOffset(10)] public int OffBits; // offset from this to pixel data
    }
    public const ushort Magic = 0x4D42;

    public readonly int Width;
    public readonly int Height;
    public readonly int BytesPerRow;
    public readonly int Resolution; // pixels per 1024 world units
    public readonly Color Color0;
    public readonly Color Color1;
    public readonly byte[] Pixels;

    public float PixelSize => 1024.0f / Resolution;
    public int CoordToIndex(int x, int y) => y * BytesPerRow + (x >> 3);
    public byte CoordToMask(int x) => (byte)(0x80u >> (x & 7));
    public ref byte ByteAt(int x, int y) => ref Pixels[CoordToIndex(x, y)];
    public Rect FullRect => new(0, 0, Width, Height);
    public Region FullRegion => new(this, FullRect);

    public bool this[int x, int y]
    {
        get => (ByteAt(x, y) & CoordToMask(x)) != 0;
        set
        {
            if (value)
                ByteAt(x, y) |= CoordToMask(x);
            else
                ByteAt(x, y) &= (byte)~CoordToMask(x);
        }
    }

    public Bitmap(int width, int height, Color color0, Color color1, int resolution = 2048)
    {
        Width = width;
        Height = height;
        BytesPerRow = (width + 31) >> 5 << 2;
        Resolution = resolution;
        Color0 = color0;
        Color1 = color1;
        Pixels = new byte[height * BytesPerRow];
    }

    public Bitmap(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        var fileHeader = stream.ReadStruct<FileHeader>();
        if (fileHeader.Type != Magic)
            throw new ArgumentException($"Not a bitmap: magic is {fileHeader.Type:X4}");

        var header = stream.ReadStruct<BITMAPINFOHEADER>();
        if (header.biSize != Marshal.SizeOf<BITMAPINFOHEADER>())
            throw new ArgumentException($"Bitmap has unsupported header size {header.biSize}");
        if (header.biWidth <= 0)
            throw new ArgumentException($"Bitmap has non-positive width {header.biWidth}");
        if (header.biHeight >= 0)
            throw new ArgumentException($"Bitmap is not top-down (height={header.biHeight})");
        if (header.biBitCount != 1)
            throw new ArgumentException($"Bitmap is not 1bpp (bitcount={header.biBitCount})");
        if (header.biCompression != 0)
            throw new ArgumentException($"Bitmap has unsupported compression method {header.biCompression:X8}");
        if (header.biXPelsPerMeter != header.biYPelsPerMeter || header.biXPelsPerMeter <= 0)
            throw new ArgumentException($"Bitmap has inconsistent or non-positive resolution {header.biXPelsPerMeter}x{header.biYPelsPerMeter}");
        if (header.biClrUsed is not (0 or 2))
            throw new ArgumentException($"Bitmap has wrong palette size {header.biClrUsed}");

        Width = header.biWidth;
        Height = -header.biHeight;
        BytesPerRow = (Width + 31) >> 5 << 2;
        Resolution = header.biXPelsPerMeter;
        Color0 = Color.FromARGB(reader.ReadUInt32());
        Color1 = Color.FromARGB(reader.ReadUInt32());
        Pixels = reader.ReadBytes(Height * BytesPerRow);
    }

    public void Save(string filename)
    {
        using var fstream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
        var headerSize = Marshal.SizeOf<FileHeader>() + Marshal.SizeOf<BITMAPINFOHEADER>() + 2 * Marshal.SizeOf<uint>();
        fstream.WriteStruct(new FileHeader() { Type = Magic, Size = headerSize + Pixels.Length, OffBits = headerSize });
        fstream.WriteStruct(new BITMAPINFOHEADER() { biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(), biWidth = Width, biHeight = -Height, biPlanes = 1, biBitCount = 1, biXPelsPerMeter = Resolution, biYPelsPerMeter = Resolution });
        fstream.WriteStruct(Color0.ToARGB());
        fstream.WriteStruct(Color1.ToARGB());
        fstream.Write(Pixels);
    }

    public Bitmap Clone()
    {
        var res = new Bitmap(Width, Height, Color0, Color1, Resolution);
        Array.Copy(Pixels, res.Pixels, Pixels.Length);
        return res;
    }
}
