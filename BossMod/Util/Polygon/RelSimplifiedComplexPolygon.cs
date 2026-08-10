// using System.IO;
// using System.Globalization;

using Clipper2Lib;

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
    public RelTriangle[] Triangulate()
    {
        return EarCut.Triangulate(this);
    }

    internal (float minX, float maxX, float minZ, float maxZ, WPos Center) CalculateCenterAndRecenter()
    {
        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
        var parts = Parts;
        var count = parts.Count;
        for (var i = 0; i < count; ++i)
        {
            var ext = parts[i].Exterior;
            var len = ext.Length;
            for (var j = 0; j < len; ++j)
            {
                var vertex = ext[j];
                var vX = vertex.X;
                var vZ = vertex.Z;
                if (vX < minX)
                {
                    minX = vX;
                }
                if (vX > maxX)
                {
                    maxX = vX;
                }
                if (vZ < minZ)
                {
                    minZ = vZ;
                }
                if (vZ > maxZ)
                {
                    maxZ = vZ;
                }
            }
        }

        // var sb = new StringBuilder();
        // sb.AppendLine("WPos[] vertices");
        // sb.AppendLine("[");

        // const int perLine = 5;
        // var count = 0;
        // var culture = CultureInfo.InvariantCulture;
        // for (var i = 0; i < count; ++i)
        // {
        //     var verts = combined[i].Vertices;
        //     for (var j = 0; j < verts.Count; ++j)
        //     {
        //         if (count % perLine == 0)
        //             sb.Append("    ");

        //         var v = verts[j];
        //         sb.Append($"new({v.X.ToString(culture)}f, {v.Z.ToString(culture)}f), ");

        //         ++count;

        //         if (count % perLine == 0)
        //             sb.AppendLine();
        //     }
        // }

        // if (count % perLine != 0)
        //     sb.AppendLine();

        // sb.AppendLine("];");

        // File.WriteAllText("vertices.txt", sb.ToString());

        var center = new WPos((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
        var dir = center.ToWDir();
        for (var i = 0; i < count; ++i)
        {
            var verts = CollectionsMarshal.AsSpan(parts[i].Vertices);
            var len = verts.Length;
            for (var j = 0; j < len; ++j)
            {
                ref var vert = ref verts[j];
                vert -= dir;
            }
        }
        InitPolygonIndex();
        return (minX, maxX, minZ, maxZ, center);
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

    // note we assume this method will be called before trying to use the index to avoid null checking every time. if the polygon changes it needs to be called again to update.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal PolygonBoundaryIndex2D InitPolygonIndex()
    {
        return _polyIndex = PolygonBoundaryIndex2D.Build(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal PolygonBoundaryIndex2D VerifyPolygonIndexExistance()
    {
        return _polyIndex ??= PolygonBoundaryIndex2D.Build(this);
    }

    // point-in-polygon test; point is defined as offset from shape center
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in WDir p)
    {
        return _polyIndex!.Contains(p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // useful for knockbacks that have the player as origin to block all angles that intersect the polygon (doesn't matter if outside or inside polygon)
    public void AddForbiddenDirections(in WDir centerOffset, Angle offset, AIHints hints, DateTime activation, float forbiddenDist, float safetyMargin = 1f)
    {
        _polyIndex!.AddForbiddenDirections(centerOffset, offset, hints, activation, forbiddenDist + safetyMargin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Raycast(in WDir originOffset, in WDir dir)
    {
        return _polyIndex!.Raycast(originOffset, dir);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation PolygonCircleIntersection(in WDir originOffset, float radius)
    {
        return _polyIndex!.ClassifyCircle(originOffset, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation PolygonDonutIntersection(in WDir originOffset, float innerRadius, float outerRadius)
    {
        return _polyIndex!.ClassifyDonut(originOffset, innerRadius, outerRadius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation PolygonRectIntersection(in WDir originOffset, in WDir direction, float halfWidth, float halfLength)
    {
        return _polyIndex!.ClassifyRectangle(originOffset, direction, halfWidth, halfLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation PolygonAABBIntersection(in WDir originOffset, float halfWidth, float halfLength)
    {
        return _polyIndex!.ClassifyAABBRect(originOffset, halfWidth, halfLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation PolygonDirectionalRectIntersection(in WDir originOffset, in WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        return _polyIndex!.ClassifyDirectionalRectangle(originOffset, direction, lenFront, lenBack, halfWidth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation PolygonCapsuleIntersection(in WDir originOffset, in WDir direction, float length, float radius)
    {
        return _polyIndex!.ClassifyDirectionalCapsule(originOffset, direction, length, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WDir ClosestPointOnBoundary(in WDir offset)
    {
        return _polyIndex!.ClosestPointOnBoundary(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WDir[] Visibility(in WDir origin)
    {
        return _polyIndex!.VisibilityFrom(origin, this);
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
