namespace BossMod;

// utility for working with 2d 1bpp bitmaps
// for easier compatibility with .bmp format, this is stored row-by-row (without any swizzle) with stride rounded up to 32 pixels (ie packing bits into dwords and requiring rows to start on dword boundary)
public sealed class Bitmap
{
    public readonly int Width;
    public readonly int Height;
    public readonly int WordsPerRow;
    public readonly uint[] Pixels;

    public int BytesPerRow => WordsPerRow << 2;
    public int CoordToIndex(int x, int y) => y * WordsPerRow + (x >> 5);
    public uint CoordToMask(int x) => 1u << (x & 31);
    public ref uint WordAt(int x, int y) => ref Pixels[CoordToIndex(x, y)];

    public bool this[int x, int y]
    {
        get => (WordAt(x, y) & CoordToMask(x)) != 0;
        set
        {
            if (value)
                WordAt(x, y) |= CoordToMask(x);
            else
                WordAt(x, y) &= ~CoordToMask(x);
        }
    }

    public Bitmap(int width, int height)
    {
        Width = width;
        Height = height;
        WordsPerRow = (width + 31) >> 5;
        Pixels = new uint[height * WordsPerRow];
    }

    public Bitmap Clone()
    {
        var res = new Bitmap(Width, Height);
        Array.Copy(Pixels, res.Pixels, Pixels.Length);
        return res;
    }

    // copy a rectangle from source bitmap into current bitmap
    // any out-of-range accesses are ignored: actual copied region is intersection of source data, copy rect and dest rect
    public void CopyRegion(Bitmap source, int sourceX, int sourceY, int sourceWidth, int sourceHeight, int destX, int destY)
    {
        var offX = destX - sourceX;
        var offY = destY - sourceY;
        ClampRect(ref sourceX, ref sourceY, ref sourceWidth, ref sourceHeight, 0, 0, source.Width, source.Height, 0, 0);
        ClampRect(ref sourceX, ref sourceY, ref sourceWidth, ref sourceHeight, 0, 0, Width, Height, offX, offY);
        if (sourceWidth <= 0)
            return; // nothing to copy

        // note: this could be optimized if needed...
        var maxX = sourceX + sourceWidth;
        var maxY = sourceY + sourceHeight;
        for (int y = sourceY; y < maxY; ++y)
        {
            for (int x = sourceX; x < maxX; ++x)
            {
                this[x + offX, y + offY] = source[x, y];
            }
        }
    }

    // upsample a region from source bitmap into current bitmap: each source pixel is converted into 2x2 destination pixels of same value
    // note: if dest x/y are odd, they are rounded down
    public void UpsampleRegion(Bitmap source, int sourceX, int sourceY, int sourceWidth, int sourceHeight, int destX, int destY)
    {
        // work in lower-resolution coordinates
        destX >>= 1;
        destY >>= 1;
        var offX = destX - sourceX;
        var offY = destY - sourceY;
        ClampRect(ref sourceX, ref sourceY, ref sourceWidth, ref sourceHeight, 0, 0, source.Width, source.Height, 0, 0);
        ClampRect(ref sourceX, ref sourceY, ref sourceWidth, ref sourceHeight, 0, 0, Width >> 1, Height >> 1, offX, offY);

        if (sourceWidth <= 0)
            return; // nothing to copy

        // note: this could be optimized if needed...
        var maxX = sourceX + sourceWidth;
        var maxY = sourceY + sourceHeight;
        for (int y = sourceY; y < maxY; ++y)
        {
            for (int x = sourceX; x < maxX; ++x)
            {
                this[(x + offX) << 1, (y + offY) << 1] = this[((x + offX) << 1) + 1, (y + offY) << 1] = this[(x + offX) << 1, ((y + offY) << 1) + 1] = this[((x + offX) << 1) + 1, ((y + offY) << 1) + 1] = source[x, y];
            }
        }
    }

    // downsample a region from source bitmap into current bitmap; each 2x2 group of source pixels are copied into destination pixel if equal, otherwise destination is set to fallback value
    // note: if source x/y/w/h are odd, they are rounded down
    public void DownsampleRegion(Bitmap source, int sourceX, int sourceY, int sourceWidth, int sourceHeight, int destX, int destY, bool fallback)
    {
        // work in lower-resolution coordinates
        sourceX >>= 1;
        sourceY >>= 1;
        sourceWidth >>= 1;
        sourceHeight >>= 1;
        var offX = destX - sourceX;
        var offY = destY - sourceY;
        ClampRect(ref sourceX, ref sourceY, ref sourceWidth, ref sourceHeight, 0, 0, source.Width >> 1, source.Height >> 1, 0, 0);
        ClampRect(ref sourceX, ref sourceY, ref sourceWidth, ref sourceHeight, 0, 0, Width, Height, offX, offY);

        if (sourceWidth <= 0)
            return; // nothing to copy

        // note: this could be optimized if needed...
        var maxX = sourceX + sourceWidth;
        var maxY = sourceY + sourceHeight;
        for (int y = sourceY; y < maxY; ++y)
        {
            for (int x = sourceX; x < maxX; ++x)
            {
                var v = source[x << 1, y << 1];
                if (v != source[(x << 1) + 1, y << 1] || v != source[x << 1, (y << 1) + 1] || v != source[(x << 1) + 1, (y << 1) + 1])
                    v = fallback;
                this[x, y] = v;
            }
        }
    }

    // modify rect (x,y,w,h) so that, if offset by (dx,dy), it is clamped to rect (cx,cy,cw,ch)
    private static void ClampRect(ref int x, ref int y, ref int w, ref int h, int cx, int cy, int cw, int ch, int dx, int dy)
    {
        var offX = x + dx - cx; // offset of left border from clip rect
        if (offX < 0)
        {
            x -= offX;
            w += offX;
        }
        var offY = y + dy - cy;
        if (offY < 0)
        {
            y -= offY;
            h += offY;
        }
        var maxW = cx + cw - (x + dx); // maximal width that rect can have so that when offset it doesn't extend past right border
        if (w > maxW)
            w = maxW;
        var maxH = cy + ch - (y + dy);
        if (h > maxH)
            h = maxH;
    }
}
