using System.Runtime.Intrinsics;

namespace BossMod;

public static class ShapeDistance
{
    public static ShapeFun HalfPlane(WPos point, WDir normal)
    {
        var n = new WVec(normal);
        var p0 = new WVec(point);

        return p => n.Dot(p - p0);
    }

    public static ShapeFun Circle(WPos origin, float radius)
    {
        var ov = new WVec(origin);
        var rv = Vector512.Create(radius);

        return p => (p - ov).Length() - rv;
    }
    public static ShapeFun InvertedCircle(WPos origin, float radius)
    {
        var c = Circle(origin, radius);

        return p => -c(p);
    }

    public static ShapeFun Donut(WPos origin, float innerRadius, float outerRadius)
    {
        if (outerRadius <= 0 || innerRadius >= outerRadius)
            return _ => Vector512.Create(float.MaxValue);

        var midpoint = (outerRadius + innerRadius) * 0.5f;
        var thickness = Vector512.Create((outerRadius - innerRadius) * 0.5f);
        var cv = Circle(origin, midpoint);

        return p => Vector512.Abs(cv(p)) - thickness;
    }

    public static ShapeFun Cone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0 || radius <= 0)
            return _ => Vector512.Create(float.MaxValue);
        if (halfAngle.Rad >= MathF.PI)
            return Circle(origin, radius);

        WVec c = new(halfAngle.ToDirection().MirrorZ());
        WVec cx = new(-centerDir.ToDirection().MirrorX());
        WVec ov = new(origin);
        var rv = Vector512.Create(radius);

        return q =>
        {
            var p = (q - ov).Rotate(cx);
            p.X = Vector512.Abs(p.X);
            return Vector512.Max(p.Length() - rv, (p - c * Vector512.Clamp(p.Dot(c), V16.Zero, rv)).Length() * Vector512.CopySign(V16.One, c.Cross(p)));
        };
    }

    public static ShapeFun InvertedCone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        var cone = Cone(origin, radius, centerDir, halfAngle);
        return p => -cone(p);
    }

    static V16 Sign(V16 input) => Vector512.CopySign(V16.One, input);

    public static ShapeFun DonutSector(WPos origin, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0 || outerRadius <= 0 || innerRadius >= outerRadius)
            return _ => Vector512.Create(float.MaxValue);

        if (halfAngle.Rad >= MathF.PI)
            return Donut(origin, innerRadius, outerRadius);
        if (innerRadius <= 0)
            return Cone(origin, outerRadius, centerDir, halfAngle);

        var r = Vector512.Create((outerRadius + innerRadius) * 0.5f);
        var th = Vector512.Create((outerRadius - innerRadius) * 0.5f);

        WVec c = new(halfAngle.ToDirection().MirrorX());
        WVec cx = new(centerDir.ToDirection().MirrorX());

        var ov = new WVec(origin);

        return q =>
        {
            var p = (q - ov).Rotate(cx);
            p.X = Vector512.Abs(p.X);
            p = p.Rotate(c);
            return Vector512.Max(Vector512.Abs(p.Length() - r) - th, new WVec(p.X, Vector512.Max(V16.Zero, Vector512.Abs(r - p.Z) - th)).Length() * Sign(p.X));
        };
    }

    public static ShapeFun Tri(WPos w0, WPos w1, WPos w2)
    {
        WVec p0 = new(w0), p1 = new(w1), p2 = new(w2);
        WVec e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;

        return p =>
        {
            WVec v0 = p - p0, v1 = p - p2, v2 = p - p2;
            var pq0 = v0 - e0 * Vector512.Clamp(v0.Dot(e0) / e0.Dot(e0), V16.Zero, V16.One);
            var pq1 = v1 - e1 * Vector512.Clamp(v1.Dot(e1) / e1.Dot(e1), V16.Zero, V16.One);
            var pq2 = v2 - e2 * Vector512.Clamp(v2.Dot(e2) / e2.Dot(e2), V16.Zero, V16.One);
            var s = Sign(e0.Cross(e2));
            var d = new WVec(pq0.Dot(pq0), s * v0.Cross(e0))
                .Min(new WVec(pq1.Dot(pq1), s * v1.Cross(e1)))
                .Min(new WVec(pq2.Dot(pq2), s * v2.Cross(e2)));
            return Vector512.Negate(Vector512.Sqrt(d.X) * Sign(d.Z));
        };
    }

    public static ShapeFun Tri(WPos origin, RelTriangle rel) => Tri(origin + rel.A, origin + rel.B, origin + rel.C);

    public static ShapeFun Rect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        var start = origin - dir * lenBack;
        var end = origin + dir * lenFront;
        var midpoint = new WVec(WPos.Lerp(start, end, 0.5f));

        var r = new WVec(halfWidth, (lenBack + lenFront) * 0.5f);
        WVec dxr = new(dir.MirrorX());

        return p =>
        {
            var q = (p - midpoint).Rotate(dxr).Abs() - r;
            return q.ClampPositive().Length() + Vector512.Min(q.MaxComponent(), V16.Zero);
        };
    }
    public static ShapeFun Rect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => Rect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static ShapeFun Rect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return Rect(from, dir / l, l, 0, halfWidth);
    }

    public static ShapeFun InvertedRect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        var r = Rect(origin, dir, lenFront, lenBack, halfWidth);
        return p => -r(p);
    }
    public static ShapeFun InvertedRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => InvertedRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static ShapeFun InvertedRect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return InvertedRect(from, dir / l, l, 0, halfWidth);
    }

    public static Func<WPos, float> Capsule(WPos origin, WDir dir, float length, float radius) => p =>
    {
        var offset = p - origin;
        var t = Math.Clamp(offset.Dot(dir), 0, length);
        var proj = origin + t * dir;
        return (p - proj).Length() - radius;
    };
    public static Func<WPos, float> Capsule(WPos origin, Angle direction, float length, float radius) => Capsule(origin, direction.ToDirection(), length, radius);

    public static ShapeFun Cross(WPos origin, Angle direction, float length, float halfWidth)
    {
        if (halfWidth <= 0 || length <= 0)
            return _ => Vector512.Create(float.MaxValue);

        var dir = new WVec(direction.ToDirection().MirrorX());
        var b = new WVec(length, halfWidth);
        var ov = new WVec(origin);

        return qq =>
        {
            var p = (qq - ov).Rotate(dir).Abs();
            p = p.AlignX();
            var q = p - b;
            var k = Vector512.Max(q.Z, q.X);
            var kpos = Vector512.IsPositive(k);
            var w = WVec.Select(Vector512.IsPositive(k), q, new(b.Z - p.X, -k));
            return Sign(k) * w.ClampPositive().Length();
        };
    }

    public static ShapeFun Polygon(List<WPos> vertices)
    {
        var n = vertices.Count;
        var vv = vertices.Select(v => new WVec(v)).ToList();

        return p =>
        {
            var d = (p - vv[0]).Dot(p - vv[0]);
            var s = V16.One;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var e = vv[j] - vv[i];
                var w = p - vv[i];
                var b = w - e * Vector512.Clamp(w.Dot(e) / e.Dot(e), V16.Zero, V16.One);
                d = Vector512.Min(d, b.Dot(b));

                var c0 = Vector512.GreaterThanOrEqual(p.Z, vv[i].Z);
                var c1 = Vector512.LessThan(p.Z, vv[j].Z);
                var c2 = Vector512.GreaterThan(e.X * w.Z, e.Z * w.X);

                s *= Vector512.ConditionalSelect((c0 & c1 & c2) | ~(c0 | c1 | c2), Vector512.Create(-1f), V16.One);
            }
            return s * Vector512.Sqrt(d);
        };
    }
    public static ShapeFun Polygon(WPos origin, Angle rotation, RelPolygonWithHoles polygon)
    {
        var ext = new List<WPos>(polygon.Exterior.Length);
        foreach (var e in polygon.Exterior)
            ext.Add(e.Rotate(rotation) + origin);

        var dext = Polygon(ext);

        if (polygon.IsSimple)
            return dext;

        var interiors = new List<ShapeFun>();

        foreach (var h in polygon.Holes)
        {
            var i = new List<WPos>();
            foreach (var e in polygon.Interior(h))
                i.Add(e.Rotate(rotation) + origin);
            interiors.Add(Polygon(i));
        }

        return p =>
        {
            var dist = dext(p);

            foreach (var hole in interiors)
                dist = Vector512.Max(dist, Vector512.Negate(hole(p)));

            return dist;
        };
    }
    public static ShapeFun ComplexPolygon(WPos origin, Angle rotation, RelSimplifiedComplexPolygon polygon) => Union([.. polygon.Parts.Select(p => Polygon(origin, rotation, p))]);

    public static ShapeFun Intersection(List<ShapeFun> funcs)
    {
        if (funcs.Count == 0)
            throw new ArgumentException("IntersectionV: funcs is empty");

        return p =>
        {
            var d = funcs[0](p);
            for (var i = 1; i < funcs.Count; i++)
                d = Vector512.Max(d, funcs[i](p));
            return d;
        };
    }
    public static ShapeFun Union(List<ShapeFun> funcs)
    {
        if (funcs.Count == 0)
            throw new ArgumentException("UnionV: funcs is empty");

        return p =>
        {
            var d = funcs[0](p);
            for (var i = 1; i < funcs.Count; i++)
                d = Vector512.Min(d, funcs[i](p));
            return d;
        };
    }

    // special distance function for precise positioning, finer than map resolution
    // it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance
    // note that if the goal position is exactly aligned with a grid point, it results in a 2x2 safe area instead of 1x1 (this never happens in practice)
    public static ShapeFun PrecisePosition(WPos origin, WDir dir, float cellSize, WPos starting, float tolerance)
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
