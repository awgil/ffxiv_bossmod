namespace BossMod;

public static class BitmapPathfindExtensions
{
    public static bool TryWorldToBitmapCell(this Bitmap map, Bitmap.Rect rect, WPos mapCenter, WPos pos, out int x, out int y)
    {
        var centerCellX = (rect.Left + rect.Right) * 0.5f;
        var centerCellY = (rect.Top + rect.Bottom) * 0.5f;
        var invRes = 1.0f / map.PixelSize;
        var delta = (pos - mapCenter) * invRes;
        x = (int)MathF.Round(centerCellX + delta.X);
        y = (int)MathF.Round(centerCellY + delta.Z);
        return (uint)x < map.Width && (uint)y < map.Height;
    }

    public static WPos CellCenterToWorld(this Bitmap map, Bitmap.Rect rect, WPos mapCenter, int x, int y)
    {
        var centerCellX = (rect.Left + rect.Right) * 0.5f;
        var centerCellY = (rect.Top + rect.Bottom) * 0.5f;
        var ps = map.PixelSize;
        return new WPos(
            mapCenter.X + (x - centerCellX) * ps,
            mapCenter.Z + (y - centerCellY) * ps);
    }

    public static bool HasObstacleMapLineOfSight(this Bitmap map, Bitmap.Rect rect, WPos mapCenter, WPos from, WPos to)
    {
        if (!map.TryWorldToBitmapCell(rect, mapCenter, from, out var x0, out var y0) || !map.TryWorldToBitmapCell(rect, mapCenter, to, out var x1, out var y1))
            return true;

        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var err = dx + dy;
        var x = x0;
        var y = y0;

        while (true)
        {
            if ((uint)x < map.Width && (uint)y < map.Height && map[x, y])
                return false;
            if (x == x1 && y == y1)
                return true;
            var e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    public static bool TryWorldToBitmapCell(this Bitmap.Region region, WPos mapCenter, WPos pos, out int x, out int y)
        => region.Bitmap.TryWorldToBitmapCell(region.Rect, mapCenter, pos, out x, out y);

    public static WPos CellCenterToWorld(this Bitmap.Region region, WPos mapCenter, int x, int y)
        => region.Bitmap.CellCenterToWorld(region.Rect, mapCenter, x, y);

    public static bool HasObstacleMapLineOfSight(this Bitmap.Region region, WPos mapCenter, WPos from, WPos to)
        => region.Bitmap.HasObstacleMapLineOfSight(region.Rect, mapCenter, from, to);
}
