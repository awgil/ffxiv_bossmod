// using System.Diagnostics;

namespace BossMod;

// radius is the largest horizontal/vertical dimension: radius for circle, max of width/height for rect
// note: this class to represent *relative* arena bounds (relative to arena center) - the reason being that in some cases effective center moves every frame, and bounds caches a lot (clip poly & base map for pathfinding)
// note: if arena bounds are changed, new instance is recreated; max approx error can change without recreating the instance

[SkipLocalsInit]
public abstract class ArenaBounds(float radius, float mapResolution, float scaleFactor = 1f, bool allowObstacleMap = false)
{
    public readonly float Radius = radius;
    public readonly float InvRadius = 1f / radius;
    public readonly float MapResolution = mapResolution;
    public readonly float ScaleFactor = scaleFactor;
    public readonly bool AllowObstacleMap = allowObstacleMap;

    // fields below are used for clipping & drawing borders
    public readonly PolygonClipper Clipper = new();
    public float MaxApproxError;
    public RelSimplifiedComplexPolygon Shape = new();
    public RelTriangle[] ShapeTriangulation = [];
    private readonly PolygonClipper.Operand _clipOperand = new();

    public float ScreenHalfSize
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                MaxApproxError = CurveApprox.ScreenError / value * Radius;
                if (ShapeTriangulation.Length == 0)
                {
                    Shape = BuildClipPoly();
                    ShapeTriangulation = Shape.Triangulate();
                    _clipOperand.Clear();
                    _clipOperand.AddPolygon(Shape); // note: shape gets simplified in ArenaBoundsCustom, other shapes don't need simplifying
                }
            }
        }
    }

    protected abstract RelSimplifiedComplexPolygon BuildClipPoly();
    public abstract void PathfindMap(Pathfinding.Map map, WPos center);
    public abstract bool Contains(in WDir offset);
    public abstract float IntersectRay(in WDir originOffset, in WDir dir);
    public abstract WDir ClampToBounds(in WDir offset);

    // functions for clipping various shapes to bounds; all shapes are expected to be defined relative to bounds center
    public RelTriangle[] ClipAndTriangulate(ReadOnlySpan<WDir> poly) => Clipper.Intersect(new PolygonClipper.Operand(poly), _clipOperand).Triangulate();
    public RelTriangle[] ClipAndTriangulate(RelSimplifiedComplexPolygon poly) => Clipper.Intersect(new(poly), _clipOperand).Triangulate();
    public RelTriangle[] Triangulate(RelSimplifiedComplexPolygon poly) => poly.Triangulate();
    public RelSimplifiedComplexPolygon Clip(ReadOnlySpan<WDir> poly) => Clipper.Intersect(new PolygonClipper.Operand(poly), _clipOperand);
    public RelSimplifiedComplexPolygon Clip(RelSimplifiedComplexPolygon poly) => Clipper.Intersect(new(poly), _clipOperand);

    public WDir[] ConeVertices(WDir centerOffset, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
    {
        // TODO: think of a better way to do that (analytical clipping?)
        if (innerRadius >= outerRadius || innerRadius < 0f || halfAngle.Rad <= 0f)
        {
            return [];
        }

        var fullCircle = halfAngle.Rad >= MathF.PI;
        var donut = innerRadius != 0;
        var points = (donut, fullCircle) switch
        {
            (false, false) => CurveApprox.CircleSector(centerOffset, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (false, true) => CurveApprox.Circle(centerOffset, outerRadius, MaxApproxError),
            (true, false) => CurveApprox.DonutSector(centerOffset, innerRadius, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (true, true) => CurveApprox.Donut(centerOffset, innerRadius, outerRadius, MaxApproxError),
        };
        return points;
    }

    public RelTriangle[] ClipAndTriangulateCone(WDir centerOffset, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
    {
        return ClipAndTriangulate(ConeVertices(centerOffset, innerRadius, outerRadius, centerDirection, halfAngle));
    }

    public RelSimplifiedComplexPolygon ClipCone(WDir centerOffset, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
    {
        return Clip(ConeVertices(centerOffset, innerRadius, outerRadius, centerDirection, halfAngle));
    }

    public RelTriangle[] ClipAndTriangulateCircle(WDir centerOffset, float radius)
    {

        return ClipAndTriangulate(CurveApprox.Circle(centerOffset, radius, MaxApproxError).AsSpan());
    }

    public RelTriangle[] TriangulateCircle(WDir centerOffset, float radius)
    {

        return Triangulate(new(CurveApprox.CircleL(centerOffset, radius, MaxApproxError)));
    }

    public RelSimplifiedComplexPolygon ClipCircle(WDir centerOffset, float radius)
    {

        return Clip(CurveApprox.Circle(centerOffset, radius, MaxApproxError).AsSpan());
    }

    public RelSimplifiedComplexPolygon CirclePolygon(WDir centerOffset, float radius)
    {

        return new(CurveApprox.CircleL(centerOffset, radius, MaxApproxError));
    }

    public WDir[] CapsuleVertices(WDir centerOffset, WDir direction, float radius, float length)
    {
        return CurveApprox.Capsule(centerOffset, direction, length, radius, MaxApproxError);
    }

    public RelSimplifiedComplexPolygon CapsulePolygon(WDir centerOffset, WDir direction, float radius, float length)
    {
        return new(CurveApprox.CapsuleL(centerOffset, direction, length, radius, MaxApproxError));
    }

    public WDir[] ArcCapsuleVertices(WDir startOffset, WDir toOrbitCenter, Angle angularLength, float radius)
    {
        var points = CurveApprox.ArcCapsule(toOrbitCenter, angularLength, radius, MaxApproxError);
        var len = points.Length;
        for (var i = 0; i < len; ++i)
        {
            points[i] += startOffset;
        }

        return points;
    }

    public RelTriangle[] ClipAndTriangulateCapsule(WDir centerOffset, WDir direction, float radius, float length)
    {
        return ClipAndTriangulate(CurveApprox.Capsule(centerOffset, direction, length, radius, MaxApproxError).AsSpan());
    }

    public RelTriangle[] TriangulateCapsule(WDir centerOffset, WDir direction, float radius, float length)
    {
        return Triangulate(CapsulePolygon(centerOffset, direction, radius, length));
    }

    public RelTriangle[] ClipAndTriangulateArcCapsule(WDir startOffset, WDir toOrbitCenter, Angle angularLength, float radius)
    {
        return ClipAndTriangulate(ArcCapsuleVertices(startOffset, toOrbitCenter, angularLength, radius));
    }

    public RelSimplifiedComplexPolygon ClipCapsule(WDir centerOffset, WDir direction, float radius, float length)
    {
        return Clip(CurveApprox.Capsule(centerOffset, direction, length, radius, MaxApproxError).AsSpan());
    }

    public RelSimplifiedComplexPolygon ClipArcCapsule(WDir startOffset, WDir toOrbitCenter, Angle angularLength, float radius)
    {
        return Clip(ArcCapsuleVertices(startOffset, toOrbitCenter, angularLength, radius));
    }

    public RelTriangle[] ClipAndTriangulateDonut(WDir centerOffset, float innerRadius, float outerRadius)
    {
        return ClipAndTriangulate(CurveApprox.DonutL(centerOffset, innerRadius, outerRadius, MaxApproxError).AsSpan());
    }

    public RelSimplifiedComplexPolygon ClipDonut(WDir centerOffset, float innerRadius, float outerRadius)
    {
        return Clip(CurveApprox.DonutL(centerOffset, innerRadius, outerRadius, MaxApproxError).AsSpan());
    }

    public RelTriangle[] ClipAndTriangulateTri(WDir oa, WDir ob, WDir oc)
        => ClipAndTriangulate([oa, ob, oc]);

    public RelTriangle[] ClipAndTriangulateIsoscelesTri(WDir apexOffset, WDir height, WDir halfBase)
        => ClipAndTriangulateTri(apexOffset, apexOffset + height + halfBase, apexOffset + height - halfBase);

    public RelTriangle[] ClipAndTriangulateIsoscelesTri(WDir apexOffset, Angle direction, Angle halfAngle, float height)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return ClipAndTriangulateIsoscelesTri(apexOffset, height * dir, height * halfAngle.Tan() * normal);
    }

    public RelSimplifiedComplexPolygon ClipTri(WDir oa, WDir ob, WDir oc)
        => Clip([oa, ob, oc]);

    public RelSimplifiedComplexPolygon ClipIsoscelesTri(WDir apexOffset, WDir height, WDir halfBase)
        => ClipIsoscelesTri(apexOffset, apexOffset + height + halfBase, apexOffset + height - halfBase);

    public RelSimplifiedComplexPolygon ClipIsoscelesTri(WDir apexOffset, Angle direction, Angle halfAngle, float height)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return ClipIsoscelesTri(apexOffset, height * dir, height * halfAngle.Tan() * normal);
    }

    public RelTriangle[] ClipAndTriangulateRect(WDir originOffset, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = originOffset + lenFront * direction;
        var back = originOffset - lenBack * direction;
        return ClipAndTriangulate([front + side, front - side, back - side, back + side]);
    }

    public RelTriangle[] ClipAndTriangulateRect(WDir originOffset, Angle direction, float lenFront, float lenBack, float halfWidth)
        => ClipAndTriangulateRect(originOffset, direction.ToDirection(), lenFront, lenBack, halfWidth);

    public RelTriangle[] ClipAndTriangulateRect(WDir startOffset, WDir endOffset, float halfWidth)
    {
        var dir = (endOffset - startOffset).Normalized();
        var side = halfWidth * dir.OrthoR();
        return ClipAndTriangulate([startOffset + side, startOffset - side, endOffset - side, endOffset + side]);
    }

    public RelTriangle[] TriangulateRect(WDir originOffset, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = originOffset + lenFront * direction;
        var back = originOffset - lenBack * direction;
        return Triangulate(new([front + side, front - side, back - side, back + side]));
    }

    public RelTriangle[] TriangulateRect(WDir originOffset, Angle direction, float lenFront, float lenBack, float halfWidth)
        => TriangulateRect(originOffset, direction.ToDirection(), lenFront, lenBack, halfWidth);

    public RelTriangle[] TriangulateRect(WDir startOffset, WDir endOffset, float halfWidth)
    {
        var dir = (endOffset - startOffset).Normalized();
        var side = halfWidth * dir.OrthoR();
        return Triangulate(new([startOffset + side, startOffset - side, endOffset - side, endOffset + side]));
    }

    public RelSimplifiedComplexPolygon RectPolygon(WDir originOffset, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = originOffset + lenFront * direction;
        var back = originOffset - lenBack * direction;
        return new([front + side, front - side, back - side, back + side]);
    }

    public RelSimplifiedComplexPolygon RectPolygon(WDir startOffset, WDir endOffset, float halfWidth)
    {
        var dir = (endOffset - startOffset).Normalized();
        var side = halfWidth * dir.OrthoR();
        return new([startOffset + side, startOffset - side, endOffset - side, endOffset + side]);
    }

    public RelSimplifiedComplexPolygon ClipRect(WDir originOffset, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = originOffset + lenFront * direction;
        var back = originOffset - lenBack * direction;
        return Clip([front + side, front - side, back - side, back + side]);
    }

    public RelSimplifiedComplexPolygon ClipRect(WDir originOffset, Angle direction, float lenFront, float lenBack, float halfWidth)
        => ClipRect(originOffset, direction.ToDirection(), lenFront, lenBack, halfWidth);

    public RelSimplifiedComplexPolygon ClipRect(WDir startOffset, WDir endOffset, float halfWidth)
    {
        var dir = (endOffset - startOffset).Normalized();
        var side = halfWidth * dir.OrthoR();
        return Clip([startOffset + side, startOffset - side, endOffset - side, endOffset + side]);
    }

    public RelSimplifiedComplexPolygon DonutPolygon(WDir centerOffset, float innerRadius, float outerRadius)
    {
        return new(CurveApprox.DonutL(centerOffset, innerRadius, outerRadius, MaxApproxError));
    }
}

[SkipLocalsInit]
public sealed class ArenaBoundsCircle(float Radius, float MapResolution = 0.5f, bool AllowObstacleMap = false) : ArenaBounds(Radius, MapResolution, allowObstacleMap: AllowObstacleMap)
{
    private Pathfinding.Map? _cachedMap;

    protected override RelSimplifiedComplexPolygon BuildClipPoly()
    {
        RelSimplifiedComplexPolygon poly = new(CurveApprox.Circle(Radius, MaxApproxError));
        poly.InitPolygonIndex();
        return poly;
    }

    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);
    public override bool Contains(in WDir offset)
    {
        var radius = Radius;
        return offset.LengthSq() <= radius * radius;
    }
    public override float IntersectRay(in WDir originOffset, in WDir dir) => Intersect.RayCircle(originOffset, dir, Radius);

    public override WDir ClampToBounds(in WDir offset)
    {
        var radius = Radius;
        return offset.LengthSq() > radius * radius ? offset * radius / offset.Length() : offset;
    }

    private Pathfinding.Map BuildMap()
    {
        var radius = Radius;
        var map = new Pathfinding.Map(MapResolution, default, radius, radius);
        var iCell = 0;

        var width = map.Width;
        var height = map.Height;
        var resolution = map.Resolution;

        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;

        var threshold = radius * radius / (resolution * resolution); // square of bounds radius, in grid coordinates
        var dy = -height / 2 + 0.5f;
        var dx = -width / 2 + 0.5f;

        for (var y = 0; y < height; ++y, ++dy)
        {
            var cy = Math.Abs(dy) + 0.5f; // farthest corner
            var cySq = cy * cy;
            var dx2 = dx;
            for (var x = 0; x < width; ++x, ++dx2)
            {
                var cx = Math.Abs(dx2) + 0.5f;
                if (cx * cx + cySq > threshold)
                {
                    pixelMaxG[iCell] = -1000f;
                    pixelPriority[iCell] = float.MinValue;
                }
                ++iCell;
            }
        }
        return map;
    }

    public override string ToString() => $"{nameof(ArenaBoundsCircle)}, Radius {Radius}, MapResolution: {MapResolution}";
}

// if rotation is 0, half-width is along X and half-height is along Z
[SkipLocalsInit]
public abstract class ABRect : ArenaBounds
{
    public ABRect(float halfWidth, float halfHeight, Angle rotation = default, float MapResolution = 0.5f, bool AllowObstacleMap = false) : base(Math.Max(halfWidth, halfHeight), MapResolution, rotation != default ? CalculateScaleFactor(rotation) : 1f, AllowObstacleMap)
    {
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
        Rotation = rotation;
        Orientation = Rotation.ToDirection();
    }
    public readonly float HalfWidth;
    public readonly float HalfHeight;
    public readonly Angle Rotation;
    private Pathfinding.Map? _cachedMap;
    public readonly WDir Orientation;

    private static float CalculateScaleFactor(Angle Rotation)
    {
        var (sin, cos) = MathF.SinCos(Rotation.Rad);
        return Math.Abs(cos) + Math.Abs(sin);
    }

    protected override RelSimplifiedComplexPolygon BuildClipPoly()
    {
        var dx = Orientation.OrthoL() * HalfWidth;
        var dz = Orientation * HalfHeight;
        RelSimplifiedComplexPolygon poly = new([dx - dz, -dx - dz, -dx + dz, dx + dz]);
        poly.InitPolygonIndex();
        return poly;
    }

    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);

    private Pathfinding.Map BuildMap()
    {
        var halfWidth = HalfWidth;
        var halfHeight = HalfHeight;
        var map = new Pathfinding.Map(MapResolution, default, halfWidth, halfHeight, Rotation);
        // pixels can be partially covered by the rectangle, so we need to rasterize it carefully
        var width = map.Width;
        var height = map.Height;
        var resolution = map.Resolution;
        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;

        var dir = Rotation.ToDirection();
        var dirX = dir.X;
        var dirZ = dir.Z;
        var normal = dir.OrthoL();
        var normalX = normal.X;
        var normalZ = normal.Z;

        var dx = normal * resolution;
        var dy = dir * resolution;
        var startPos = map.Center - ((width >> 1) - 0.5f) * dx - ((height >> 1) - 0.5f) * dy;
        var halfPixel = 0.5f * resolution;

        for (var y = 0; y < height; ++y)
        {
            var posY = startPos + y * dy;
            var rowBase = y * width;
            for (var x = 0; x < width; ++x)
            {
                var pos = posY + x * dx;
                var pX = pos.X;
                var pZ = pos.Z;

                var distParr = pX * dirX + pZ * dirZ;
                var distOrtho = pX * normalX + pZ * normalZ;

                if (!((distParr - halfPixel) >= -halfHeight && (distParr + halfPixel) <= halfHeight) || !((distOrtho - halfPixel) >= -halfWidth && (distOrtho + halfPixel) <= halfWidth))
                {
                    pixelMaxG[rowBase + x] = -1000f;
                    pixelPriority[rowBase + x] = float.MinValue;
                }
            }
        }
        return map;
    }

    public override bool Contains(in WDir offset) => offset.InRect(Orientation, HalfHeight, HalfHeight, HalfWidth);
    public override float IntersectRay(in WDir originOffset, in WDir dir) => Intersect.RayRect(originOffset, dir, Orientation, HalfWidth, HalfHeight);

    public override WDir ClampToBounds(in WDir offset)
    {
        var orientation = Orientation;
        var halfWidth = HalfWidth;
        var halfHeight = HalfHeight;
        var offsetX = offset.Dot(orientation.OrthoL());
        var offsetY = offset.Dot(orientation);
        if (Math.Abs(offsetX) > halfWidth)
        {
            offsetX = Math.Sign(offsetX) * halfWidth;
        }
        if (Math.Abs(offsetY) > halfHeight)
        {
            offsetY = Math.Sign(offsetY) * halfHeight;
        }
        return orientation.OrthoL() * offsetX + orientation * offsetY;
    }
}

[SkipLocalsInit]
public sealed class ArenaBoundsRect(float halfWidth, float halfHeight, Angle rotation = default, float mapResolution = 0.5f, bool allowObstacleMap = false) : ABRect(halfWidth, halfHeight, rotation, mapResolution, allowObstacleMap)
{
    public override string ToString() => $"{nameof(ArenaBoundsRect)}, Radius {Radius}, HalfWidth: {HalfWidth}, HalfHeight: {HalfHeight}, MapResolution: {MapResolution}, ScaleFactor: {ScaleFactor}";
}
[SkipLocalsInit]
public sealed class ArenaBoundsSquare(float halfWidth, Angle rotation = default, float mapResolution = 0.5f, bool allowObstacleMap = false) : ABRect(halfWidth, halfWidth, rotation, mapResolution, allowObstacleMap)
{
    public override string ToString() => $"{nameof(ArenaBoundsSquare)}, Radius {Radius}, HalfWidth: {HalfWidth}, MapResolution: {MapResolution}, ScaleFactor: {ScaleFactor}";
}

// custom complex polygon bounds
// for creating complex bounds by using arrays of shapes
// first array contains platforms that will be united, second optional array contains shapes that will be subtracted
// for convenience third array will optionally perform additional unions at the end
// offset shrinks the pathfinding map only, for example if the edges of the arena are deadly and floating point errors cause the AI to fall of the map or problems like that
// AdjustForHitbox adjusts both the visible map and the pathfinding map (ignores additional unions)
[SkipLocalsInit]
public sealed class ArenaBoundsCustom : ArenaBounds
{
    private Pathfinding.Map? _cachedMap;
    public readonly RelSimplifiedComplexPolygon Polygon;
    public readonly float HalfWidth, HalfHeight;
    private readonly float offset;
    public readonly WPos Center;
    public bool IsCircle; // can be used by gaze component for gazes outside of the arena

    public ArenaBoundsCustom(Shape[] UnionShapes, Shape[]? DifferenceShapes = null, Shape[]? AdditionalShapes = null, float MapResolution = 0.5f, float ScaleFactor = 1f, bool AllowObstacleMap = false, float Offset = default, bool AdjustForHitboxInwards = false, bool AdjustForHitboxOutwards = false)
    : base(BuildBounds(UnionShapes, DifferenceShapes ?? [], AdditionalShapes ?? [], ScaleFactor, AdjustForHitboxInwards, AdjustForHitboxOutwards, out var poly, out var center, out var halfWidth, out var halfHeight), MapResolution, ScaleFactor, AllowObstacleMap)
    {
        Center = center;
        HalfWidth = halfWidth + Offset;
        HalfHeight = halfHeight + Offset;
        Polygon = poly;
        offset = Offset;
    }

    private static float BuildBounds(Shape[] unionShapes, Shape[]? differenceShapes, Shape[]? additionalShapes, float scalefactor, bool adjustForHitboxInwards, bool adjustForHitboxOutwards, out RelSimplifiedComplexPolygon poly, out WPos center, out float halfWidth, out float halfHeight)
    {
        var properties = CalculatePolygonProperties(unionShapes, differenceShapes ?? [], additionalShapes ?? [], adjustForHitboxInwards, adjustForHitboxOutwards);
        center = properties.Center;
        halfWidth = properties.HalfWidth;
        halfHeight = properties.HalfHeight;
        poly = properties.Poly;
        return scalefactor == 1f ? properties.Radius : properties.Radius / scalefactor;
    }

    private static (WPos Center, float HalfWidth, float HalfHeight, float Radius, RelSimplifiedComplexPolygon Poly) CalculatePolygonProperties(Shape[] unionShapes, Shape[] differenceShapes, Shape[] additionalShapes, bool adjustForHitboxInwards, bool adjustForHitboxOutwards)
    {
        var unionPolygons = ParseShapes(unionShapes);
        var differencePolygons = ParseShapes(differenceShapes);
        var additionalPolygons = ParseShapes(additionalShapes);
        var combinedPoly = CombinePolygons(unionPolygons, differencePolygons, additionalPolygons, adjustForHitboxInwards ? -0.5f : adjustForHitboxOutwards ? 0.5f : default);

        var props = combinedPoly.CalculateCenterAndRecenter();
        var center = props.Center;
        var maxX = props.maxX;
        var minX = props.minX;
        var maxZ = props.maxZ;
        var minZ = props.minZ;
        var centerX = center.X;
        var centerZ = center.Z;
        var maxDistX = Math.Max(Math.Abs(maxX - centerX), Math.Abs(minX - centerX));
        var maxDistZ = Math.Max(Math.Abs(maxZ - centerZ), Math.Abs(minZ - centerZ));
        var halfWidth = (maxX - minX) * 0.5f;
        var halfHeight = (maxZ - minZ) * 0.5f;

        return (center, halfWidth, halfHeight, Math.Max(maxDistX, maxDistZ), combinedPoly);

        static RelSimplifiedComplexPolygon[] ParseShapes(Shape[] shapes)
        {
            var lenght = shapes.Length;
            var polygons = new RelSimplifiedComplexPolygon[lenght];
            for (var i = 0; i < lenght; ++i)
            {
                polygons[i] = shapes[i].ToPolygon(default);
            }
            return polygons;
        }
    }

    protected override RelSimplifiedComplexPolygon BuildClipPoly() => Polygon;
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WDir offset) => Polygon.Contains(offset);

    // useful to get forbidden directions if the player is origin of a self knockback
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddForbiddenDirections(in WDir centerOffset, Angle offset, AIHints hints, DateTime activation, float forbiddenDist, float safetyMargin = 1f) => Polygon.AddForbiddenDirections(centerOffset, offset, hints, activation, forbiddenDist, safetyMargin);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float IntersectRay(in WDir originOffset, in WDir dir) => Intersect.RayPolygon(originOffset, dir, Polygon);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override WDir ClampToBounds(in WDir offset)
    {
        if (offset.AlmostEqual(default, 1f) || Math.Abs(offset.X) < 0.1f) // if actor is almost in the center of the arena, do nothing (eg donut arena or wall boss)
        {
            return offset;
        }
        return Polygon.ClosestPointOnBoundary(offset);
    }

    private Pathfinding.Map BuildMap()
    {
        var polygon = offset != default ? Polygon.Offset(offset) : Polygon;
        if (offset != default)
        {
            polygon.InitPolygonIndex();
        }
        var map = new Pathfinding.Map(MapResolution, default, HalfWidth, HalfHeight);

        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;
        var width = map.Width;
        var height = map.Height;
        var resolution = map.Resolution;
        // var startTime = Stopwatch.GetTimestamp();
        // for (var i = 0; i < 10000; ++i)
        // {
        var halfCell = resolution * 0.49999f; // tiny offset to account for floating point inaccuracies and the assumption that being exactly on the polygon border is safe

        var dx = new WDir(resolution, default);
        var dy = new WDir(default, resolution);
        var startPos = map.Center - ((width >> 1) - 0.5f) * dx - ((height >> 1) - 0.5f) * dy;

        Parallel.ForEach(Partitioner.Create(0, height), range =>
        {
            var r1 = range.Item1;
            var r2 = range.Item2;
            for (var y = r1; y < r2; ++y)
            {
                var rowOffset = y * width;
                var posY = startPos + y * dy;

                for (var x = 0; x < width; ++x)
                {
                    var cellCenter = posY + x * dx;
                    var relativeCenter = new WDir(cellCenter.X, cellCenter.Z);

                    var relation = polygon.PolygonAABBIntersection(relativeCenter, halfCell, halfCell);

                    if (relation == PolygonShapeRelation.Inside)
                    {
                        continue;
                    }

                    var index = rowOffset + x;
                    pixelMaxG[index] = -1000f;
                    pixelPriority[index] = float.MinValue;
                }
            }
        });
        // }
        // var rasterFinish = Stopwatch.GetTimestamp();
        // Service.Log($"raster time: {(rasterFinish - startTime) * 1000d / Stopwatch.Frequency}ms");
        return map;
    }

    private static RelSimplifiedComplexPolygon CombinePolygons(RelSimplifiedComplexPolygon[] unionPolygons, RelSimplifiedComplexPolygon[] differencePolygons, RelSimplifiedComplexPolygon[] secondUnionPolygons, float offset)
    {
        var clipper = new PolygonClipper();
        var operandUnion = new PolygonClipper.Operand();
        var operandDifference = new PolygonClipper.Operand();
        var operandSecondUnion = new PolygonClipper.Operand();

        var unionLen = unionPolygons.Length;
        for (var i = 0; i < unionLen; ++i)
        {
            operandUnion.AddPolygon(unionPolygons[i]);
        }
        var differenceLen = differencePolygons.Length;
        for (var i = 0; i < differenceLen; ++i)
        {
            operandDifference.AddPolygon(differencePolygons[i]);
        }
        var secUnionLen = secondUnionPolygons.Length;
        for (var i = 0; i < secUnionLen; ++i)
        {
            operandSecondUnion.AddPolygon(secondUnionPolygons[i]);
        }

        var combinedShape = clipper.Difference(operandUnion, operandDifference);
        var polyAdjust = offset != default ? combinedShape.Offset(offset, Clipper2Lib.JoinType.Round) : combinedShape;
        if (secUnionLen != 0)
        {
            polyAdjust = clipper.Union(new PolygonClipper.Operand(polyAdjust), operandSecondUnion);
        }
        return polyAdjust;
    }

    public override string ToString()
    {
        var parts = Polygon.Parts;
        var count = parts.Count;
        var vertsCount = 0;
        for (var i = 0; i < count; ++i)
        {
            vertsCount += parts[i].Vertices.Count;
        }
        return $"{nameof(ArenaBoundsCustom)}, Radius {Radius}, HalfWidth: {HalfWidth}, HalfHeight: {HalfHeight}, MapResolution: {MapResolution}, Pathfinding offset: {offset}, Vertices: {vertsCount}, ScaleFactor: {ScaleFactor}";
    }
}
