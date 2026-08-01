using Clipper2Lib;
using System.Threading;

namespace BossMod;

// generic 'simplified' complex polygon that consists of 0 or more non-intersecting polygons with holes (note however that some polygons could be fully inside other polygon's hole)
[SkipLocalsInit]
public sealed class RelSimplifiedComplexPolygon(List<RelPolygonWithHoles> parts)
{
    public readonly List<RelPolygonWithHoles> Parts = parts;
    private PolygonBoundaryIndex2D? _polyIndex;

    public RelSimplifiedComplexPolygon() : this(new List<RelPolygonWithHoles>()) { }

    // constructors for simple polygon
    public RelSimplifiedComplexPolygon(List<WDir> simpleVertices) : this([new RelPolygonWithHoles(simpleVertices)]) { }

    // build a triangulation of the polygon
    public List<RelTriangle> Triangulate()
    {
        List<RelTriangle> result = [];
        var count = Parts.Count;
        for (var i = 0; i < count; ++i)
        {
            Parts[i].Triangulate(result);
        }
        return result;
    }

    // build a new polygon by transformation
    public RelSimplifiedComplexPolygon Transform(WDir offset, WDir rotation)
    {
        var count = Parts.Count;
        var transformedParts = new List<RelPolygonWithHoles>(count);
        for (var i = 0; i < count; ++i)
        {
            transformedParts.Add(Parts[i].Transform(offset, rotation));
        }
        return new(transformedParts);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal PolygonBoundaryIndex2D GetPolygonIndex(RelSimplifiedComplexPolygon polygon)
    {
        var idx = _polyIndex;
        if (idx == null)
        {
            var built = PolygonBoundaryIndex2D.Build(polygon);
            var original = Interlocked.CompareExchange(ref _polyIndex, built, null);
            idx = original ?? built;
        }
        return idx;
    }

    // point-in-polygon test; point is defined as offset from shape center
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in WDir p)
    {
        var idx = GetPolygonIndex(this);
        return idx.Contains(p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // useful for knockbacks that have the player as origin to block all angles that intersect the polygon (doesn't matter if outside or inside polygon)
    public void AddForbiddenDirections(Actor actor, WPos center, AIHints hints, DateTime activation, float forbiddenDist, float safetyMargin = 1f)
    {
        var idx = GetPolygonIndex(this);
        idx.AddForbiddenDirections(actor, center, this, hints, activation, forbiddenDist + safetyMargin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Raycast(in WDir originOffset, in WDir dir)
    {
        var idx = GetPolygonIndex(this);
        return idx.Raycast(originOffset, dir);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation PolygonCircleIntersection(in WDir originOffset, float radius)
    {
        var idx = GetPolygonIndex(this);
        return idx.ClassifyCircle(originOffset, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation PolygonDonutIntersection(in WDir originOffset, float innerRadius, float outerRadius)
    {
        var idx = GetPolygonIndex(this);
        return idx.ClassifyDonut(originOffset, innerRadius, outerRadius);
    }

    public PolygonShapeRelation PolygonRectIntersection(in WDir originOffset, in WDir direction, float halfWidth, float halfLength)
    {
        var idx = GetPolygonIndex(this);
        return idx.ClassifyRectangle(originOffset, direction, halfWidth, halfLength);
    }

    public PolygonShapeRelation PolygonDirectionalRectIntersection(in WDir originOffset, in WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var idx = GetPolygonIndex(this);
        return idx.ClassifyDirectionalRectangle(originOffset, direction, halfWidth, lenFront, lenBack);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WDir ClosestPointOnBoundary(in WDir offset)
    {
        var idx = GetPolygonIndex(this);
        return idx.ClosestPointOnBoundary(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WDir[] Visibility(in WDir origin)
    {
        var idx = GetPolygonIndex(this);
        return idx.VisibilityFrom(origin, this);
    }

    // positive offsets inflate, negative shrink polygon, use join JoinType Round to simulate a Minkowski Sum with a circle
    public RelSimplifiedComplexPolygon Offset(float offset, JoinType joinType = JoinType.Miter)
    {
        var clipperOffset = new ClipperOffset
        {
            ArcTolerance = 10000d
        };
        var allPaths = new Paths64();
        var count = Parts.Count;
        for (var i = 0; i < count; ++i)
        {
            var part = Parts[i];
            allPaths.Add(ToPath64(part.Exterior));
            var countH = part.HoleStarts.Count;
            for (var j = 0; j < countH; ++j)
            {
                allPaths.Add(ToPath64(part.Interior(j)));
            }
        }

        var solution = new Paths64();
        clipperOffset.AddPaths(allPaths, joinType, EndType.Polygon);
        clipperOffset.Execute(offset * PolygonClipper.Scale, solution);

        var result = new RelSimplifiedComplexPolygon();
        BuildResultFromPaths(result, solution);
        return result;
    }

    public void BuildResultFromPaths(RelSimplifiedComplexPolygon result, Paths64 paths)
    {
        var c = new Clipper64();
        c.AddPaths(paths, PathType.Subject);
        var tree = new PolyTree64();
        c.Execute(ClipType.Union, FillRule.NonZero, tree);

        PolygonClipper.BuildResult(result, tree);
    }

    private static Path64 ToPath64(ReadOnlySpan<WDir> vertices)
    {
        var len = vertices.Length;
        var path = new Path64(len);
        for (var i = 0; i < len; ++i)
        {
            var vertex = vertices[i];
            path.Add(new(vertex.X * PolygonClipper.Scale, vertex.Z * PolygonClipper.Scale));
        }
        return path;
    }
}
