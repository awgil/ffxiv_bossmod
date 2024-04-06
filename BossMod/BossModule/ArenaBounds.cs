namespace BossMod;

// note: if arena bounds are changed, new instance is recreated
// max approx error can change without recreating the instance
// you can use hash-code to cache clipping results - it will change whenever anything in the instance changes
public abstract class ArenaBounds
{
    public WPos Center { get; init; }
    public float HalfSize { get; init; } // largest horizontal/vertical dimension: radius for circle, max of width/height for rect

    // fields below are used for clipping
    public float MaxApproxError { get; private set; }

    private Clip2D _clipper = new();
    public IEnumerable<WPos> ClipPoly => _clipper.ClipPoly;
    public List<(WPos, WPos, WPos)> ClipAndTriangulate(ClipperLib.PolyTree poly) => _clipper.ClipAndTriangulate(poly);
    public List<(WPos, WPos, WPos)> ClipAndTriangulate(IEnumerable<WPos> poly) => _clipper.ClipAndTriangulate(poly);

    private float _screenHalfSize;
    public float ScreenHalfSize
    {
        get => _screenHalfSize;
        set
        {
            if (_screenHalfSize != value)
            {
                _screenHalfSize = value;
                MaxApproxError = CurveApprox.ScreenError / value * HalfSize;
                _clipper.ClipPoly = BuildClipPoly();
            }
        }
    }

    protected ArenaBounds(WPos center, float halfSize)
    {
        Center = center;
        HalfSize = halfSize;
    }

    public abstract IEnumerable<WPos> BuildClipPoly(float offset = 0); // positive offset increases area, negative decreases
    public abstract Pathfinding.Map BuildMap(float resolution = 0.5f);
    public abstract bool Contains(WPos p);
    public abstract float IntersectRay(WPos origin, WDir dir);
    public abstract WDir ClampToBounds(WDir offset, float scale = 1);
    public WPos ClampToBounds(WPos position) => Center + ClampToBounds(position - Center);

    // functions for clipping various shapes to bounds
    public List<(WPos, WPos, WPos)> ClipAndTriangulateCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
    {
        // TODO: think of a better way to do that (analytical clipping?)
        if (innerRadius >= outerRadius || innerRadius < 0 || halfAngle.Rad <= 0)
            return new();

        bool fullCircle = halfAngle.Rad >= MathF.PI;
        bool donut = innerRadius > 0;
        var points = (donut, fullCircle) switch
        {
            (false, false) => CurveApprox.CircleSector(center, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (false, true) => CurveApprox.Circle(center, outerRadius, MaxApproxError),
            (true, false) => CurveApprox.DonutSector(center, innerRadius, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (true, true) => CurveApprox.Donut(center, innerRadius, outerRadius, MaxApproxError),
        };
        return ClipAndTriangulate(points);
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateCircle(WPos center, float radius)
    {
        return ClipAndTriangulate(CurveApprox.Circle(center, radius, MaxApproxError));
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateDonut(WPos center, float innerRadius, float outerRadius)
    {
        if (innerRadius >= outerRadius || innerRadius < 0)
            return new();
        return ClipAndTriangulate(CurveApprox.Donut(center, innerRadius, outerRadius, MaxApproxError));
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateTri(WPos a, WPos b, WPos c)
    {
        return ClipAndTriangulate(new[] { a, b, c });
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateIsoscelesTri(WPos apex, WDir height, WDir halfBase)
    {
        return ClipAndTriangulateTri(apex, apex + height + halfBase, apex + height - halfBase);
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return ClipAndTriangulateIsoscelesTri(apex, height * dir, height * halfAngle.Tan() * normal);
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = origin + lenFront * direction;
        var back = origin - lenBack * direction;
        return ClipAndTriangulate(new[] { front + side, front - side, back - side, back + side });
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
    {
        return ClipAndTriangulateRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateRect(WPos start, WPos end, float halfWidth)
    {
        var dir = (end - start).Normalized();
        var side = halfWidth * dir.OrthoR();
        return ClipAndTriangulate(new[] { start + side, start - side, end - side, end + side });
    }
}

public class ArenaBoundsCircle : ArenaBounds
{
    public ArenaBoundsCircle(WPos center, float radius) : base(center, radius) { }

    public override IEnumerable<WPos> BuildClipPoly(float offset) => CurveApprox.Circle(Center, HalfSize + offset, MaxApproxError);

    public override Pathfinding.Map BuildMap(float resolution)
    {
        var map = new Pathfinding.Map(resolution, Center, HalfSize, HalfSize, new());
        map.BlockPixelsInside(ShapeDistance.InvertedCircle(Center, HalfSize), 0, 0);
        return map;
    }

    public override bool Contains(WPos position) => position.InCircle(Center, HalfSize);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayCircle(origin, dir, Center, HalfSize);

    public override WDir ClampToBounds(WDir offset, float scale)
    {
        var r = HalfSize * scale;
        if (offset.LengthSq() > r * r)
            offset *= r / offset.Length();
        return offset;
    }
}

public class ArenaBoundsSquare : ArenaBounds
{
    public ArenaBoundsSquare(WPos center, float halfWidth) : base(center, halfWidth) { }

    public override IEnumerable<WPos> BuildClipPoly(float offset)
    {
        var s = HalfSize + offset;
        yield return Center + new WDir(s, -s);
        yield return Center + new WDir(s, s);
        yield return Center + new WDir(-s, s);
        yield return Center + new WDir(-s, -s);
    }

    public override Pathfinding.Map BuildMap(float resolution) => new Pathfinding.Map(resolution, Center, HalfSize, HalfSize);
    public override bool Contains(WPos position) => WPos.AlmostEqual(position, Center, HalfSize);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayRect(origin, dir, Center, new(0, 1), HalfSize, HalfSize);

    public override WDir ClampToBounds(WDir offset, float scale)
    {
        var wh = HalfSize * scale;
        if (Math.Abs(offset.X) > wh)
            offset *= wh / Math.Abs(offset.X);
        if (Math.Abs(offset.Z) > wh)
            offset *= wh / Math.Abs(offset.Z);
        return offset;
    }
}

public class ArenaBoundsRect : ArenaBounds
{
    public float HalfWidth { get; init; } // along X if rotation is 0
    public float HalfHeight { get; init; } // along Z if rotation is 0
    public Angle Rotation { get; init; }

    public ArenaBoundsRect(WPos center, float halfWidth, float halfHeight, Angle rotation = new()) : base(center, MathF.Max(halfWidth, halfHeight))
    {
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
        Rotation = rotation;
    }

    public override IEnumerable<WPos> BuildClipPoly(float offset)
    {
        var n = Rotation.ToDirection(); // local 'z' axis
        var dx = n.OrthoL() * (HalfWidth + offset);
        var dz = n * (HalfHeight + offset);
        yield return Center + dx - dz;
        yield return Center + dx + dz;
        yield return Center - dx + dz;
        yield return Center - dx - dz;
    }

    public override Pathfinding.Map BuildMap(float resolution) => new Pathfinding.Map(resolution, Center, HalfWidth, HalfHeight, Rotation);
    public override bool Contains(WPos position) => position.InRect(Center, Rotation, HalfHeight, HalfHeight, HalfWidth);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayRect(origin, dir, Center, Rotation.ToDirection(), HalfWidth, HalfHeight);

    public override WDir ClampToBounds(WDir offset, float scale)
    {
        var n = Rotation.ToDirection();
        var dx = MathF.Abs(offset.Dot(n.OrthoL()));
        if (dx > HalfWidth * scale)
            offset *= HalfWidth * scale / dx;
        var dy = MathF.Abs(offset.Dot(n));
        if (dy > HalfHeight * scale)
            offset *= HalfHeight * scale / dy;
        return offset;
    }
}

// TODO: revise and reconsider, not convinced it needs to be here, and it's not well implemented
public class ArenaBoundsTri : ArenaBounds
{
    private static readonly float sqrt3 = MathF.Sqrt(3);

    public ArenaBoundsTri(WPos center, float sideLength) : base(center, sideLength * sqrt3 / 3) { } // HalfSize is the radius of the circumscribed circle

    public override IEnumerable<WPos> BuildClipPoly(float offset = 0)
    {
        // Calculate the vertices of the equilateral triangle
        var height = HalfSize * sqrt3; // Height of the equilateral triangle
        var halfSide = HalfSize;
        yield return Center + new WDir(-halfSide, height / 3);
        yield return Center + new WDir(halfSide, height / 3);
        yield return Center + new WDir(0, -2 * height / 3);
    }

    public override Pathfinding.Map BuildMap(float resolution = 0.5f)
    {
        // BuildMap implementation for equilateral triangle
        // This is a simplified example and would need to be adapted based on specific pathfinding requirements
        throw new NotImplementedException();
    }

    public override bool Contains(WPos p)
    {
        var a = Center + new WDir(-HalfSize, HalfSize * sqrt3 / 3);
        var b = Center + new WDir(HalfSize, HalfSize * sqrt3 / 3);
        var c = Center + new WDir(0, -2 * HalfSize * sqrt3 / 3);

        bool b1 = Sign(p, a, b) < 0;
        bool b2 = Sign(p, b, c) < 0;
        bool b3 = Sign(p, c, a) < 0;

        return (b1 == b2) && (b2 == b3);
    }

    private float Sign(WPos p1, WPos p2, WPos p3)
    {
        return (p1.X - p3.X) * (p2.Z - p3.Z) - (p2.X - p3.X) * (p1.Z - p3.Z);
    }

    public override float IntersectRay(WPos origin, WDir dir)
    {
        // Define triangle vertices
        var a = Center + new WDir(-HalfSize, HalfSize * sqrt3 / 3);
        var b = Center + new WDir(HalfSize, HalfSize * sqrt3 / 3);
        var c = Center + new WDir(0, -2 * HalfSize * sqrt3 / 3);

        // Ray-triangle intersection algorithm goes here
        // This is a complex topic and requires a bit of math
        // Placeholder for the actual intersection calculation
        return float.NaN; // Return NaN to indicate that this method needs proper implementation
    }

    public override WDir ClampToBounds(WDir offset, float scale = 1)
    {
        // Clamping within a triangle is highly context-dependent
        // This method needs a detailed implementation based on specific requirements
        return new WDir(0, 0); // Placeholder to indicate that clamping logic is needed
    }
}
