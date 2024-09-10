namespace BossMod.Pathfinding;

// 'map' used for running pathfinding algorithms
// this is essentially a square grid representing an arena (or immediate neighbourhood of the player) where we rasterize forbidden/desired zones
// area covered by each pixel can be in one of the following states:
// - default: safe to traverse but non-goal
// - danger: unsafe to traverse after X seconds (X >= 0); instead of X, we store max 'g' value (distance travelled assuming constant speed) for which pixel is still considered unblocked
// - goal: destination with X priority (X > 0); 'default' is considered a goal with priority 0
// - goal and danger are mutually exclusive, 'danger' overriding 'goal' state
public class Map
{
    public float Resolution { get; private set; } // pixel size, in world units
    public int Width { get; private set; } // always even
    public int Height { get; private set; } // always even
    public float[] PixelMaxG = []; // == MaxValue if not dangerous (TODO: consider changing to a byte per pixel?)
    public sbyte[] PixelPriority = [];

    public WPos Center { get; private set; } // position of map center in world units
    public Angle Rotation { get; private set; } // rotation relative to world space (=> ToDirection() is equal to direction of local 'height' axis in world space)
    public WDir LocalZDivRes { get; private set; }

    public float MaxG; // maximal 'maxG' value of all blocked pixels
    public int MaxPriority; // maximal 'priority' value of all goal pixels

    // min-max bounds of 'interesting' area, default to (0,0) to (width-1,height-1)
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;

    public Map() { }
    public Map(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = default) => Init(resolution, center, worldHalfWidth, worldHalfHeight, rotation);

    public void Init(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = default)
    {
        Resolution = resolution;
        Width = 2 * (int)MathF.Ceiling(worldHalfWidth / resolution);
        Height = 2 * (int)MathF.Ceiling(worldHalfHeight / resolution);

        var numPixels = Width * Height;
        if (PixelMaxG.Length < numPixels)
            PixelMaxG = new float[numPixels];
        Array.Fill(PixelMaxG, float.MaxValue, 0, numPixels); // fill is unconditional, can we avoid it by changing storage?..
        if (PixelPriority.Length < numPixels)
            PixelPriority = new sbyte[numPixels];
        else
            Array.Fill(PixelPriority, (sbyte)0, 0, numPixels);

        Center = center;
        Rotation = rotation;
        LocalZDivRes = rotation.ToDirection() / Resolution;

        MaxG = 0;
        MaxPriority = 0;

        MinX = MinY = 0;
        MaxX = Width - 1;
        MaxY = Height - 1;
    }

    public void Init(Map source, WPos center)
    {
        Resolution = source.Resolution;
        Width = source.Width;
        Height = source.Height;

        var numPixels = Width * Height;
        if (PixelMaxG.Length < numPixels)
            PixelMaxG = new float[numPixels];
        Array.Copy(source.PixelMaxG, PixelMaxG, numPixels);
        if (PixelPriority.Length < numPixels)
            PixelPriority = new sbyte[numPixels];
        Array.Copy(source.PixelPriority, PixelPriority, numPixels);

        Center = center;
        Rotation = source.Rotation;
        LocalZDivRes = source.LocalZDivRes;

        MaxG = source.MaxG;
        MaxPriority = source.MaxPriority;

        MinX = source.MinX;
        MinY = source.MinY;
        MaxX = source.MaxX;
        MaxY = source.MaxY;
    }

    public Vector2 WorldToGridFrac(WPos world)
    {
        var offset = world - Center;
        var x = offset.Dot(LocalZDivRes.OrthoL());
        var y = offset.Dot(LocalZDivRes);
        return new(Width / 2 + x, Height / 2 + y);
    }

    public int GridToIndex(int x, int y) => y * Width + x;
    public (int x, int y) IndexToGrid(int index) => (index % Width, index / Width);
    public (int x, int y) FracToGrid(Vector2 frac) => ((int)MathF.Floor(frac.X), (int)MathF.Floor(frac.Y));
    public (int x, int y) WorldToGrid(WPos world) => FracToGrid(WorldToGridFrac(world));
    public (int x, int y) ClampToGrid((int x, int y) pos) => (Math.Clamp(pos.x, 0, Width - 1), Math.Clamp(pos.y, 0, Height - 1));
    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public WPos GridToWorld(int gx, int gy, float fx, float fy)
    {
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        float ax = (gx - Width / 2 + fx) * rsq;
        float az = (gy - Height / 2 + fy) * rsq;
        return Center + ax * LocalZDivRes.OrthoL() + az * LocalZDivRes;
    }

    // block all pixels for which function returns value smaller than threshold ('inside' shape + extra cushion)
    public void BlockPixelsInside(Func<WPos, float> shape, float maxG, float threshold)
    {
        MaxG = MathF.Max(MaxG, maxG);
        foreach (var (x, y, center) in EnumeratePixels())
        {
            if (shape(center) < threshold)
            {
                ref var pixel = ref PixelMaxG[y * Width + x];
                pixel = MathF.Min(pixel, maxG);
            }
        }
    }

    public IEnumerable<(int x, int y, WPos center)> EnumeratePixels()
    {
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        var dx = LocalZDivRes.OrthoL() * rsq;
        var dy = LocalZDivRes * rsq;
        var cy = Center + (-Width / 2 + 0.5f) * dx + (-Height / 2 + 0.5f) * dy;
        for (int y = 0; y < Height; y++)
        {
            var cx = cy;
            for (int x = 0; x < Width; ++x)
            {
                yield return (x, y, cx);
                cx += dx;
            }
            cy += dy;
        }
    }

    // enumerate pixels along line starting from (x1, y1) to (x2, y2); first is not returned, last is returned
    public IEnumerable<(int x, int y)> EnumeratePixelsInLine(int x1, int y1, int x2, int y2)
    {
        int dx = x2 - x1;
        int dy = y2 - y1;
        int sx = dx > 0 ? 1 : -1;
        int sy = dy > 0 ? 1 : -1;
        dx = Math.Abs(dx);
        dy = Math.Abs(dy);
        if (dx >= dy)
        {
            int err = 2 * dy - dx;
            do
            {
                x1 += sx;
                yield return (x1, y1);
                if (err > 0)
                {
                    y1 += sy;
                    yield return (x1, y1);
                    err -= 2 * dx;
                }
                err += 2 * dy;
            }
            while (x1 != x2);
        }
        else
        {
            int err = 2 * dx - dy;
            do
            {
                y1 += sy;
                yield return (x1, y1);
                if (err > 0)
                {
                    x1 += sx;
                    yield return (x1, y1);
                    err -= 2 * dy;
                }
                err += 2 * dx;
            }
            while (y1 != y2);
        }
    }
}
