using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BossMod;

// TODO: replace me with union when it gets implemented
[StructLayout(LayoutKind.Explicit)]
public struct Sdf
{
    public enum SdfType
    {
        Continuous,
        Discrete,
        Indexed
    }

    [FieldOffset(0)] private Func<WPos, float> SdfCont;
    [FieldOffset(0)] private Func<WPos, bool> SdfDisc;
    [FieldOffset(0)] private Func<WPos, int, bool> SdfIx;
    [FieldOffset(8)] public SdfType Type;

    public static Sdf Continuous(Func<WPos, float> d) => new() { Type = SdfType.Continuous, SdfCont = d };
    public static Sdf Discrete(Func<WPos, bool> b) => new() { Type = SdfType.Discrete, SdfDisc = b };
    public static Sdf Indexed(Func<WPos, int, bool> i) => new() { Type = SdfType.Indexed, SdfIx = i };

    public readonly Sdf Inverted()
    {
        switch (Type)
        {
            case SdfType.Continuous:
                var fc = SdfCont;
                return Continuous(p => -fc(p));
            case SdfType.Discrete:
                var fd = SdfDisc;
                return Discrete(p => !fd(p));
            case SdfType.Indexed:
                var fi = SdfIx;
                return Indexed((p, i) => !fi(p, i));
            default:
                throw new UnreachableException();
        }
    }

#pragma warning disable IDE0066 // Convert switch statement to expression
    public readonly bool Check(WPos p)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont(p) < 0;

            case SdfType.Discrete:
                return SdfDisc(p);

            case SdfType.Indexed:
                return SdfIx(p, -1);

            default:
                throw new UnreachableException();
        }
    }

    public readonly float Distance(WPos p)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont(p);

            case SdfType.Discrete:
                return SdfDisc(p) ? float.MinValue : float.MaxValue;

            case SdfType.Indexed:
                return SdfIx(p, -1) ? float.MinValue : float.MaxValue;

            default:
                throw new UnreachableException();
        }
    }

    public readonly bool CheckIndexed(WPos p, int index)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont(p) < 0;

            case SdfType.Discrete:
                return SdfDisc(p);

            case SdfType.Indexed:
                return SdfIx(p, index);

            default:
                throw new UnreachableException();
        }
    }
#pragma warning restore IDE0066 // Convert switch statement to expression

    public readonly bool IsContinuous => Type == SdfType.Continuous;
}

public static class ShapeDistance
{
    public static Func<WPos, float> HalfPlane(WPos point, WDir normal) => p => normal.Dot(p - point);

    public static Func<WPos, float> Circle(WPos origin, float radius) => radius <= 0 ? (_ => float.MaxValue) : (p => (p - origin).Length() - radius);
    public static Func<WPos, float> InvertedCircle(WPos origin, float radius) => radius <= 0 ? (_ => float.MinValue) : (p => radius - (p - origin).Length());

    public static Func<WPos, float> Donut(WPos origin, float innerRadius, float outerRadius)
    {
        if (outerRadius <= 0 || innerRadius >= outerRadius)
            return _ => float.MaxValue;
        if (innerRadius <= 0)
            return Circle(origin, outerRadius);
        return p =>
        {
            // intersection of outer circle and inverted inner circle
            var distOrigin = (p - origin).Length();
            var distOuter = distOrigin - outerRadius;
            var distInner = innerRadius - distOrigin;
            return Math.Max(distOuter, distInner);
        };
    }

    public static Func<WPos, float> Cone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0 || radius <= 0)
            return _ => float.MaxValue;
        if (halfAngle.Rad >= MathF.PI)
            return Circle(origin, radius);

        var c = halfAngle.ToDirection().MirrorZ();
        var cx = -centerDir.ToDirection().MirrorX();

        return q =>
        {
            var p = (q - origin).Rotate(cx);
            p.X = Math.Abs(p.X);
            return Math.Max(p.Length() - radius, (p - c * Math.Clamp(p.Dot(c), 0, radius)).Length() * Math.Sign(c.Cross(p)));
        };
    }

    public static Func<WPos, float> InvertedCone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        var cone = Cone(origin, radius, centerDir, halfAngle);
        return p => -cone(p);
    }

    public static Func<WPos, float> DonutSector(WPos origin, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0 || outerRadius <= 0 || innerRadius >= outerRadius)
            return _ => float.MaxValue;
        if (halfAngle.Rad >= MathF.PI)
            return Donut(origin, innerRadius, outerRadius);
        if (innerRadius <= 0)
            return Cone(origin, outerRadius, centerDir, halfAngle);

        var r = (outerRadius + innerRadius) * 0.5f;
        var th = (outerRadius - innerRadius) * 0.5f;

        var c = halfAngle.ToDirection().MirrorX();
        var cx = centerDir.ToDirection().MirrorX();

        return q =>
        {
            var p = (q - origin).Rotate(cx);
            p.X = MathF.Abs(p.X);
            p = p.Rotate(c);
            return Math.Max(MathF.Abs(p.Length() - r) - th, new WDir(p.X, MathF.Max(0, MathF.Abs(r - p.Z) - th)).Length() * Math.Sign(p.X));
        };
    }

    public static Func<WPos, float> Tri(WPos origin, RelTriangle tri)
    {
        var ab = tri.B - tri.A;
        var bc = tri.C - tri.B;
        var ca = tri.A - tri.C;
        var n1 = ab.OrthoL().Normalized();
        var n2 = bc.OrthoL().Normalized();
        var n3 = ca.OrthoL().Normalized();
        if (ab.Cross(bc) < 0)
        {
            n1 = -n1;
            n2 = -n2;
            n3 = -n3;
        }
        var a = origin + tri.A;
        var b = origin + tri.B;
        var c = origin + tri.C;
        return p =>
        {
            var d1 = n1.Dot(p - a);
            var d2 = n2.Dot(p - b);
            var d3 = n3.Dot(p - c);
            return Math.Max(Math.Max(d1, d2), d3);
        };
    }

    static WDir Max0(WDir a) => new(Math.Max(0, a.X), Math.Max(0, a.Z));
    static float Maxcomp(WDir a) => Math.Max(a.X, a.Z);

    public static Func<WPos, float> Rect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        var start = origin - dir * lenBack;
        var end = origin + dir * lenFront;
        var midpoint = WPos.Lerp(start, end, 0.5f);

        var r = new WDir(halfWidth, (lenBack + lenFront) * 0.5f);
        var dxr = dir.MirrorX();

        return p =>
        {
            var q = (p - midpoint).Rotate(dxr).Abs() - r;
            return Max0(q).Length() + Math.Min(Maxcomp(q), 0);
        };
    }
    public static Func<WPos, float> Rect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => Rect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static Func<WPos, float> Rect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        return Rect(from, dir / l, l, 0, halfWidth);
    }

    public static Func<WPos, float> InvertedRect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        var r = Rect(origin, dir, lenFront, lenBack, halfWidth);
        return p => -r(p);
    }
    public static Func<WPos, float> InvertedRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => InvertedRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    public static Func<WPos, float> InvertedRect(WPos from, WPos to, float halfWidth)
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

    public static Func<WPos, float> Cross(WPos origin, Angle direction, float length, float halfWidth)
    {
        if (halfWidth <= 0 || length <= 0)
            return _ => float.MaxValue;

        var dir = direction.ToDirection().MirrorX();
        var b = new WDir(length, halfWidth);

        return qq =>
        {
            var p = (qq - origin).Rotate(dir).Abs();
            if (p.Z > p.X)
                p = new(p.Z, p.X);
            var q = p - b;
            var k = Math.Max(q.Z, q.X);
            var w = k > 0 ? q : new(b.Z - p.X, -k);
            return Math.Sign(k) * Max0(w).Length();
        };
    }

    // positive offset increases area
    public static Func<WPos, float> ConvexPolygon(IEnumerable<(WPos, WPos)> edges, bool cw)
    {
        Func<WPos, float> edge((WPos p1, WPos p2) e)
        {
            if (e.p1 == e.p2)
                return _ => float.MinValue;
            var dir = (e.p2 - e.p1).Normalized();
            var normal = cw ? dir.OrthoL() : dir.OrthoR();
            return p => normal.Dot(p - e.p1);
        }
        return Intersection([.. edges.Select(edge)]);
    }
    public static Func<WPos, float> ConvexPolygon(IEnumerable<WPos> vertices, bool cw) => ConvexPolygon(PolygonUtil.EnumerateEdges(vertices), cw);
    public static Func<WPos, float> Polygon(List<WPos> vertices)
    {
        var n = vertices.Count;

        return p =>
        {
            var d = (p - vertices[0]).Dot(p - vertices[0]);
            var s = 1f;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var e = vertices[j] - vertices[i];
                var w = p - vertices[i];
                var b = w - e * Math.Clamp(w.Dot(e) / e.Dot(e), 0, 1);
                d = Math.Min(d, b.Dot(b));

                var c0 = p.Z >= vertices[i].Z;
                var c1 = p.Z < vertices[j].Z;
                var c2 = e.X * w.Z > e.Z * w.X;
                if (c0 == c1 && c1 == c2)
                    s *= -1;
            }
            return s * MathF.Sqrt(d);
        };
    }
    public static Func<WPos, float> Polygon(WPos origin, Angle rotation, RelPolygonWithHoles polygon)
    {
        var ext = new List<WPos>(polygon.Exterior.Length);
        foreach (var e in polygon.Exterior)
            ext.Add(e.Rotate(rotation) + origin);

        var dext = Polygon(ext);

        if (polygon.IsSimple)
            return dext;

        var interiors = new List<Func<WPos, float>>();

        foreach (var h in polygon.Holes)
        {
            var i = new List<WPos>();
            foreach (var e in polygon.Interior(h))
                i.Add(e.Rotate(rotation) + origin);
            interiors.Add(Polygon(i));
        }

        return p =>
        {
            var dOut = dext(p);
            if (dOut >= 0)
                return dOut;

            foreach (var hole in interiors)
            {
                var dh = hole(p);
                if (dh < 0)
                    return -dh;
            }

            return dOut;
        };
    }

    public static Func<WPos, float> ComplexPolygon(WPos origin, Angle rotation, RelSimplifiedComplexPolygon polygon) => Union([.. polygon.Parts.Select(p => Polygon(origin, rotation, p))]);

    public static Func<WPos, float> Intersection(List<Func<WPos, float>> funcs) => p => funcs.Max(e => e(p));
    public static Func<WPos, float> Union(List<Func<WPos, float>> funcs) => p => funcs.Min(e => e(p));

    // special distance function for precise positioning, finer than map resolution
    // it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance
    // note that if the goal position is exactly aligned with a grid point, it results in a 2x2 safe area instead of 1x1 (this never happens in practice)
    public static Func<WPos, float> PrecisePosition(WPos origin, WDir dir, float cellSize, WPos starting, float tolerance)
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
