namespace BossMod;

// radius is the largest horizontal/vertical dimension: radius for circle, max of width/height for rect
// note: if arena bounds are changed, new instance is recreated
// max approx error can change without recreating the instance
public abstract record class ArenaBounds(WPos Center, float Radius)
{
    // fields below are used for clipping
    public readonly PolygonClipper Clipper = new();
    public float MaxApproxError { get; private set; }
    public SimplifiedComplexPolygon ShapeSimplified { get; private set; } = new();
    public List<Triangle> ShapeTriangulation { get; private set; } = [];
    private readonly PolygonClipper.Operand _clipOperand = new();

    public List<Triangle> ClipAndTriangulate(PolygonClipper.Operand poly) => Clipper.Intersect(poly, _clipOperand).Triangulate();
    public List<Triangle> ClipAndTriangulate(IEnumerable<WPos> poly) => Clipper.Intersect(new(poly), _clipOperand).Triangulate();
    public List<Triangle> ClipAndTriangulate(SimplifiedComplexPolygon poly) => Clipper.Intersect(new(poly), _clipOperand).Triangulate();

    private float _screenHalfSize;
    public float ScreenHalfSize
    {
        get => _screenHalfSize;
        set
        {
            if (_screenHalfSize != value)
            {
                _screenHalfSize = value;
                MaxApproxError = CurveApprox.ScreenError / value * Radius;
                ShapeSimplified = Clipper.Simplify(BuildClipPoly());
                ShapeTriangulation = ShapeSimplified.Triangulate();
                _clipOperand.Clear();
                _clipOperand.AddPolygon(ShapeSimplified); // note: I assume using simplified shape as an operand is better than raw one
            }
        }
    }

    public abstract PolygonClipper.Operand BuildClipPoly();
    public abstract Pathfinding.Map BuildMap(float resolution = 0.5f);
    public abstract bool Contains(WPos p);
    public abstract float IntersectRay(WPos origin, WDir dir);
    public abstract WDir ClampToBounds(WDir offset, float scale = 1);
    public WPos ClampToBounds(WPos position) => Center + ClampToBounds(position - Center);

    // functions for clipping various shapes to bounds
    public List<Triangle> ClipAndTriangulateCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
    {
        // TODO: think of a better way to do that (analytical clipping?)
        if (innerRadius >= outerRadius || innerRadius < 0 || halfAngle.Rad <= 0)
            return [];

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

    public List<Triangle> ClipAndTriangulateCircle(WPos center, float radius)
        => ClipAndTriangulate(CurveApprox.Circle(center, radius, MaxApproxError));

    public List<Triangle> ClipAndTriangulateDonut(WPos center, float innerRadius, float outerRadius)
        => innerRadius < outerRadius && innerRadius >= 0
            ? ClipAndTriangulate(CurveApprox.Donut(center, innerRadius, outerRadius, MaxApproxError))
            : [];

    public List<Triangle> ClipAndTriangulateTri(WPos a, WPos b, WPos c)
        => ClipAndTriangulate([a, b, c]);

    public List<Triangle> ClipAndTriangulateIsoscelesTri(WPos apex, WDir height, WDir halfBase)
        => ClipAndTriangulateTri(apex, apex + height + halfBase, apex + height - halfBase);

    public List<Triangle> ClipAndTriangulateIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return ClipAndTriangulateIsoscelesTri(apex, height * dir, height * halfAngle.Tan() * normal);
    }

    public List<Triangle> ClipAndTriangulateRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = origin + lenFront * direction;
        var back = origin - lenBack * direction;
        return ClipAndTriangulate([front + side, front - side, back - side, back + side]);
    }

    public List<Triangle> ClipAndTriangulateRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
        => ClipAndTriangulateRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);

    public List<Triangle> ClipAndTriangulateRect(WPos start, WPos end, float halfWidth)
    {
        var dir = (end - start).Normalized();
        var side = halfWidth * dir.OrthoR();
        return ClipAndTriangulate([start + side, start - side, end - side, end + side]);
    }
}

public record class ArenaBoundsCircle(WPos Center, float Radius) : ArenaBounds(Center, Radius)
{
    public override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Circle(Center, Radius, MaxApproxError));

    public override Pathfinding.Map BuildMap(float resolution)
    {
        var map = new Pathfinding.Map(resolution, Center, Radius, Radius, new());
        map.BlockPixelsInside(ShapeDistance.InvertedCircle(Center, Radius), 0, 0);
        return map;
    }

    public override bool Contains(WPos p) => p.InCircle(Center, Radius);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayCircle(origin, dir, Center, Radius);

    public override WDir ClampToBounds(WDir offset, float scale)
    {
        var r = Radius * scale;
        if (offset.LengthSq() > r * r)
            offset *= r / offset.Length();
        return offset;
    }
}

public record class ArenaBoundsSquare(WPos Center, float Radius) : ArenaBounds(Center, Radius)
{
    public float HalfWidth => Radius;

    public override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Rect(Center, new(Radius, 0), new(0, Radius)));
    public override Pathfinding.Map BuildMap(float resolution) => new(resolution, Center, Radius, Radius);
    public override bool Contains(WPos p) => WPos.AlmostEqual(p, Center, Radius);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayRect(origin, dir, Center, new(0, 1), Radius, Radius);

    public override WDir ClampToBounds(WDir offset, float scale)
    {
        var wh = Radius * scale;
        if (Math.Abs(offset.X) > wh)
            offset *= wh / Math.Abs(offset.X);
        if (Math.Abs(offset.Z) > wh)
            offset *= wh / Math.Abs(offset.Z);
        return offset;
    }
}

// if rotation is 0, half-width is along X and half-height is along Z
public record class ArenaBoundsRect(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : ArenaBounds(Center, MathF.Max(HalfWidth, HalfHeight))
{
    public override PolygonClipper.Operand BuildClipPoly()
    {
        var n = Rotation.ToDirection(); // local 'z' axis
        var dx = n.OrthoL() * HalfWidth;
        var dz = n * HalfHeight;
        return new(CurveApprox.Rect(Center, dx, dz));
    }

    public override Pathfinding.Map BuildMap(float resolution) => new(resolution, Center, HalfWidth, HalfHeight, Rotation);
    public override bool Contains(WPos p) => p.InRect(Center, Rotation, HalfHeight, HalfHeight, HalfWidth);
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
// HalfSize is the radius of the circumscribed circle
public record class ArenaBoundsTri(WPos Center, float SideLength) : ArenaBounds(Center, SideLength * Sqrt3 / 3)
{
    private const float Sqrt3 = 1.7320508075688772935274463415059f;

    public override PolygonClipper.Operand BuildClipPoly()
    {
        // Calculate the vertices of the equilateral triangle
        var height = Radius * Sqrt3; // Height of the equilateral triangle
        var halfSide = Radius;
        return new([Center + new WDir(-halfSide, height / 3), Center + new WDir(halfSide, height / 3), Center + new WDir(0, -2 * height / 3)]);
    }

    public override Pathfinding.Map BuildMap(float resolution = 0.5f)
    {
        // BuildMap implementation for equilateral triangle
        // This is a simplified example and would need to be adapted based on specific pathfinding requirements
        throw new NotImplementedException();
    }

    public override bool Contains(WPos p)
    {
        var a = Center + new WDir(-Radius, Radius * Sqrt3 / 3);
        var b = Center + new WDir(Radius, Radius * Sqrt3 / 3);
        var c = Center + new WDir(0, -2 * Radius * Sqrt3 / 3);

        bool b1 = Sign(p, a, b) < 0;
        bool b2 = Sign(p, b, c) < 0;
        bool b3 = Sign(p, c, a) < 0;

        return b1 == b2 && b2 == b3;
    }

    private float Sign(WPos p1, WPos p2, WPos p3)
    {
        return (p1.X - p3.X) * (p2.Z - p3.Z) - (p2.X - p3.X) * (p1.Z - p3.Z);
    }

    public override float IntersectRay(WPos origin, WDir dir)
    {
        // Define triangle vertices
        //var a = Center + new WDir(-HalfSize, HalfSize * sqrt3 / 3);
        //var b = Center + new WDir(HalfSize, HalfSize * sqrt3 / 3);
        //var c = Center + new WDir(0, -2 * HalfSize * sqrt3 / 3);

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
