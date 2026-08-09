namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectFixedDirection(WPos Center, WDir Direction, float HalfWidth, float HalfHeight) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction;
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;

    public override bool Contains(in WPos p) => !(p + direction).InRect(center, halfWidth, halfHeight);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectAwayFromOrigin(WPos Center, WPos Origin, float Distance, float HalfWidth, float HalfHeight) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;
    private readonly float distance = Distance;

    public override bool Contains(in WPos p) => !(p + distance * (p - origin).Normalized()).InRect(center, halfWidth, halfHeight);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectLeftRightAlongZAxis(WPos Center, float Distance, float HalfWidth, float HalfHeight) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originZ = Center.Z;
    private readonly WDir dir1 = new(default, Distance);
    private readonly WDir dir2 = new(default, -Distance);
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        if (!(p + (p.Z > originZ ? dir1 : dir2)).InRect(center, halfWidth, halfHeight))
        {
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectLeftRightAlongXAxis(WPos Center, float Distance, float HalfWidth, float HalfHeight) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originZ = Center.Z;
    private readonly WDir dir1 = new(Distance, default);
    private readonly WDir dir2 = new(-Distance, default);
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        if (!(p + (p.Z > originZ ? dir1 : dir2)).InRect(center, halfWidth, halfHeight))
        {
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectLeftRightAlongZAxisPlusAOERects(WPos Center, float Distance, float HalfWidth, float HalfHeight, (WPos Origin, WDir Direction)[] AOEs, float LengthFront, float RectHalfWidth, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originZ = Center.Z;
    private readonly WDir dir1 = new(default, Distance);
    private readonly WDir dir2 = new(default, -Distance);
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;
    private readonly (WPos origin, WDir direction)[] aoes = AOEs;
    private readonly float lenFront = LengthFront;
    private readonly float rectHalfWidth = RectHalfWidth;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + (p.Z > originZ ? dir1 : dir2);

        if (!projected.InRect(center, halfWidth, halfHeight))
        {
            return true;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InRect(aoe.origin, aoe.direction, lenFront, default, rectHalfWidth))
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
public sealed class SDKnockbackInAABBRectFixedDirectionPlusAOECircle(WPos Center, WDir Direction, float HalfWidth, float HalfHeight, WPos CircleOrigin, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float radius = Radius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + direction;
        return !projected.InRect(center, halfWidth, halfHeight) || projected.InCircle(circleOrigin, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectFixedDirectionPlusAOECircles(WPos Center, WDir Direction, float HalfWidth, float HalfHeight, WPos[] Origins, float Radius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;
    private readonly WPos[] origins = Origins;
    private readonly float radius = Radius;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + direction;
        if (!projected.InRect(center, halfWidth, halfHeight))
        {
            return true;
        }

        for (var i = 0; i < len; ++i)
        {
            if (projected.InCircle(origins[i], radius))
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
