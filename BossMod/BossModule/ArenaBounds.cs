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
    public static readonly Dictionary<object, object> StaticCache = [];
    public readonly Dictionary<object, object> Cache = [];

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

    public void AddToInstanceCache(object key, object value)
    {
        if (Cache.Count > 2500)
            Cache.Clear();
        Cache[key] = value;
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
        map.BlockPixelsInsideAccuracy(ShapeDistance.InvertedCircle(default, Radius), 0, 0);
        return map;
    }
}

// if rotation is 0, half-width is along X and half-height is along Z
public record class ArenaBoundsRect(float HalfWidth, float HalfHeight, Angle Rotation = default, float MapResolution = 0.5f) : ArenaBounds(CalculateRadius(HalfHeight, HalfWidth, Rotation), MapResolution)
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

public record class ArenaBoundsSquare(float Radius, Angle Rotation = default, float MapResolution = 0.5f) : ArenaBoundsRect(Radius, Radius, Rotation, MapResolution) { }

// custom complex polygon bounds
public record class ArenaBoundsCustom(float Radius, RelSimplifiedComplexPolygon Poly, float MapResolution = 0.5f, float Offset = 0) : ArenaBounds(Radius, MapResolution)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(Poly);
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);

    public override bool Contains(WDir offset)
    {
        var cacheKey = (Poly, offset, Radius);
        if (Cache.TryGetValue(cacheKey, out var cachedResult)) // caching contains seems to lower drawtime by ~33%
            return (bool)cachedResult;
        var result = Poly.Contains(offset);
        AddToInstanceCache(cacheKey, result);
        return result;
    }

    public override float IntersectRay(WDir originOffset, WDir dir)
    {
        var cacheKey = (Poly, originOffset, dir);
        if (Cache.TryGetValue(cacheKey, out var cachedResult)) // caching intersections seems to lower drawtime by ~12.5% while in use
            return (float)cachedResult;
        var result = Intersect.RayPolygon(originOffset, dir, Poly);
        AddToInstanceCache(cacheKey, result);
        return result;
    }

    public override WDir ClampToBounds(WDir offset)
    {
        var cacheKey = (Poly, offset);
        if (Cache.TryGetValue(cacheKey, out var cachedResult)) // caching ClampToBounds seems to lower drawtime by about 50%
            return (WDir)cachedResult;
        if (Contains(offset) || offset.AlmostEqual(default, 0.1f)) // if actor is almost in the center of the arena, do nothing (eg donut arena)
        {
            Cache[(Poly, offset)] = offset;
            return offset;
        }
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
        AddToInstanceCache(cacheKey, nearestPoint);
        return nearestPoint;
    }

    private static WDir NearestPointOnSegment(WDir point, WDir segmentStart, WDir segmentEnd)
    {
        var segmentVector = segmentEnd - segmentStart;
        var segmentLengthSq = segmentVector.LengthSq();
        if (segmentLengthSq == 0)
            return segmentStart;

        var t = Math.Max(0, Math.Min(1, (point - segmentStart).Dot(segmentVector) / segmentLengthSq));
        return segmentStart + t * segmentVector;
    }

    private static (float halfWidth, float halfHeight) CalculatePolygonProperties(RelSimplifiedComplexPolygon Poly)
    {
        if (StaticCache.TryGetValue(Poly, out var cachedResult))
            return ((float, float))cachedResult;

        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var part in Poly.Parts)
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

        var result = ((maxX - minX) / 2, (maxZ - minZ) / 2);
        StaticCache[Poly] = result;
        return result;
    }

    private Pathfinding.Map BuildMap()
    {
        // faster than using a shapedistance method, high accuracy due to 9 sample points per pixel
        var polygon = Offset != 0 ? Poly.Offset(Offset) : Poly;

        var (halfWidth, halfHeight) = CalculatePolygonProperties(polygon); // calculate bounding box to ensure the smallest amount of pixels need to be checked
        var map = new Pathfinding.Map(MapResolution, default, halfWidth, halfHeight);
        var halfSample = MapResolution / 2 - 1e-5f; // tiny offset to account for floating point inaccuracies
        WDir[] sampleOffsets =
        [
        new(-halfSample, -halfSample),
        new(-halfSample,  0),
        new(-halfSample,  halfSample),
        new(0, -halfSample),
        new(0, 0),
        new(0, halfSample),
        new(halfSample, -halfSample),
        new(halfSample, 0),
        new(halfSample, halfSample)
        ];

        Parallel.ForEach(map.EnumeratePixels(), pixel =>
        {
            var (x, y, pos) = pixel;
            var relativeCenter = new WDir(pos.X, pos.Z);
            var allInside = true;
            for (var i = 0; i < 9; i++)
            {
                var samplePoint = relativeCenter + sampleOffsets[i];
                if (!Poly.Contains(samplePoint))
                {
                    allInside = false;
                    break;
                }
            }
            map.PixelMaxG[map.GridToIndex(x, y)] = allInside ? float.MaxValue : 0;
        });

        return map;
    }
}

// for creating complex bounds by using two IEnumerable of shapes
// first IEnumerable contains platforms that will be united, second optional IEnumberale contains shapes that will be subtracted
// for convenience third list will optionally perform additional unions at the end
public record class ArenaBoundsComplex : ArenaBoundsCustom
{
    public WPos Center;

    public ArenaBoundsComplex(IEnumerable<Shape> UnionShapes, IEnumerable<Shape>? DifferenceShapes = null, IEnumerable<Shape>? AdditionalShapes = null, float MapResolution = 0.5f, float Offset = 0)
        : base(BuildBounds(UnionShapes, DifferenceShapes, AdditionalShapes, MapResolution, Offset, out var center))
    {
        Center = center;
    }

    private static ArenaBoundsCustom BuildBounds(IEnumerable<Shape> unionShapes, IEnumerable<Shape>? differenceShapes, IEnumerable<Shape>? additionalShapes, float mapResolution, float offset, out WPos center)
    {
        var cacheKey = CreateCacheKey(unionShapes, differenceShapes ?? [], additionalShapes ?? []);
        var properties = CalculatePolygonProperties(cacheKey, unionShapes, differenceShapes ?? [], additionalShapes ?? []);
        center = properties.Center;
        return new ArenaBoundsCustom(properties.Radius, properties.Poly, mapResolution, offset);
    }

    private static (WPos Center, float Radius, RelSimplifiedComplexPolygon Poly) CalculatePolygonProperties(int cacheKey, IEnumerable<Shape> unionShapes, IEnumerable<Shape> differenceShapes, IEnumerable<Shape> additionalShapes)
    {
        if (StaticCache.TryGetValue(cacheKey, out var cachedResult))
            return ((WPos, float, RelSimplifiedComplexPolygon))cachedResult;

        var unionPolygons = ParseShapes(unionShapes);
        var differencePolygons = ParseShapes(differenceShapes);
        var additionalPolygons = ParseShapes(additionalShapes);

        var combinedPoly = CombinePolygons(unionPolygons, differencePolygons, additionalPolygons);

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

        var combinedPolyCentered = CombinePolygons(ParseShapes(unionShapes, center), ParseShapes(differenceShapes, center), ParseShapes(additionalShapes, center));
        var result = (center, radius, combinedPolyCentered);
        StaticCache[cacheKey] = result;
        return result;
    }

    private static int CreateCacheKey(IEnumerable<Shape> unionShapes, IEnumerable<Shape> differenceShapes, IEnumerable<Shape> additionalShapes)
    {
        var hashCode = new HashCode();
        foreach (var shape in unionShapes.Concat(differenceShapes).Concat(additionalShapes))
            hashCode.Add(shape.GetHashCode());
        return hashCode.ToHashCode();
    }

    private static RelSimplifiedComplexPolygon CombinePolygons(List<RelSimplifiedComplexPolygon> unionPolygons, List<RelSimplifiedComplexPolygon> differencePolygons, List<RelSimplifiedComplexPolygon> secondUnionPolygons)
    {
        var clipper = new PolygonClipper();
        var operandUnion = new PolygonClipper.Operand();
        var operandDifference = new PolygonClipper.Operand();
        var operandSecondUnion = new PolygonClipper.Operand();

        foreach (var polygon in unionPolygons)
            operandUnion.AddPolygon(polygon);
        foreach (var polygon in differencePolygons)
            operandDifference.AddPolygon(polygon);
        foreach (var polygon in secondUnionPolygons)
            operandSecondUnion.AddPolygon(polygon);

        var combinedShape = clipper.Union(operandUnion, new PolygonClipper.Operand());
        if (differencePolygons.Count != 0)
            combinedShape = clipper.Difference(new PolygonClipper.Operand(combinedShape), operandDifference);
        if (secondUnionPolygons.Count != 0)
            combinedShape = clipper.Union(new PolygonClipper.Operand(combinedShape), operandSecondUnion);

        return combinedShape;
    }

    private static List<RelSimplifiedComplexPolygon> ParseShapes(IEnumerable<Shape> shapes, WPos center = default) => shapes.Select(shape => shape.ToPolygon(center)).ToList();
}
