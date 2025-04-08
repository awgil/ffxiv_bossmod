namespace BossMod;

// no longer a signed distance field now that we don't add extra forbidden zone cushion
public static class ShapeContains
{
    public static Func<WPos, bool> HalfPlane(WPos point, WDir normal) => p => normal.Dot(p - point) <= 0;

    public static Func<WPos, bool> Circle(WPos origin, float radius) => radius <= 0 ? (_ => false) : (p => p.InCircle(origin, radius));
    public static Func<WPos, bool> InvertedCircle(WPos origin, float radius) => radius <= 0 ? (_ => true) : (p => !p.InCircle(origin, radius));

    public static Func<WPos, bool> Donut(WPos origin, float innerRadius, float outerRadius) => p => p.InDonut(origin, innerRadius, outerRadius);

    public static Func<WPos, bool> Cone(WPos origin, float radius, Angle centerDir, Angle halfAngle) => p => p.InCircleCone(origin, radius, centerDir, halfAngle);

    public static Func<WPos, bool> InvertedCone(WPos origin, float radius, Angle centerDir, Angle halfAngle) => p => !p.InCircleCone(origin, radius, centerDir, halfAngle);

    public static Func<WPos, bool> DonutSector(WPos origin, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle) => p => p.InDonutCone(origin, innerRadius, outerRadius, centerDir, halfAngle);

    public static Func<WPos, bool> Tri(WPos origin, RelTriangle tri) => p => p.InTri(origin + tri.A, origin + tri.B, origin + tri.C);

    public static Func<WPos, bool> Rect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth) => p => p.InRect(origin, dir, lenFront, lenBack, halfWidth);
    public static Func<WPos, bool> Rect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => Rect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static Func<WPos, bool> Rect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return Rect(from, dir / l, l, 0, halfWidth);
    }

    public static Func<WPos, bool> InvertedRect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth) => p => !p.InRect(origin, dir, lenFront, lenBack, halfWidth);
    public static Func<WPos, bool> InvertedRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => InvertedRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static Func<WPos, bool> InvertedRect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return InvertedRect(from, dir / l, l, 0, halfWidth);
    }

    public static Func<WPos, bool> Capsule(WPos origin, WDir dir, float length, float radius) => p =>
    {
        var offset = p - origin;
        var t = Math.Clamp(offset.Dot(dir), 0, length);
        var proj = origin + t * dir;
        return (p - proj).Length() <= radius;
    };
    public static Func<WPos, bool> Capsule(WPos origin, Angle direction, float length, float radius) => Capsule(origin, direction.ToDirection(), length, radius);

    public static Func<WPos, bool> Cross(WPos origin, Angle direction, float length, float halfWidth) => p => p.InRect(origin, direction, length, length, halfWidth) || p.InRect(origin, direction + 90.Degrees(), length, length, halfWidth);

    // positive offset increases area
    public static Func<WPos, bool> ConvexPolygon(IEnumerable<(WPos, WPos)> edges, bool cw)
    {
        Func<WPos, bool> edge((WPos p1, WPos p2) e)
        {
            if (e.p1 == e.p2)
                return _ => true;
            var dir = (e.p2 - e.p1).Normalized();
            var normal = cw ? dir.OrthoL() : dir.OrthoR();
            return p => normal.Dot(p - e.p1) <= 0;
        }
        return Intersection([.. edges.Select(edge)]);
    }
    public static Func<WPos, bool> ConvexPolygon(IEnumerable<WPos> vertices, bool cw) => ConvexPolygon(PolygonUtil.EnumerateEdges(vertices), cw);

    public static Func<WPos, bool> Intersection(List<Func<WPos, bool>> funcs) => p => funcs.All(e => e(p));
    public static Func<WPos, bool> Union(List<Func<WPos, bool>> funcs) => p => funcs.Any(e => e(p));

    // special distance function for precise positioning, finer than map resolution
    // it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance
    public static Func<WPos, bool> PrecisePosition(WPos origin, WDir dir, float cellSize, WPos starting, float tolerance)
    {
        var delta = starting - origin;
        var dparr = delta.Dot(dir);
        if (dparr > tolerance && dparr <= cellSize)
            origin -= cellSize * dir;
        else if (dparr < -tolerance && dparr >= -cellSize)
            origin += cellSize * dir;

        var normal = dir.OrthoL();
        var dortho = delta.Dot(normal);
        if (dortho > tolerance && dortho <= cellSize)
            origin -= cellSize * normal;
        else if (dortho < -tolerance && dortho >= -cellSize)
            origin += cellSize * normal;

        return InvertedRect(origin, dir, cellSize, cellSize, cellSize);
    }
}
