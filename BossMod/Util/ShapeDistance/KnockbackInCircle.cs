namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOrigin(WPos Center, WPos Origin, float Distance, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        return !projected.InCircle(center, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginMixedAOEs(WPos Center, WPos Origin, float Distance, float Radius, Components.GenericAOEs.AOEInstance[] AOEs, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly Components.GenericAOEs.AOEInstance[] aoes = AOEs;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InCircle(center, radius))
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

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginPlusAOERects(WPos Center, WPos Origin, float Distance, float Radius, (WPos Origin, WDir Direction)[] AOEs, float LengthFront, float HalfWidth, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly (WPos origin, WDir direction)[] aoes = AOEs;
    private readonly float lenFront = LengthFront;
    private readonly float halfWidth = HalfWidth;
    private readonly int len = Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InCircle(center, radius))
        {
            return true;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InRect(aoe.origin, aoe.direction, lenFront, default, halfWidth))
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
public sealed class SDKnockbackInCircleFixedDirection(WPos Center, WDir Direction, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction;
    private readonly float radius = Radius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => !(p + direction).InCircle(center, radius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginPlusMixedAOEsPlusSingleCircleIntersection(WPos Center, WPos Origin, float Radius, float Distance, SDUnion AOEs, WPos CircleOrigin, float CircleRadius, bool Pull) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly SDUnion aoes = AOEs;
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float circleRadius = CircleRadius;
    private readonly bool pull = Pull;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var dir = (p - origin).Normalized();
        var kb = pull ? -dir : dir;
        var projected = p + distance * kb;

        if (!projected.InCircle(center, radius) && Intersect.RayCircle(p - circleOrigin, kb, circleRadius, distance))
        {
            return true;
        }
        return aoes.Contains(projected);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p)
    {
        var dir = (p - origin).Normalized();
        var kb = pull ? -dir : dir;
        var projected = p + distance * kb;
        if (!projected.InCircle(center, radius) || Intersect.RayCircle(p - circleOrigin, kb, circleRadius, distance))
        {
            return default;
        }
        return aoes.Distance(projected);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleFixedDirectionAndAwayFromOrigin(WPos Center, WPos Origin, WDir Direction, float Distance, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly WDir direction = Direction; // direction including distance for the fixed direction kb
    private readonly float radius = Radius;
    private readonly float distance = Distance; // distance for the away from origin kb

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => !(p + direction).InCircle(center, radius) || !(p + distance * (p - origin).Normalized()).InCircle(center, radius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginIntoDirection(WPos Center, WPos Origin, float Distance, float ArenaRadius, WDir Direction, Angle Tolerance) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float arenaRadius = ArenaRadius;
    private readonly float distance = Distance;
    private readonly SDCone cone = new(Origin, 100f, Direction.ToAngle(), Tolerance);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        return !projected.InCircle(center, arenaRadius) || !cone.Contains(projected);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginMulti3(WPos Center, Components.GenericKnockback.Knockback[] Knockbacks, float Distance, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly Components.GenericKnockback.Knockback[] knockbacks = Knockbacks;
    private readonly float radius = Radius;
    private readonly float distance = Distance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var kb = knockbacks;
        ref readonly var kb0 = ref kb[0];
        var projected = p + distance * (p - kb0.Origin).Normalized();
        if (!projected.InCircle(center, radius))
        {
            return true;
        }
        ref readonly var kb1 = ref kb[1];
        projected += distance * (projected - kb1.Origin).Normalized();
        if (!projected.InCircle(center, radius))
        {
            return true;
        }
        ref readonly var kb2 = ref kb[2];
        projected += distance * (projected - kb2.Origin).Normalized();
        return !projected.InCircle(center, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginMulti2(WPos Center, Components.GenericKnockback.Knockback[] Knockbacks, float Distance, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly Components.GenericKnockback.Knockback[] knockbacks = Knockbacks;
    private readonly float radius = Radius;
    private readonly float distance = Distance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var kb = knockbacks;
        ref readonly var kb0 = ref kb[0];
        var projected = p + distance * (p - kb0.Origin).Normalized();
        if (!projected.InCircle(center, radius))
        {
            return true;
        }
        ref readonly var kb1 = ref kb[1];
        projected += distance * (projected - kb1.Origin).Normalized();
        return !projected.InCircle(center, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginPlusAOECircles(WPos Center, WPos Origin, float Distance, float Radius, WPos[] AOEs, float AOERadius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly WPos[] aoes = AOEs;
    private readonly float aoeRadius = AOERadius;
    private readonly int len = Length;

    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InCircle(center, radius))
        {
            return true;
        }

        for (var i = 0; i < len; ++i)
        {
            if (projected.InCircle(aoes[i], aoeRadius))
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
public sealed class SDKnockbackFixedDirectionIntoCircle(WDir Direction, WPos CircleOrigin, float Radius) : ShapeDistance
{
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float radius = Radius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => !(p + direction).InCircle(circleOrigin, radius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleLeftRightAlongZAxis(WPos Center, float Distance, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originZ = Center.Z;
    private readonly WDir dir1 = new(default, Distance);
    private readonly WDir dir2 = new(default, -Distance);
    private readonly float radius = Radius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        if (!(p + (p.Z > originZ ? dir1 : dir2)).InCircle(center, radius))
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
public sealed class SDKnockbackInCircleLeftRightAlongXAxis(WPos Center, float Distance, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originX = Center.X;
    private readonly WDir dir1 = new(Distance, default);
    private readonly WDir dir2 = new(-Distance, default);
    private readonly float radius = Radius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        if (!(p + (p.X > originX ? dir1 : dir2)).InCircle(center, radius))
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
public sealed class SDKnockbackInCircleAwayFromOriginIntoCircle(WPos Center, WPos Origin, float Distance, float Radius, WPos CircleOrigin, float CircleRadius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float circleRadius = CircleRadius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InCircle(center, radius))
        {
            return true;
        }
        return !projected.InCircle(circleOrigin, circleRadius);

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
