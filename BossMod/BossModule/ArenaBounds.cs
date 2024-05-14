namespace BossMod;

// radius is the largest horizontal/vertical dimension: radius for circle, max of width/height for rect
// note: this class to represent *relative* arena bounds (relative to arena center) - the reason being that in some cases effective center moves every frame, and bounds caches a lot (clip poly & base map for pathfinding)
// note: if arena bounds are changed, new instance is recreated; max approx error can change without recreating the instance
public abstract record class ArenaBounds(float Radius, float MapResolution, WPos Center)
{
    // fields below are used for clipping & drawing borders
    public readonly PolygonClipper Clipper = new();
    public float MaxApproxError { get; private set; }
    public RelSimplifiedComplexPolygon ShapeSimplified { get; private set; } = new();
    public List<RelTriangle> ShapeTriangulation { get; private set; } = [];
    private readonly PolygonClipper.Operand _clipOperand = new();
    public static readonly Dictionary<object, object> StaticCache = [];

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
    public abstract Pathfinding.Map PathfindMap(WPos center); // if implementation caches a map, it should return a clone
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

        var fullCircle = halfAngle.Rad >= MathF.PI;
        var donut = innerRadius > 0;
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

public record class ArenaBoundsCircle(float Radius, float MapResolution = 0.5f, WPos Center = default) : ArenaBounds(Radius, MapResolution, Center)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Circle(Radius, MaxApproxError));
    public override Pathfinding.Map PathfindMap(WPos center) => (_cachedMap ??= BuildMap()).Clone(center);
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

// if rotation is 0, half-width is along X and half-height is along Z
public record class ArenaBoundsRect(float HalfWidth, float HalfHeight, Angle Rotation = default, float MapResolution = 0.5f, WPos Center = default) : ArenaBounds(CalculateRadius(HalfHeight, HalfWidth, Rotation), MapResolution, Center)
{
    public readonly WDir Orientation = Rotation.ToDirection();

    private static float CalculateRadius(float HalfWidth, float HalfHeight, Angle Rotation)
    {
        if (StaticCache.TryGetValue((HalfWidth, HalfHeight, Rotation), out var cachedResult))
            return (float)cachedResult;

        var cos = MathF.Abs(MathF.Cos(Rotation.Rad));
        var sin = MathF.Abs(MathF.Sin(Rotation.Rad));
        var corner1 = new WDir(HalfWidth * cos - HalfHeight * sin, HalfWidth * sin + HalfHeight * cos);
        var corner2 = new WDir(HalfWidth * cos + HalfHeight * sin, HalfWidth * sin - HalfHeight * cos);
        var maxDistX = Math.Max(MathF.Abs(corner1.X), MathF.Abs(corner2.X));
        var maxDistZ = Math.Max(MathF.Abs(corner1.Z), MathF.Abs(corner2.Z));
        var radius = Math.Max(maxDistX, maxDistZ);

        StaticCache[(HalfWidth, HalfHeight, Rotation)] = radius;
        return radius;
    }

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Rect(Orientation, HalfWidth, HalfHeight));
    public override Pathfinding.Map PathfindMap(WPos center) => new(MapResolution, center, HalfWidth, HalfHeight, Rotation);
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

public record class ArenaBoundsSquare(float Radius, Angle Rotation = default, float MapResolution = 0.5f, WPos Center = default) : ArenaBoundsRect(Radius, Radius, Rotation, MapResolution, Center) { }

// custom complex polygon bounds
public record class ArenaBoundsCustom(float Radius, RelSimplifiedComplexPolygon Poly, float MapResolution = 0.5f, WPos Center = default) : ArenaBounds(Radius, MapResolution, Center)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(Poly);
    public override Pathfinding.Map PathfindMap(WPos center) => (_cachedMap ??= BuildMap()).Clone(center);
    public override bool Contains(WDir offset) => Poly.Contains(offset);
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayPolygon(originOffset, dir, Poly);

    public override WDir ClampToBounds(WDir offset)
    {
        if (Contains(offset))
            return offset;

        var minDistance = float.MaxValue;
        var nearestPoint = offset;

        foreach (var part in Poly.Parts)
        {
            foreach (var edge in part.ExteriorEdges.Concat(part.Holes.SelectMany(part.InteriorEdges)))
            {
                var edgeStart = edge.Item1;
                var edgeEnd = edge.Item2;
                var nearest = NearestPointOnSegment(offset, edgeStart, edgeEnd);
                var distance = (nearest - offset).LengthSq();

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = nearest;
                }
            }
        }

        return nearestPoint;
    }

    private WDir NearestPointOnSegment(WDir point, WDir segmentStart, WDir segmentEnd)
    {
        var segmentVector = segmentEnd - segmentStart;
        var segmentLengthSq = segmentVector.LengthSq();
        if (segmentLengthSq == 0)
            return segmentStart;

        var t = Math.Max(0, Math.Min(1, (point - segmentStart).Dot(segmentVector) / segmentLengthSq));
        return segmentStart + t * segmentVector;
    }

    private Pathfinding.Map BuildMap()
    {
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minZ = float.MaxValue;
        var maxZ = float.MinValue;

        foreach (var part in Poly.Parts)
        {
            foreach (var vertex in part.Exterior)
            {
                var absVertex = Center + vertex;
                if (absVertex.X < minX)
                    minX = absVertex.X;
                if (absVertex.X > maxX)
                    maxX = absVertex.X;
                if (absVertex.Z < minZ)
                    minZ = absVertex.Z;
                if (absVertex.Z > maxZ)
                    maxZ = absVertex.Z;
            }
        }

        var halfWidth = (maxX - minX) / 2;
        var halfHeight = (maxZ - minZ) / 2;

        var map = new Pathfinding.Map(MapResolution, Center, halfWidth, halfHeight);

        foreach (var (x, y, pos) in map.EnumeratePixels())
        {
            var relativeCenter = new WDir(pos.X - Center.X, pos.Z - Center.Z);
            map.Pixels[y * map.Width + x].MaxG = Poly.Contains(relativeCenter) ? float.MaxValue : 0;
        }
        return map;
    }
}

// for creating complex bounds by using two IEnumerable of shapes
// first IEnumerable contains platforms that will be united, second optional IEnumberale contains shapes that will be subtracted

public record class ArenaBoundsComplex(IEnumerable<Shape> UnionShapes, IEnumerable<Shape>? DifferenceShapes = null, float MapResolution = 0.5f) : ArenaBoundsCustom(BuildBounds(UnionShapes, DifferenceShapes ?? [], MapResolution))
{
    private static ArenaBoundsCustom BuildBounds(IEnumerable<Shape> unionShapes, IEnumerable<Shape> differenceShapes, float mapResolution)
    {
        var (center, radius, poly) = CalculatePolygonProperties(unionShapes, differenceShapes);
        return new ArenaBoundsCustom(radius, poly, mapResolution, center);
    }

    private static (WPos Center, float Radius, RelSimplifiedComplexPolygon Poly) CalculatePolygonProperties(IEnumerable<Shape> unionShapes, IEnumerable<Shape> differenceShapes)
    {
        var unionPolygons = ParseShapes(unionShapes, default);
        var differencePolygons = ParseShapes(differenceShapes, default);

        var combinedPoly = CombinePolygons(unionPolygons, differencePolygons);

        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var part in combinedPoly.Parts)
        {
            foreach (var vertex in part.Exterior)
            {
                if (vertex.X < minX)
                    minX = vertex.X;
                if (vertex.X > maxX)
                    maxX = vertex.X;
                if (vertex.Z < minZ)
                    minZ = vertex.Z;
                if (vertex.Z > maxZ)
                    maxZ = vertex.Z;
            }
        }

        var center = new WPos((minX + maxX) / 2, (minZ + maxZ) / 2);
        var maxDistX = Math.Max(MathF.Abs(maxX - center.X), MathF.Abs(minX - center.X));
        var maxDistZ = Math.Max(MathF.Abs(maxZ - center.Z), MathF.Abs(minZ - center.Z));
        var radius = Math.Max(maxDistX, maxDistZ);

        var unionPolygonsCentered = ParseShapes(unionShapes, center);
        var differencePolygonsCentered = ParseShapes(differenceShapes, center);
        var combinedPolyCentered = CombinePolygons(unionPolygonsCentered, differencePolygonsCentered);

        return (center, radius, combinedPolyCentered);
    }

    private static RelSimplifiedComplexPolygon CombinePolygons(List<RelSimplifiedComplexPolygon> unionPolygons, List<RelSimplifiedComplexPolygon> differencePolygons)
    {
        var clipper = new PolygonClipper();
        var operandUnion = new PolygonClipper.Operand();
        var operandDifference = new PolygonClipper.Operand();

        foreach (var polygon in unionPolygons)
            operandUnion.AddPolygon(polygon);

        foreach (var polygon in differencePolygons)
            operandDifference.AddPolygon(polygon);

        var combinedShape = clipper.Union(operandUnion, new PolygonClipper.Operand(), Clipper2Lib.FillRule.NonZero);
        if (differencePolygons.Count != 0)
            combinedShape = clipper.Difference(new PolygonClipper.Operand(combinedShape), operandDifference, Clipper2Lib.FillRule.NonZero);

        return combinedShape;
    }

    private static List<RelSimplifiedComplexPolygon> ParseShapes(IEnumerable<Shape> shapes, WPos center) => shapes.Select(shape => shape.ToPolygon(center)).ToList();
}
