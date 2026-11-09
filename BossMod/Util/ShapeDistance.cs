namespace BossMod;

public static class ShapeDistance
{
    public static ShapeDist HalfPlane(WPos point, WDir normal)
    {
        var n = new WVec(normal);
        var p0 = new WVec(point);

        return p => n.Dot(p - p0);
    }

    public static ShapeDist Circle(WPos origin, float radius)
    {
        var ov = new WVec(origin);
        var rv = VV.Create(radius);

        return p => (p - ov).Length() - rv;
    }
    public static ShapeDist InvertedCircle(WPos origin, float radius)
    {
        var c = Circle(origin, radius);

        return p => -c(p);
    }

    public static ShapeDist Donut(WPos origin, float innerRadius, float outerRadius)
    {
        if (outerRadius <= 0 || innerRadius >= outerRadius)
            return _ => VV.Create(float.MaxValue);

        var midpoint = (outerRadius + innerRadius) * 0.5f;
        var thickness = VV.Create((outerRadius - innerRadius) * 0.5f);
        var cv = Circle(origin, midpoint);

        return p => VV.Abs(cv(p)) - thickness;
    }

    public static ShapeDist Cone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0 || radius <= 0)
            return _ => VV.Create(float.MaxValue);
        if (halfAngle.Rad >= MathF.PI)
            return Circle(origin, radius);

        WVec c = new(halfAngle.ToDirection().MirrorZ());
        WVec cx = new(-centerDir.ToDirection().MirrorX());
        WVec ov = new(origin);
        var rv = VV.Create(radius);

        return q =>
        {
            var p = (q - ov).Rotate(cx);
            p.X = VV.Abs(p.X);
            return VV.Max(p.Length() - rv, (p - c * VV.Clamp(p.Dot(c), VF.Zero, rv)).Length() * VV.CopySign(VF.One, c.Cross(p)));
        };
    }

    public static ShapeDist InvertedCone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        var cone = Cone(origin, radius, centerDir, halfAngle);
        return p => -cone(p);
    }

    static VF Sign(VF input) => VV.CopySign(VF.One, input);

    public static ShapeDist DonutSector(WPos origin, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0 || outerRadius <= 0 || innerRadius >= outerRadius)
            return _ => VV.Create(float.MaxValue);

        if (halfAngle.Rad >= MathF.PI)
            return Donut(origin, innerRadius, outerRadius);
        if (innerRadius <= 0)
            return Cone(origin, outerRadius, centerDir, halfAngle);

        var r = VV.Create((outerRadius + innerRadius) * 0.5f);
        var th = VV.Create((outerRadius - innerRadius) * 0.5f);

        WVec c = new(halfAngle.ToDirection().MirrorX());
        WVec cx = new(centerDir.ToDirection().MirrorX());

        return q =>
        {
            var p = (q - origin).Rotate(cx);
            p.X = VV.Abs(p.X);
            p = p.Rotate(c);
            return VV.Max(VV.Abs(p.Length() - r) - th, new WVec(p.X, VV.Max(VF.Zero, VV.Abs(r - p.Z) - th)).Length() * Sign(p.X));
        };
    }

    public static ShapeDist Tri(WPos w0, WPos w1, WPos w2)
    {
        WVec p0 = new(w0), p1 = new(w1), p2 = new(w2);
        WVec e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;

        return p =>
        {
            WVec v0 = p - p0, v1 = p - p2, v2 = p - p2;
            var pq0 = v0 - e0 * VV.Clamp(v0.Dot(e0) / e0.Dot(e0), VF.Zero, VF.One);
            var pq1 = v1 - e1 * VV.Clamp(v1.Dot(e1) / e1.Dot(e1), VF.Zero, VF.One);
            var pq2 = v2 - e2 * VV.Clamp(v2.Dot(e2) / e2.Dot(e2), VF.Zero, VF.One);
            var s = Sign(e0.Cross(e2));
            var d = new WVec(pq0.Dot(pq0), s * v0.Cross(e0))
                .Min(new WVec(pq1.Dot(pq1), s * v1.Cross(e1)))
                .Min(new WVec(pq2.Dot(pq2), s * v2.Cross(e2)));
            return VV.Negate(VV.SquareRoot(d.X) * Sign(d.Z));
        };
    }

    public static ShapeDist Tri(WPos origin, RelTriangle rel) => Tri(origin + rel.A, origin + rel.B, origin + rel.C);

    public static ShapeDist Rect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        var start = origin - dir * lenBack;
        var end = origin + dir * lenFront;
        var midpoint = new WVec(WPos.Lerp(start, end, 0.5f));

        var r = new WVec(halfWidth, (lenBack + lenFront) * 0.5f);
        WVec dxr = new(dir.MirrorX());

        return p =>
        {
            var q = (p - midpoint).Rotate(dxr).Abs() - r;
            return q.ClampPositive().Length() + VV.Min(q.MaxComponent(), VF.Zero);
        };
    }
    public static ShapeDist Rect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => Rect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static ShapeDist Rect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return Rect(from, dir / l, l, 0, halfWidth);
    }

    public static ShapeDist InvertedRect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        var r = Rect(origin, dir, lenFront, lenBack, halfWidth);
        return p => -r(p);
    }
    public static ShapeDist InvertedRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => InvertedRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static ShapeDist InvertedRect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return InvertedRect(from, dir / l, l, 0, halfWidth);
    }

    public static ShapeDist Capsule(WPos origin, WDir dir, float length, float radius)
    {
        var a = new WVec(origin);
        var ba = new WVec(dir * length);
        var rv = VV.Create(radius);

        return p =>
        {
            var pa = p - a;
            var h = VV.Clamp(pa.Dot(ba) / ba.Dot(ba), VF.Zero, VF.One);
            return (pa - ba * h).Length() - rv;
        };
    }
    public static ShapeDist Capsule(WPos origin, Angle direction, float length, float radius) => Capsule(origin, direction.ToDirection(), length, radius);

    public static ShapeDist Cross(WPos origin, Angle direction, float length, float halfWidth)
    {
        if (halfWidth <= 0 || length <= 0)
            return _ => VV.Create(float.MaxValue);

        var dir = new WVec(direction.ToDirection().MirrorX());
        var b = new WVec(length, halfWidth);

        return qq =>
        {
            var p = (qq - origin).Rotate(dir).Abs();
            p = p.AlignX();
            var q = p - b;
            var k = VV.Max(q.Z, q.X);
            var kpos = VV.IsPositive(k);
            var w = WVec.Select(VV.IsPositive(k), q, new(b.Z - p.X, -k));
            return Sign(k) * w.ClampPositive().Length();
        };
    }

    public static ShapeDist Polygon(IEnumerable<WPos> vertices)
    {
        var vv = vertices.Select(v => new WVec(v)).ToList();
        var n = vv.Count;

        return p =>
        {
            var d = (p - vv[0]).Dot(p - vv[0]);
            var s = VF.One;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var e = vv[j] - vv[i];
                var w = p - vv[i];
                var b = w - e * VV.Clamp(w.Dot(e) / e.Dot(e), VF.Zero, VF.One);
                d = VV.Min(d, b.Dot(b));

                var c0 = VV.GreaterThanOrEqual(p.Z, vv[i].Z);
                var c1 = VV.LessThan(p.Z, vv[j].Z);
                var c2 = VV.GreaterThan(e.X * w.Z, e.Z * w.X);

                s *= VV.ConditionalSelect((c0 & c1 & c2) | ~(c0 | c1 | c2), VV.Create(-1f), VF.One);
            }
            return s * VV.SquareRoot(d);
        };
    }
    public static ShapeDist Polygon(WPos origin, Angle rotation, RelPolygonWithHoles polygon)
    {
        var ext = new List<WPos>(polygon.Exterior.Length);
        foreach (var e in polygon.Exterior)
            ext.Add(e.Rotate(rotation) + origin);

        var dext = Polygon(ext);

        if (polygon.IsSimple)
            return dext;

        var interiors = new List<ShapeDist>();

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
                dist = VV.Max(dist, VV.Negate(hole(p)));

            return dist;
        };
    }
    public static ShapeDist ComplexPolygon(WPos origin, Angle rotation, RelSimplifiedComplexPolygon polygon) => Union([.. polygon.Parts.Select(p => Polygon(origin, rotation, p))]);

    public static ShapeDist Intersection(List<ShapeDist> funcs) => p => funcs.Max(f => f(p));
    public static ShapeDist Union(List<ShapeDist> funcs) => p => funcs.Min(f => f(p));

    public static ShapeDist Xor(ShapeDist a, ShapeDist b)
    {
        return p =>
        {
            var pa = a(p);
            var pb = b(p);
            return VV.Max(VV.Min(pa, pb), -VV.Max(pa, pb));
        };
    }

    // special distance function for precise positioning, finer than map resolution
    // it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance
    // note that if the goal position is exactly aligned with a grid point, it results in a 2x2 safe area instead of 1x1 (this never happens in practice)
    public static ShapeDist PrecisePosition(WPos origin, WDir dir, float cellSize, WPos starting, float tolerance)
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
