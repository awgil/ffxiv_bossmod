namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareLeftRightAlongXAxisPlusAOECircles(WPos Center, float Distance, float HalfWidth, WPos[] AOEs, float Radius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originX = Center.X;
    private readonly WDir dir1 = new(Distance, default);
    private readonly WDir dir2 = new(-Distance, default);
    private readonly float halfWidth = HalfWidth;
    private readonly WPos[] aoes = AOEs;
    private readonly float radius = Radius;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + (p.X > originX ? dir1 : dir2);
        if (!projected.InSquare(center, halfWidth))
        {
            return true;
        }

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
public sealed class SDKnockbackInAABBSquareLeftRightAlongZAxisTowardsGoal(WPos Center, float Distance, float HalfWidth, WPos GoalPos, float GoalRadius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originZ = Center.Z;
    private readonly WDir dir1 = new(default, Distance);
    private readonly WDir dir2 = new(default, -Distance);
    private readonly float halfWidth = HalfWidth;
    private readonly WPos goalPos = GoalPos;
    private readonly float goalRadius = GoalRadius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var dest = p + (p.Z > originZ ? dir1 : dir2);
        if (!(dest.InSquare(center, halfWidth) && dest.InCircle(goalPos, goalRadius)))
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
public sealed class SDKnockbackInAABBSquareLeftRightAlongXAxisTowardsGoal(WPos Center, float Distance, float HalfWidth, WPos GoalPos, float GoalRadius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originX = Center.X;
    private readonly WDir dir1 = new(Distance, default);
    private readonly WDir dir2 = new(-Distance, default);
    private readonly float halfWidth = HalfWidth;
    private readonly WPos goalPos = GoalPos;
    private readonly float goalRadius = GoalRadius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var dest = p + (p.X > originX ? dir1 : dir2);
        if (!(dest.InSquare(center, halfWidth) && dest.InCircle(goalPos, goalRadius)))
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
public sealed class SDKnockbackInAABBSquareAwayFromOrigin(WPos Center, WPos Origin, float Distance, float HalfWidth) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => !(p + distance * (p - origin).Normalized()).InSquare(center, halfWidth);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusRectAOE(WPos Center, WPos Origin, float Distance, float HalfWidth, WPos RectOrigin, WDir RectDirection, float LengthFront, float RectHalfWidth) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly WPos rectOrigin = RectOrigin;
    private readonly WDir rectDirection = RectDirection;
    private readonly float lengthFront = LengthFront;
    private readonly float rectHalfWidth = RectHalfWidth;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        return !projected.InSquare(center, halfWidth) || !projected.InRect(rectOrigin, rectDirection, lengthFront, default, rectHalfWidth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareAwayFromOriginIntoCircle(WPos Center, WPos Origin, float Distance, float HalfWidth, WPos CircleOrigin, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float radius = Radius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        return !projected.InCircle(circleOrigin, radius) || !projected.InSquare(center, halfWidth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareFixedDirection(WPos Center, WDir Direction, float HalfWidth) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => !(p + direction).InSquare(center, halfWidth);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareFixedDirectionPlusAOECircle(WPos Center, WDir Direction, float HalfWidth, WPos CircleOrigin, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float radius = Radius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + direction;
        return !projected.InSquare(center, halfWidth) || projected.InCircle(circleOrigin, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareFixedDirectionPlusAOECircles(WPos Center, WDir Direction, float HalfWidth, WPos[] Origins, float Radius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly WPos[] origins = Origins;
    private readonly float radius = Radius;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + direction;
        if (!projected.InSquare(center, halfWidth))
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

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusAOECirclesMixedRadii(WPos Center, WPos Origin, float Distance, float HalfWidth, (WPos Origin, float Radius)[] AOEs, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly (WPos origin, float radius)[] aoes = AOEs;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InSquare(center, halfWidth))
        {
            return true;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InCircle(aoe.origin, aoe.radius))
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
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusAOECirclesMixedRadiiPlusAvoidShape(WPos Center, WPos Origin, float Distance, float HalfWidth, (WPos Origin, float Radius)[] AOEs, int Length, ShapeDistance Shape) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly (WPos origin, float radius)[] aoes = AOEs;
    private readonly int len = Length;
    private readonly ShapeDistance shape = Shape;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        if (shape.Distance(p) <= 0f)
        {
            return true;
        }

        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InSquare(center, halfWidth))
        {
            return true;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InCircle(aoe.origin, aoe.radius))
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
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusAOECircles(WPos Center, WPos Origin, float Distance, float HalfWidth, WPos[] AOEs, float Radius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly WPos[] aoes = AOEs;
    private readonly float radius = Radius;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InSquare(center, halfWidth))
        {
            return true;
        }

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
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusAOERects(WPos Center, WPos Origin, float Distance, float HalfWidth, (WPos Origin, WDir Direction)[] AOEs, float LengthFront, float RectHalfWidth, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly (WPos origin, WDir direction)[] aoes = AOEs;
    private readonly float lenFront = LengthFront;
    private readonly float rectHalfWidth = RectHalfWidth;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InSquare(center, halfWidth))
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
public sealed class SDKnockbackInAABBSquareFixedDirectionPlusMixedAOEs(WPos Center, WDir Direction, float HalfWidth, Components.GenericAOEs.AOEInstance[] AOEs, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly Components.GenericAOEs.AOEInstance[] aoes = AOEs;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + direction;
        if (!projected.InSquare(center, halfWidth))
        {
            return true;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Check(projected))
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
