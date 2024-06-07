namespace BossMod;

// radius is the largest horizontal/vertical dimension: radius for circle, max of width/height for rect
// note: this class to represent *relative* arena bounds (relative to arena center) - the reason being that in some cases effective center moves every frame, and bounds caches a lot (clip poly & base map for pathfinding)
// note: if arena bounds are changed, new instance is recreated; max approx error can change without recreating the instance
public abstract record class ArenaBounds(float Radius, float MapResolution)
{
    // fields below are used for clipping & drawing borders
    public readonly PolygonClipper Clipper = new();
    public float MaxApproxError { get; private set; }
    public RelSimplifiedComplexPolygon ShapeSimplified { get; private set; } = new();
    public List<RelTriangle> ShapeTriangulation { get; private set; } = [];
    private readonly PolygonClipper.Operand _clipOperand = new();

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
    public abstract void PathfindMap(Pathfinding.Map map, WPos center);
    public abstract bool Contains(WDir offset);
    public abstract float IntersectRay(WDir originOffset, WDir dir);
    public abstract WDir ClampToBounds(WDir offset);

    // functions for clipping various shapes to bounds; all shapes are expected to be defined relative to bounds center
    public List<RelTriangle> ClipAndTriangulate(IEnumerable<WDir> poly) => Clipper.Intersect(new(poly), _clipOperand).Triangulate();
    public List<RelTriangle> ClipAndTriangulate(RelSimplifiedComplexPolygon poly) => Clipper.Intersect(new(poly), _clipOperand).Triangulate();

    public List<RelTriangle> ClipAndTriangulateCone(WDir centerOffset, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
    {
        // TODO: think of a better way to do that (analytical clipping?)
        if (innerRadius >= outerRadius || innerRadius < 0 || halfAngle.Rad <= 0)
            return [];

        bool fullCircle = halfAngle.Rad >= MathF.PI;
        bool donut = innerRadius > 0;
        var points = (donut, fullCircle) switch
        {
            (false, false) => CurveApprox.CircleSector(outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (false, true) => CurveApprox.Circle(outerRadius, MaxApproxError),
            (true, false) => CurveApprox.DonutSector(innerRadius, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (true, true) => CurveApprox.Donut(innerRadius, outerRadius, MaxApproxError),
        };
        return ClipAndTriangulate(points.Select(p => p + centerOffset));
    }

    public List<RelTriangle> ClipAndTriangulateCircle(WDir centerOffset, float radius)
        => ClipAndTriangulate(CurveApprox.Circle(radius, MaxApproxError).Select(p => p + centerOffset));

    public List<RelTriangle> ClipAndTriangulateDonut(WDir centerOffset, float innerRadius, float outerRadius)
        => innerRadius < outerRadius && innerRadius >= 0
            ? ClipAndTriangulate(CurveApprox.Donut(innerRadius, outerRadius, MaxApproxError).Select(p => p + centerOffset))
            : [];

    public List<RelTriangle> ClipAndTriangulateTri(WDir oa, WDir ob, WDir oc)
        => ClipAndTriangulate([oa, ob, oc]);

    public List<RelTriangle> ClipAndTriangulateIsoscelesTri(WDir apexOffset, WDir height, WDir halfBase)
        => ClipAndTriangulateTri(apexOffset, apexOffset + height + halfBase, apexOffset + height - halfBase);

    public List<RelTriangle> ClipAndTriangulateIsoscelesTri(WDir apexOffset, Angle direction, Angle halfAngle, float height)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return ClipAndTriangulateIsoscelesTri(apexOffset, height * dir, height * halfAngle.Tan() * normal);
    }

    public List<RelTriangle> ClipAndTriangulateRect(WDir originOffset, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = originOffset + lenFront * direction;
        var back = originOffset - lenBack * direction;
        return ClipAndTriangulate([front + side, front - side, back - side, back + side]);
    }

    public List<RelTriangle> ClipAndTriangulateRect(WDir originOffset, Angle direction, float lenFront, float lenBack, float halfWidth)
        => ClipAndTriangulateRect(originOffset, direction.ToDirection(), lenFront, lenBack, halfWidth);

    public List<RelTriangle> ClipAndTriangulateRect(WDir startOffset, WDir endOffset, float halfWidth)
    {
        var dir = (endOffset - startOffset).Normalized();
        var side = halfWidth * dir.OrthoR();
        return ClipAndTriangulate([startOffset + side, startOffset - side, endOffset - side, endOffset + side]);
    }
}

public record class ArenaBoundsCircle(float Radius, float MapResolution = 0.5f) : ArenaBounds(Radius, MapResolution)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Circle(Radius, MaxApproxError));
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);
    public override bool Contains(WDir offset) => offset.LengthSq() <= Radius * Radius;
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayCircle(originOffset, dir, Radius);

    public override WDir ClampToBounds(WDir offset)
    {
        if (offset.LengthSq() > Radius * Radius)
            offset *= Radius / offset.Length();
        return offset;
    }

    private Pathfinding.Map BuildMap()
    {
        var map = new Pathfinding.Map(MapResolution, default, Radius, Radius);
        map.BlockPixelsInside(ShapeDistance.InvertedCircle(default, Radius), 0, 0);
        return map;
    }
}

public record class ArenaBoundsSquare(float Radius, float MapResolution = 0.5f) : ArenaBounds(Radius, MapResolution)
{
    public float HalfWidth => Radius;

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Rect(new(Radius, 0), new(0, Radius)));
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(MapResolution, center, Radius, Radius);
    public override bool Contains(WDir offset) => offset.AlmostZero(Radius);
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayAABB(originOffset, dir, Radius, Radius);

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
public record class ArenaBoundsRect(float HalfWidth, float HalfHeight, Angle Rotation = default, float MapResolution = 0.5f) : ArenaBounds(MathF.Max(HalfWidth, HalfHeight), MapResolution)
{
    public readonly WDir Orientation = Rotation.ToDirection();

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Rect(Orientation, HalfWidth, HalfHeight));
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(MapResolution, center, HalfWidth, HalfHeight, Rotation);
    public override bool Contains(WDir offset) => offset.InRect(Orientation, HalfHeight, HalfHeight, HalfWidth);
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayRect(originOffset, dir, Orientation, HalfWidth, HalfHeight);

    public override WDir ClampToBounds(WDir offset)
    {
        var dx = MathF.Abs(offset.Dot(Orientation.OrthoL()));
        if (dx > HalfWidth)
            offset *= HalfWidth / dx;
        var dy = MathF.Abs(offset.Dot(Orientation));
        if (dy > HalfHeight)
            offset *= HalfHeight / dy;
        return offset;
    }
}

// custom complex polygon bounds
public record class ArenaBoundsCustom(float Radius, RelSimplifiedComplexPolygon Poly, float MapResolution = 0.5f) : ArenaBounds(Radius, MapResolution)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(Poly);
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);
    public override bool Contains(WDir offset) => Poly.Contains(offset);
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayPolygon(originOffset, dir, Poly);
    public override WDir ClampToBounds(WDir offset)
    {
        var l = offset.Length();
        var dir = offset / l;
        var t = Intersect.RayPolygon(default, offset, Poly);
        return dir * Math.Min(t, l);
    }

    private Pathfinding.Map BuildMap()
    {
        var map = new Pathfinding.Map(MapResolution, default, Radius, Radius);
        var tri = ShapeDistance.TriList(default, Poly.Triangulate());
        map.BlockPixelsInside(p => -tri(p), 0, 0);
        return map;
    }
}
