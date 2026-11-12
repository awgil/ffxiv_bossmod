namespace BossMod;

public static class ShapeDistance
{
    public static DistFn HalfPlane(WPos point, WDir normal)
    {
        var n = new WVec(normal);
        var p0 = new WVec(point);

        return p => n.Dot(p - p0);
    }

    public static DistFn CircleMulti(WVec origin, float radius) => p => (p - origin).Length() - radius;
    public static DistFn Circle(WPos origin, float radius) => CircleMulti(new(origin), radius);
    public static DistFn InvertedCircle(WPos origin, float radius) => p => radius - (p - origin).Length();

    public static DistFn Donut(WPos origin, float innerRadius, float outerRadius)
    {
        var midpoint = (outerRadius + innerRadius) * 0.5f;
        var thickness = (outerRadius - innerRadius) * 0.5f;

        return Ring(Circle(origin, midpoint), thickness);
    }

    public static DistFn ConeMulti(WVec origin, float radius, WVec centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0 || radius <= 0)
            return _ => VV.Create(float.MaxValue);

        WVec c = new(halfAngle.ToDirection().MirrorZ());
        var cx = -centerDir.MirrorX();
        var rv = VV.Create(radius);

        return q =>
        {
            var p = (q - origin).Rotate(cx);
            p.X = VV.Abs(p.X);
            var l = p.Length() - rv;
            var m = (p - c * VV.Clamp(p.Dot(c), VFloat.Zero, rv)).Length();
            return VV.Max(l, m * c.Cross(p).Sign());
        };
    }

    public static DistFn Cone(WPos origin, float radius, WDir centerDir, Angle halfAngle) => ConeMulti(new(origin), radius, new(centerDir), halfAngle);
    public static DistFn Cone(WPos origin, float radius, Angle centerDir, Angle halfAngle) => ConeMulti(new(origin), radius, new(centerDir.ToDirection()), halfAngle);

    public static DistFn InvertedCone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        var cone = Cone(origin, radius, centerDir, halfAngle);
        return p => -cone(p);
    }

    public static DistFn DonutSector(WPos origin, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle)
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
            p.X = p.X.Abs();
            p = p.Rotate(c);
            return VV.Max(VV.Abs(p.Length() - r) - th, new WVec(p.X, VV.Max(VFloat.Zero, (r - p.Z).Abs() - th)).Length() * p.X.Sign());
        };
    }

    public static DistFn Tri(WPos w0, WPos w1, WPos w2)
    {
        WVec p0 = new(w0), p1 = new(w1), p2 = new(w2);
        WVec e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;

        return p =>
        {
            WVec v0 = p - p0, v1 = p - p2, v2 = p - p2;
            var pq0 = v0 - e0 * VV.Clamp(v0.Dot(e0) / e0.Dot(e0), VFloat.Zero, VFloat.One);
            var pq1 = v1 - e1 * VV.Clamp(v1.Dot(e1) / e1.Dot(e1), VFloat.Zero, VFloat.One);
            var pq2 = v2 - e2 * VV.Clamp(v2.Dot(e2) / e2.Dot(e2), VFloat.Zero, VFloat.One);
            var s = e0.Cross(e2).Sign();
            var d = new WVec(pq0.Dot(pq0), s * v0.Cross(e0))
                .Min(new WVec(pq1.Dot(pq1), s * v1.Cross(e1)))
                .Min(new WVec(pq2.Dot(pq2), s * v2.Cross(e2)));
            return -(VV.SquareRoot(d.X) * d.Z.Sign());
        };
    }

    public static DistFn Tri(WPos origin, RelTriangle rel) => Tri(origin + rel.A, origin + rel.B, origin + rel.C);

    public static DistFn Rect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        var start = origin - dir * lenBack;
        var end = origin + dir * lenFront;
        var midpoint = new WVec(WPos.Lerp(start, end, 0.5f));

        var r = new WVec(halfWidth, (lenBack + lenFront) * 0.5f);
        WVec dxr = new(dir.MirrorX());

        return p =>
        {
            var q = (p - midpoint).Rotate(dxr).Abs() - r;
            return q.ClampPositive().Length() + VV.Min(q.MaxComponent(), VFloat.Zero);
        };
    }
    public static DistFn Rect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => Rect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static DistFn Rect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return Rect(from, dir / l, l, 0, halfWidth);
    }

    public static DistFn RectMulti(WVec origin, WVec dir, float lenFront, float lenBack, float halfWidth)
    {
        var start = origin - dir * lenBack;
        var end = origin + dir * lenFront;
        var midpoint = WVec.Lerp(start, end, VV.Create(0.5f));

        var r = new WVec(halfWidth, (lenBack + lenFront) * 0.5f);
        var dxr = dir.MirrorX();

        return p =>
        {
            var q = (p - midpoint).Rotate(dxr).Abs() - r;
            return q.ClampPositive().Length() + VV.Min(q.MaxComponent(), VFloat.Zero);
        };
    }

    public static DistFn InvertedRect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        var r = Rect(origin, dir, lenFront, lenBack, halfWidth);
        return p => -r(p);
    }
    public static DistFn InvertedRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => InvertedRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static DistFn InvertedRect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return InvertedRect(from, dir / l, l, 0, halfWidth);
    }

    public static DistFn Capsule(WPos origin, WDir dir, float length, float radius)
    {
        var a = new WVec(origin);
        var ba = new WVec(dir * length);
        var rv = VV.Create(radius);

        return p =>
        {
            var pa = p - a;
            var h = VV.Clamp(pa.Dot(ba) / ba.Dot(ba), VFloat.Zero, VFloat.One);
            return (pa - ba * h).Length() - rv;
        };
    }
    public static DistFn Capsule(WPos origin, Angle direction, float length, float radius) => Capsule(origin, direction.ToDirection(), length, radius);

    public static DistFn Cross(WPos origin, Angle direction, float length, float halfWidth)
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
            var w = WVec.Select(VV.IsPositive(VV.Max(q.Z, q.X)).Mask(), q, new(b.Z - p.X, -VV.Max(q.Z, q.X)));
            return VV.Max(q.Z, q.X).Sign() * w.ClampPositive().Length();
        };
    }

    public static DistFn Polygon(IEnumerable<WPos> vertices)
    {
        var vv = vertices.Select(v => new WVec(v)).ToList();
        var n = vv.Count;

        return p =>
        {
            var d = (p - vv[0]).LengthSq();
            var s = VFloat.One;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var e = vv[j] - vv[i];
                var w = p - vv[i];
                var b = w - e * VV.Clamp(w.Dot(e) / e.Dot(e), VFloat.Zero, VFloat.One);
                d = VV.Min(d, b.LengthSq());

                var c0 = p.Z >= vv[i].Z;
                var c1 = p.Z < vv[j].Z;
                var c2 = e.X * w.Z > e.Z * w.X;

                s *= VV.ConditionalSelect((c0 & c1 & c2) | ~(c0 | c1 | c2), -VFloat.One, VFloat.One);
            }
            return s * VV.SquareRoot(d);
        };
    }
    public static DistFn Polygon(WPos origin, Angle rotation, RelPolygonWithHoles polygon)
    {
        var ext = new List<WPos>(polygon.Exterior.Length);
        foreach (var e in polygon.Exterior)
            ext.Add(e.Rotate(rotation) + origin);

        var dext = Polygon(ext);

        if (polygon.IsSimple)
            return dext;

        var interiors = new List<DistFn>();

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
                dist = VV.Max(dist, -hole(p));

            return dist;
        };
    }
    public static DistFn Polygon(WPos origin, Angle rotation, RelSimplifiedComplexPolygon polygon) => Union([.. polygon.Parts.Select(p => Polygon(origin, rotation, p))]);

    public static DistFn Intersection(List<DistFn> funcs)
    {
        if (funcs.Count == 0)
            return _ => VV.Create(float.MaxValue);

        return p =>
        {
            var d = VV.Create(float.MinValue);
            foreach (var f in funcs)
                d = VV.Max(d, f(p));
            return d;
        };
    }
    public static DistFn Union(List<DistFn> funcs)
    {
        if (funcs.Count == 0)
            return _ => VV.Create(float.MaxValue);

        return p =>
        {
            var d = VV.Create(float.MaxValue);
            foreach (var f in funcs)
                d = VV.Min(d, f(p));
            return d;
        };
    }

    public static DistFn Xor(DistFn a, DistFn b)
    {
        return p =>
        {
            var pa = a(p);
            var pb = b(p);
            return VV.Max(VV.Min(pa, pb), -VV.Max(pa, pb));
        };
    }

    public static DistFn Ring(DistFn input, float thickness) => p => input(p).Abs() - thickness;

    public static Func<WVec, VInt> Count(List<DistFn> fs)
    {
        return p =>
        {
            var sum = VInt.Zero;
            foreach (var f in fs)
                sum += (f(p) < VFloat.Zero).WithValue(VInt.One);
            return sum;
        };
    }

    // special distance function for precise positioning, finer than map resolution
    // it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance
    public static DistFn PrecisePosition(WPos origin, WDir dir, float cellSize, WPos starting, float tolerance)
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
