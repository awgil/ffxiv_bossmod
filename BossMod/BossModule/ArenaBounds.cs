namespace BossMod;

// note: if arena bounds are changed, new instance is recreated
// max approx error can change without recreating the instance
// you can use hash-code to cache clipping results - it will change whenever anything in the instance changes
public abstract class ArenaBounds(WPos center, float halfSize)
{
    public WPos Center { get; init; } = center;
    public float HalfSize { get; init; } = halfSize; // largest horizontal/vertical dimension: radius for circle, max of width/height for rect

    // fields below are used for clipping
    public float MaxApproxError { get; private set; } = 0.05f;

    private readonly Clip2D _clipper = new();
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

    public abstract IEnumerable<WPos> BuildClipPoly(float offset = 0); // positive offset increases area, negative decreases
    public abstract Pathfinding.Map BuildMap(float resolution = 0.5f);
    public abstract bool Contains(WPos p);
    public abstract float IntersectRay(WPos origin, WDir dir);
    public override int GetHashCode() => HashCode.Combine(Center, HalfSize, ClipPoly);

    public abstract WDir ClampToBounds(WDir offset, float scale = 1);
    public WPos ClampToBounds(WPos position) => Center + ClampToBounds(position - Center);

    // functions for clipping various shapes to bounds
    public List<(WPos, WPos, WPos)> ClipAndTriangulateCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
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

    public List<(WPos, WPos, WPos)> ClipAndTriangulateCircle(WPos center, float radius)
    {
        return ClipAndTriangulate(CurveApprox.Circle(center, radius, MaxApproxError));
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateDonut(WPos center, float innerRadius, float outerRadius)
    {
        if (innerRadius >= outerRadius || innerRadius < 0)
            return [];
        return ClipAndTriangulate(CurveApprox.Donut(center, innerRadius, outerRadius, MaxApproxError));
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateTri(WPos a, WPos b, WPos c)
    {
        return ClipAndTriangulate([a, b, c]);
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
        return ClipAndTriangulate([front + side, front - side, back - side, back + side]);
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
    {
        return ClipAndTriangulateRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
    }

    public List<(WPos, WPos, WPos)> ClipAndTriangulateRect(WPos start, WPos end, float halfWidth)
    {
        var dir = (end - start).Normalized();
        var side = halfWidth * dir.OrthoR();
        return ClipAndTriangulate([start + side, start - side, end - side, end + side]);
    }

    public static (WPos center, float HalfHeight, float Halfwidth) CalculatePolygonProperties(IEnumerable<WPos> points)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (var point in points)
        {
            if (point.X < minX) minX = point.X;
            if (point.X > maxX) maxX = point.X;
            if (point.Z < minZ) minZ = point.Z;
            if (point.Z > maxZ) maxZ = point.Z;
        }

        float halfWidth = (maxX - minX) / 2;
        float halfHeight = (maxZ - minZ) / 2;
        WPos center = new((minX + maxX) / 2, (minZ + maxZ) / 2);

        return (center, halfHeight, halfWidth);
    }
}

public class ArenaBoundsCircle(WPos center, float radius) : ArenaBounds(center, radius)
{
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

public class ArenaBoundsSquare(WPos center, float halfWidth) : ArenaBounds(center, halfWidth)
{
    public override IEnumerable<WPos> BuildClipPoly(float offset)
    {
        var s = HalfSize + offset;
        yield return Center + new WDir(s, -s);
        yield return Center + new WDir(s, s);
        yield return Center + new WDir(-s, s);
        yield return Center + new WDir(-s, -s);
    }

    public override Pathfinding.Map BuildMap(float resolution) => new(resolution, Center, HalfSize, HalfSize);
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

public class ArenaBoundsRect(WPos center, float halfWidth, float halfHeight, Angle rotation = new()) : ArenaBounds(center, MathF.Max(halfWidth, halfHeight))
{
    public float HalfWidth { get; init; } = halfWidth;
    public float HalfHeight { get; init; } = halfHeight;
    public Angle Rotation { get; init; } = rotation;

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

    public override Pathfinding.Map BuildMap(float resolution) => new(resolution, Center, HalfWidth, HalfHeight, Rotation);
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

//should work for any simple (no self intersections or holes) polygon with an IEnumerable/List of points
public class ArenaBoundsPolygon : ArenaBounds
{
    public float HalfWidth { get; private set; }
    public float HalfHeight { get; private set; }
    public readonly IEnumerable<WPos> Points;

    public ArenaBoundsPolygon(IEnumerable<WPos> points) : base(CalculatePolygonProperties(points).center, MathF.Max(CalculatePolygonProperties(points).Halfwidth, CalculatePolygonProperties(points).HalfHeight))
    {
        Points = points;
        (Center, HalfHeight, HalfWidth) = CalculatePolygonProperties(points);
    }

    public override IEnumerable<WPos> BuildClipPoly(float offset) => Points;

    public override bool Contains(WPos position) => position.InPolygon(Points);

    public override Pathfinding.Map BuildMap(float resolution)
    {
        var map = new Pathfinding.Map(resolution, CalculatePolygonProperties(Points).center, CalculatePolygonProperties(Points).Halfwidth, CalculatePolygonProperties(Points).HalfHeight);
        map.BlockPixelsInside(ShapeDistance.InvertedPolygon(Points), 0, 0);
        return map;
    }

    public override float IntersectRay(WPos origin, WDir dir)
    {
        float minDistance = float.MaxValue;
        int i = 0;
        foreach (var point in Points)
        {
            int j = (i + 1) % Points.Count();
            WPos p0 = point;
            WPos p1 = Points.ElementAt(j);
            float distance = Intersect.RaySegment(origin, dir, p0, p1);
            if (distance < minDistance)
                minDistance = distance;
            ++i;
        }
        return minDistance;
    }

    public override WDir ClampToBounds(WDir offset, float scale)
    {
        float minDistance = float.MaxValue;
        WPos closestPoint = default;

        int i = 0;
        foreach (var point in Points)
        {
            int j = (i + 1) % Points.Count();
            WPos p0 = point;
            WPos p1 = Points.ElementAt(j);
            WPos pointOnSegment = Intersect.ClosestPointOnSegment(p0, p1, Center + offset);
            float distance = (pointOnSegment - (Center + offset)).Length();

            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = pointOnSegment;
            }
            ++i;
        }

        return closestPoint - Center;
    }
}

public class ArenaBoundsDonut(WPos center, float innerRadius, float outerRadius) : ArenaBounds(center, outerRadius)
{
    public float OuterRadius { get; init; } = outerRadius;
    public float InnerRadius { get; init; } = innerRadius;

    public override IEnumerable<WPos> BuildClipPoly(float offset = 0) => CurveApprox.Donut(Center, InnerRadius - offset, OuterRadius + offset, MaxApproxError);

    public override Pathfinding.Map BuildMap(float resolution = 0.5f)
    {
        var map = new Pathfinding.Map(resolution, Center, OuterRadius, OuterRadius);
        map.BlockPixelsInside(ShapeDistance.InvertedDonut(Center, InnerRadius, OuterRadius), 0, 0);
        return map;
    }

    public override bool Contains(WPos position)
    {
        float distanceFromCenter = (position - Center).Length();
        return distanceFromCenter < OuterRadius && distanceFromCenter > InnerRadius;
    }

    public override float IntersectRay(WPos origin, WDir dir)
    {
        float outerIntersection = Intersect.RayCircle(origin, dir, Center, OuterRadius);
        float innerIntersection = Intersect.RayCircle(origin, dir, Center, InnerRadius);
        float minOuter = outerIntersection >= 0 ? outerIntersection : float.MaxValue;
        float minInner = innerIntersection >= 0 ? innerIntersection : float.MaxValue;
        return Math.Min(minOuter, minInner);
    }

    public override WDir ClampToBounds(WDir offset, float scale = 1)
    {
        var normOffset = offset.Normalized();
        var scaledOuter = OuterRadius * scale;
        var scaledInner = InnerRadius * scale;
        var distance = offset.Length();

        if (distance > scaledOuter)
            return normOffset * scaledOuter;
        else if (distance < scaledInner)
            return normOffset * scaledInner;
        return offset;
    }
}

//for unions of different ArenaBounds, shapes that have clockwise winding order get united, counterclockwise differentiated
//buggy if there are more than 2 disjointed shapes or more than one non-self-intersecting hole, consider improving BuildClipPoly if such an arena shape is needed
public class ArenaBoundsUnion : ArenaBounds
{
    private const float ScalingFactor = 1000000;
    private List<ArenaBounds> _boundsList;
    public List<ArenaBounds> BoundsList
    {
        get => _boundsList;
        set
        {
            if (!ReferenceEquals(_boundsList, value)) //clear caches for clamptobounds, intersectray and contains if union composition gets modified, BuildClipPolygon and BuildMap stays incase map gets reverted later
            {
                _boundsList = value;
                _containsCache.Clear();
                _intersectRayCache.Clear();
                _clampToBoundsCache.Clear();
                _cachedUnionProperties = null;
            }
        }
    }

    public ArenaBoundsUnion(IEnumerable<ArenaBounds> bounds) : base(default, default)
    {
        _boundsList = bounds.ToList();
        _cachedUnionProperties = CalculateUnionProperties2(_boundsList);

        var props = _cachedUnionProperties.Value;
        Center = props.Center;
        HalfSize = MathF.Max(props.HalfHeight, props.HalfWidth);
    }

    private (WPos Center, float HalfHeight, float HalfWidth)? _cachedUnionProperties;
    private readonly Dictionary<List<ArenaBounds>, IEnumerable<WPos>> _polyCache = [];
    private readonly Dictionary<WPos, bool> _containsCache = [];
    private readonly Dictionary<(WPos, WDir), float> _intersectRayCache = [];
    private readonly Dictionary<List<ArenaBounds>, Pathfinding.Map> _buildMapCache = [];
    private readonly Dictionary<(WPos, WDir), WDir> _clampToBoundsCache = [];

    private (WPos Center, float HalfHeight, float HalfWidth) CalculateUnionProperties()
    {
        if (!_cachedUnionProperties.HasValue)
            _cachedUnionProperties = CalculateUnionProperties2(_boundsList);
        return _cachedUnionProperties.Value;
    }

    private static (WPos Center, float HalfHeight, float HalfWidth) CalculateUnionProperties2(IEnumerable<ArenaBounds> bounds)
    {
        float totalWeight = 0;
        WPos weightedSum = new(0, 0);
        float maxX = float.MinValue, minX = float.MaxValue, maxZ = float.MinValue, minZ = float.MaxValue;

        foreach (var bound in bounds)
        {
            float weight = bound.HalfSize * bound.HalfSize;
            weightedSum += bound.Center * weight;
            totalWeight += weight;

            var boundPoly = bound.BuildClipPoly().ToList();
            foreach (var point in boundPoly)
            {
                if (point.X > maxX) maxX = point.X;
                if (point.X < minX) minX = point.X;
                if (point.Z > maxZ) maxZ = point.Z;
                if (point.Z < minZ) minZ = point.Z;
            }
        }

        float halfWidth = (maxX - minX) / 2;
        float halfHeight = (maxZ - minZ) / 2;
        WPos center = new((minX + maxX) / 2, (minZ + maxZ) / 2);

        return (center, halfHeight, halfWidth);
    }

    public override IEnumerable<WPos> BuildClipPoly(float offset = 0)
    {
        if (_polyCache.TryGetValue(_boundsList, out var cachedResult))
            return cachedResult;

        var result = new List<WPos>();
        var clipper = new ClipperLib.Clipper(0);
        foreach (var bound in _boundsList)
        {
            var poly = bound.BuildClipPoly(offset).ToList();
            var clipperPoly = poly.Select(p => new ClipperLib.IntPoint(p.X * ScalingFactor, p.Z * ScalingFactor)).ToList();

            if (!ClipperLib.Clipper.Orientation(clipperPoly))
                clipperPoly.Reverse();

            clipper.AddPath(clipperPoly, ClipperLib.PolyType.ptSubject, true);
        }

        var combined = new ClipperLib.PolyTree();
        clipper.Execute(ClipperLib.ClipType.ctUnion, combined, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero);

        var polygons = ClipperLib.Clipper.ClosedPathsFromPolyTree(combined);
        foreach (var solPoly in polygons)
        {
            var polyResult = solPoly.Select(intPoint => new WPos(intPoint.X / ScalingFactor, intPoint.Y / ScalingFactor)).ToList();
            if (polyResult.First().Equals(polyResult.Last()))
                polyResult.RemoveAt(polyResult.Count - 1);

            result.AddRange(polyResult);
            result.Add(polyResult.First());
        }

        _polyCache[_boundsList] = result;
        return result;
    }

    public override Pathfinding.Map BuildMap(float resolution)
    {
        if (_buildMapCache.TryGetValue(_boundsList, out var cachedResult))
            return cachedResult;
        var unionProperties = CalculateUnionProperties();
        var map = new Pathfinding.Map(resolution, unionProperties.Center, unionProperties.HalfWidth, unionProperties.HalfHeight);

        foreach (var (x, y, center) in map.EnumeratePixels())
        {
            bool isInside = false;
            foreach (var bound in _boundsList)
            {
                if (bound.Contains(center))
                {
                    isInside = true;
                    break;
                }
            }
            map.Pixels[y * map.Width + x].MaxG = isInside ? float.MaxValue : 0;
        }
        _buildMapCache[_boundsList] = map;
        return map;
    }

    public override bool Contains(WPos position)
    {
        if (_containsCache.TryGetValue(position, out var cachedResult))
            return cachedResult;
        var combinedPoly = BuildClipPoly().ToList();
        var clipperPoly = combinedPoly.Select(p => new ClipperLib.IntPoint(p.X * ScalingFactor, p.Z * ScalingFactor)).ToList();

        if (!clipperPoly.First().Equals(clipperPoly.Last()))
            clipperPoly.Add(clipperPoly.First());

        var intPoint = new ClipperLib.IntPoint(position.X * ScalingFactor, position.Z * ScalingFactor);
        var result = ClipperLib.Clipper.PointInPolygon(intPoint, clipperPoly) == 1;
        _containsCache[position] = result;
        return result;
    }

    public override float IntersectRay(WPos origin, WDir dir)
    {
        if (_intersectRayCache.TryGetValue((origin, dir), out var cachedResult))
            return cachedResult;

        float nearestIntersection = float.MaxValue;

        var clipper = new ClipperLib.Clipper(0);
        foreach (var bound in _boundsList)
        {
            var poly = bound.BuildClipPoly().Select(p => new ClipperLib.IntPoint(p.X * ScalingFactor, p.Z * ScalingFactor)).ToList();
            if (!ClipperLib.Clipper.Orientation(poly))
                poly.Reverse();
            clipper.AddPath(poly, ClipperLib.PolyType.ptSubject, true);
        }

        var solution = new ClipperLib.PolyTree();
        clipper.Execute(ClipperLib.ClipType.ctUnion, solution, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero);

        var polygons = ClipperLib.Clipper.ClosedPathsFromPolyTree(solution);
        foreach (var poly in polygons)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                ClipperLib.IntPoint currentPoint = poly[i];
                ClipperLib.IntPoint nextPoint = poly[(i + 1) % poly.Count];
                WPos p1 = new(currentPoint.X / ScalingFactor, currentPoint.Y / ScalingFactor);
                WPos p2 = new(nextPoint.X / ScalingFactor, nextPoint.Y / ScalingFactor);

                float distance = Intersect.RaySegment(origin, dir, p1, p2);
                if (distance >= 0 && distance < nearestIntersection)
                    nearestIntersection = distance;
            }
        }

        _intersectRayCache[(origin, dir)] = nearestIntersection;
        return nearestIntersection;
    }

    public override WDir ClampToBounds(WDir offset, float scale = 1)
    {
        WPos position = Center + offset;
        if (_clampToBoundsCache.TryGetValue((position, offset), out var cachedResult))
            return cachedResult;

        var clipper = new ClipperLib.Clipper(0);
        foreach (var bound in _boundsList)
        {
            var poly = bound.BuildClipPoly().Select(p => new ClipperLib.IntPoint(p.X * ScalingFactor, p.Z * ScalingFactor)).ToList();
            if (!ClipperLib.Clipper.Orientation(poly))
                poly.Reverse();
            clipper.AddPath(poly, ClipperLib.PolyType.ptSubject, true);
        }

        var solution = new ClipperLib.PolyTree();
        clipper.Execute(ClipperLib.ClipType.ctUnion, solution, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero);

        if (!Contains(position))
        {
            float closestDist = float.MaxValue;
            WPos closestPoint = default;

            var polygons = ClipperLib.Clipper.ClosedPathsFromPolyTree(solution);
            foreach (var poly in polygons)
            {
                for (int i = 0; i < poly.Count; i++)
                {
                    WPos p1 = new(poly[i].X / ScalingFactor, poly[i].Y / ScalingFactor);
                    WPos p2 = new(poly[(i + 1) % poly.Count].X / ScalingFactor, poly[(i + 1) % poly.Count].Y / ScalingFactor);

                    WPos currentClosest = Intersect.ClosestPointOnSegment(p1, p2, position);
                    float currentDist = (currentClosest - position).Length();

                    if (currentDist < closestDist)
                    {
                        closestDist = currentDist;
                        closestPoint = currentClosest;
                    }
                }
            }
            return closestPoint - Center;
        }
        _clampToBoundsCache[(position, offset)] = offset;
        return offset;
    }
}
