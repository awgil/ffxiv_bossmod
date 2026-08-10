namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOrigin : ShapeDistance
{
    public SDKnockbackInComplexPolygonAwayFromOrigin(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon)
    {
        center = Center;
        origin = Origin;
        distance = Distance;
        polygon = Polygon;
        polygon.VerifyPolygonIndexExistance();
    }

    private readonly WPos center;
    private readonly WPos origin;
    private readonly float distance;
    private readonly RelSimplifiedComplexPolygon polygon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => !polygon.Contains(p - center + distance * (p - origin).Normalized());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonFixedDirection : ShapeDistance
{
    public SDKnockbackInComplexPolygonFixedDirection(WPos Center, WDir Direction, RelSimplifiedComplexPolygon Polygon)
    {
        center = Center;
        direction = Direction;
        polygon = Polygon;
        polygon.VerifyPolygonIndexExistance();
    }

    private readonly WPos center;
    private readonly WDir direction;
    private readonly RelSimplifiedComplexPolygon polygon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => !polygon.Contains(p - center + direction);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOriginPlusAOEAABBSquares : ShapeDistance
{
    public SDKnockbackInComplexPolygonAwayFromOriginPlusAOEAABBSquares(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon, WPos[] AOEs, float HalfWidth, int Length)
    {
        center = Center;
        origin = Origin;
        polygon = Polygon;
        distance = Distance;
        aoes = AOEs;
        halfWidth = HalfWidth;
        len = Length;
        polygon.VerifyPolygonIndexExistance();
    }

    private readonly WPos center;
    private readonly WPos origin;
    private readonly RelSimplifiedComplexPolygon polygon;
    private readonly float distance;
    private readonly WPos[] aoes;
    private readonly float halfWidth;
    private readonly int len;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var dir = distance * (p - origin).Normalized();
        if (!polygon.Contains(p - center + dir))
        {
            return true;
        }

        var projected = p + dir;
        for (var i = 0; i < len; ++i)
        {
            if (projected.InSquare(aoes[i], halfWidth))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOriginPlusAOECircles : ShapeDistance
{
    public SDKnockbackInComplexPolygonAwayFromOriginPlusAOECircles(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon, WPos[] AOEs, float Radius, int Length)
    {
        center = Center;
        origin = Origin;
        polygon = Polygon;
        distance = Distance;
        aoes = AOEs;
        radius = Radius;
        len = Length;
        polygon.VerifyPolygonIndexExistance();
    }

    private readonly WPos center;
    private readonly WPos origin;
    private readonly RelSimplifiedComplexPolygon polygon;
    private readonly float distance;
    private readonly WPos[] aoes;
    private readonly float radius;
    private readonly int len;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var dir = distance * (p - origin).Normalized();
        if (!polygon.Contains(p - center + dir))
        {
            return true;
        }

        var projected = p + dir;
        for (var i = 0; i < len; ++i)
        {
            if (projected.InCircle(aoes[i], radius))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOriginPlusIntersectionTest : ShapeDistance
{
    public SDKnockbackInComplexPolygonAwayFromOriginPlusIntersectionTest(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon)
    {
        center = Center;
        origin = Origin;
        distance = Distance;
        polygon = Polygon;
        polygon.VerifyPolygonIndexExistance();
    }

    private readonly WPos center;
    private readonly WPos origin;
    private readonly float distance;
    private readonly RelSimplifiedComplexPolygon polygon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var offset = p - center;
        var dir = (p - origin).Normalized();
        return !polygon.Contains(offset + distance * dir) || polygon.Raycast(offset, dir) < distance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
