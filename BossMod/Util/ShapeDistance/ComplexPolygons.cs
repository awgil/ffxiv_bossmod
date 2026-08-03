namespace BossMod;

[SkipLocalsInit]
public sealed class SDComplexPolygonInvertedContains(RelSimplifiedComplexPolygon Polygon, WPos Center) : ShapeDistance
{
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;
    private readonly WPos center = Center;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => polygon.Contains(p - center);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public readonly struct SDPolygonWithHolesBase
{
    private readonly RelSimplifiedComplexPolygon _polygon;
    private readonly float _originX, _originZ;
    private readonly Edge[] _edges;
    private readonly SpatialIndex _spatialIndex; // faster but less exact than the PolygonBoundaryIndex2D, should be sufficient though since we test upto 5 points per cell during rasterization anyway

    public SDPolygonWithHolesBase(WPos origin, RelSimplifiedComplexPolygon polygon)
    {
        _originX = origin.X;
        _originZ = origin.Z;
        _polygon = polygon;

        var parts = polygon.Parts;
        var countP = parts.Count;
        var vertsCount = 0;
        for (var i = 0; i < countP; ++i)
        {
            vertsCount += parts[i].Vertices.Count;
        }

        _edges = new Edge[vertsCount];
        var edgeIndex = 0;

        for (var i = 0; i < countP; ++i)
        {
            var part = polygon.Parts[i];
            edgeIndex = AppendEdges(_edges, edgeIndex, part.Exterior, origin);
            var countH = part.HoleStarts.Count;
            for (var j = 0; j < countH; ++j)
            {
                edgeIndex = AppendEdges(_edges, edgeIndex, part.Interior(j), origin);
            }
        }
        _spatialIndex = new(_edges);

        static int AppendEdges(Edge[] destination, int index, ReadOnlySpan<WDir> vertices, WPos origin)
        {
            var count = vertices.Length;
            if (count == 0)
                return index;

            var originX = origin.X;
            var originZ = origin.Z;

            var prev = vertices[count - 1];

            for (var i = 0; i < count; ++i)
            {
                var curr = vertices[i];

                var prevX = prev.X;
                var prevZ = prev.Z;

                destination[index++] = new(originX + prevX, originZ + prevZ, curr.X - prevX, curr.Z - prevZ);

                prev = curr;
            }

            return index;
        }
    }

    public readonly float Distance(in WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        if (_polygon.Contains(new(pX - _originX, pZ - _originZ))) // NOTE: our usecase doesn't care about distance inside of the polygon, so we can short circuit here
        {
            return default;
        }
        var minDistanceSq = float.MaxValue;

        var indices = _spatialIndex.Query(pX, pZ);
        var len = indices.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[indices[i]];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeDx = edge.Dx;
            var edgeDy = edge.Dy;
            var t = Math.Clamp(((pX - edgeAx) * edgeDx + (pZ - edgeAy) * edgeDy) * edge.InvLengthSq, default, 1f);
            var distX = pX - (edgeAx + t * edgeDx);
            var distY = pZ - (edgeAy + t * edgeDy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }
        return MathF.Sqrt(minDistanceSq);
    }

    public readonly float DistanceInverted(in WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        if (!_polygon.Contains(new(pX - _originX, pZ - _originZ))) // NOTE: our usecase doesn't care about distance outside of the polygon, so we can short circuit here
        {
            return default;
        }
        var minDistanceSq = float.MaxValue;

        var indices = _spatialIndex.Query(pX, pZ);
        var len = indices.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[indices[i]];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeDx = edge.Dx;
            var edgeDy = edge.Dy;
            var t = Math.Clamp(((pX - edgeAx) * edgeDx + (pZ - edgeAy) * edgeDy) * edge.InvLengthSq, default, 1f);
            var distX = pX - (edgeAx + t * edgeDx);
            var distY = pZ - (edgeAy + t * edgeDy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }
        return MathF.Sqrt(minDistanceSq);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(in WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        return _polygon.Contains(new(pX - _originX, pZ - _originZ));
    }
}

[SkipLocalsInit]
public sealed class SDPolygonWithHoles(SDPolygonWithHolesBase core) : ShapeDistance
{
    private readonly SDPolygonWithHolesBase _core = core;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => _core.Distance(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => _core.Contains(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDInvertedPolygonWithHoles(SDPolygonWithHolesBase core) : ShapeDistance
{
    private readonly SDPolygonWithHolesBase _core = core;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => _core.DistanceInverted(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => !_core.Contains(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
