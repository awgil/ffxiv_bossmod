namespace BossMod;

// radius is the largest horizontal/vertical dimension: radius for circle, max of width/height for rect
// note: if arena bounds are changed, new instance is recreated
// max approx error can change without recreating the instance
// TODO: consider changing this class to represent *relative* arena bounds (relative to arena center) - the reason being that in some cases effective center moves every frame, and bounds caches a lot (clip poly & base map for pathfinding)
public abstract record class ArenaBounds(WPos Center, float Radius, float MapResolution)
{
    // fields below are used for clipping & drawing borders
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

    protected abstract PolygonClipper.Operand BuildClipPoly();
    public abstract Pathfinding.Map PathfindMap(); // if implementation caches a map, it should return a clone
    public abstract bool Contains(WPos p);
    public abstract float IntersectRay(WPos origin, WDir dir);
    public abstract WDir ClampToBounds(WDir offset);

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

public record class ArenaBoundsCircle(WPos Center, float Radius, float MapResolution = 0.5f) : ArenaBounds(Center, Radius, MapResolution)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Circle(Center, Radius, MaxApproxError));
    public override Pathfinding.Map PathfindMap() => (_cachedMap ??= BuildMap()).Clone();
    public override bool Contains(WPos p) => p.InCircle(Center, Radius);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayCircle(origin, dir, Center, Radius);

    public override WDir ClampToBounds(WDir offset)
    {
        if (offset.LengthSq() > Radius * Radius)
            offset *= Radius / offset.Length();
        return offset;
    }

    private Pathfinding.Map BuildMap()
    {
        var map = new Pathfinding.Map(MapResolution, Center, Radius, Radius);
        map.BlockPixelsInside(ShapeDistance.InvertedCircle(Center, Radius), 0, 0);
        return map;
    }
}

public record class ArenaBoundsSquare(WPos Center, float Radius, float MapResolution = 0.5f) : ArenaBounds(Center, Radius, MapResolution)
{
    public float HalfWidth => Radius;

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Rect(Center, new(Radius, 0), new(0, Radius)));
    public override Pathfinding.Map PathfindMap() => new(MapResolution, Center, Radius, Radius);
    public override bool Contains(WPos p) => WPos.AlmostEqual(p, Center, Radius);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayRect(origin, dir, Center, new(0, 1), Radius, Radius);

    public override WDir ClampToBounds(WDir offset)
    {
        if (Math.Abs(offset.X) > Radius)
            offset *= Radius / Math.Abs(offset.X);
        if (Math.Abs(offset.Z) > Radius)
            offset *= Radius / Math.Abs(offset.Z);
        return offset;
    }
}

// if rotation is 0, half-width is along X and half-height is along Z
public record class ArenaBoundsRect(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default, float MapResolution = 0.5f) : ArenaBounds(Center, MathF.Max(HalfWidth, HalfHeight), MapResolution)
{
    private WDir _orientation = Rotation.ToDirection();

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Rect(Center, _orientation, HalfWidth, HalfHeight));
    public override Pathfinding.Map PathfindMap() => new(MapResolution, Center, HalfWidth, HalfHeight, Rotation);
    public override bool Contains(WPos p) => p.InRect(Center, Rotation, HalfHeight, HalfHeight, HalfWidth);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayRect(origin, dir, Center, _orientation, HalfWidth, HalfHeight);

    public override WDir ClampToBounds(WDir offset)
    {
        var dx = MathF.Abs(offset.Dot(_orientation.OrthoL()));
        if (dx > HalfWidth)
            offset *= HalfWidth / dx;
        var dy = MathF.Abs(offset.Dot(_orientation));
        if (dy > HalfHeight)
            offset *= HalfHeight / dy;
        return offset;
    }
}

// custom complex polygon bounds
public record class ArenaBoundsCustom(WPos Center, float Radius, SimplifiedComplexPolygon Poly, float MapResolution = 0.5f) : ArenaBounds(Center, Radius, MapResolution)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(Poly);
    public override Pathfinding.Map PathfindMap() => (_cachedMap ??= BuildMap()).Clone();
    public override bool Contains(WPos p) => Poly.Contains(p);
    public override float IntersectRay(WPos origin, WDir dir) => Intersect.RayPolygon(origin, dir, Poly);
    public override WDir ClampToBounds(WDir offset)
    {
        var l = offset.Length();
        var dir = offset / l;
        var t = Intersect.RayPolygon(Center, offset, Poly);
        return dir * Math.Min(t, l);
    }

    private Pathfinding.Map BuildMap()
    {
        var map = new Pathfinding.Map(MapResolution, Center, Radius, Radius);
        var tri = ShapeDistance.TriList(Poly.Triangulate());
        map.BlockPixelsInside(p => -tri(p), 0, 0);
        return map;
    }
}
