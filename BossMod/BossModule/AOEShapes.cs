namespace BossMod;

public abstract record class AOEShape
{
    public abstract bool Check(WPos position, WPos origin, Angle rotation);
    public abstract void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE);
    public abstract void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger);
    public abstract IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset = 0, float maxError = 1); // positive offset increases area, negative decreases
    public abstract Func<WPos, float> Distance(WPos origin, Angle rotation);

    public bool Check(WPos position, Actor? origin)
    {
        return origin != null && Check(position, origin.Position, origin.Rotation);
    }

    public void Draw(MiniArena arena, Actor? origin, uint color = ArenaColor.AOE)
    {
        if (origin != null)
            Draw(arena, origin.Position, origin.Rotation, color);
    }

    public void Outline(MiniArena arena, Actor? origin, uint color = ArenaColor.Danger)
    {
        if (origin != null)
            Outline(arena, origin.Position, origin.Rotation, color);
    }
}

public sealed record class AOEShapeCone(float Radius, Angle HalfAngle, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Cone: r={Radius:f3}, angle={HalfAngle * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCircleCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneCone(origin, 0, Radius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger) => arena.AddCone(origin, Radius, rotation + DirectionOffset, HalfAngle, color);
    public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
    {
        var centerOffset = offset == 0 ? 0 : offset / HalfAngle.Sin();
        var rot = rotation + DirectionOffset;
        yield return CurveApprox.CircleSector(origin - centerOffset * rot.ToDirection(), Radius + centerOffset + offset, rot - HalfAngle, rot + HalfAngle, maxError);
    }
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Cone(origin, Radius, rotation + DirectionOffset, HalfAngle);
}

public sealed record class AOEShapeCircle(float Radius) : AOEShape
{
    public override string ToString() => $"Circle: r={Radius:f3}";
    public override bool Check(WPos position, WPos origin, Angle rotation = new()) => position.InCircle(origin, Radius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.AOE) => arena.ZoneCircle(origin, Radius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.Danger) => arena.AddCircle(origin, Radius, color);
    public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
    {
        yield return CurveApprox.Circle(origin, Radius + offset, maxError);
    }
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Circle(origin, Radius);
}

public sealed record class AOEShapeDonut(float InnerRadius, float OuterRadius) : AOEShape
{
    public override string ToString() => $"Donut: r={InnerRadius:f3}-{OuterRadius:f3}";
    public override bool Check(WPos position, WPos origin, Angle rotation = new()) => position.InDonut(origin, InnerRadius, OuterRadius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.AOE) => arena.ZoneDonut(origin, InnerRadius, OuterRadius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.Danger)
    {
        arena.AddCircle(origin, InnerRadius, color);
        arena.AddCircle(origin, OuterRadius, color);
    }
    public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
    {
        yield return CurveApprox.Circle(origin, OuterRadius + offset, maxError);
        yield return CurveApprox.Circle(origin, InnerRadius - offset, maxError);
    }
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Donut(origin, InnerRadius, OuterRadius);
}

public sealed record class AOEShapeDonutSector(float InnerRadius, float OuterRadius, Angle HalfAngle, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Donut sector: r={InnerRadius:f3}-{OuterRadius:f3}, angle={HalfAngle * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger) => arena.AddDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
    public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
    {
        var centerOffset = offset == 0 ? 0 : offset / HalfAngle.Sin();
        var rot = rotation + DirectionOffset;
        yield return CurveApprox.DonutSector(origin - centerOffset * rot.ToDirection(), InnerRadius + centerOffset - offset, OuterRadius + centerOffset + offset, rot - HalfAngle, rot + HalfAngle, maxError);
    }
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.DonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
}

public sealed record class AOEShapeRect(float LengthFront, float HalfWidth, float LengthBack = 0, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Rect: l={LengthFront:f3}+{LengthBack:f3}, w={HalfWidth * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger) => arena.AddRect(origin, (rotation + DirectionOffset).ToDirection(), LengthFront, LengthBack, HalfWidth, color);
    public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
    {
        var direction = (rotation + DirectionOffset).ToDirection();
        var side = (HalfWidth + offset) * direction.OrthoR();
        var front = origin + (LengthFront + offset) * direction;
        var back = origin - (LengthBack + offset) * direction;
        yield return new[] { front + side, front - side, back - side, back + side };
    }
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Rect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
}

public sealed record class AOEShapeCross(float Length, float HalfWidth, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Cross: l={Length:f3}, w={HalfWidth * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, Length, Length, HalfWidth) || position.InRect(origin, rotation + DirectionOffset, HalfWidth, HalfWidth, Length);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZonePoly(HashCode.Combine(GetType(), Length, HalfWidth, DirectionOffset), ContourPoints(origin, rotation), color);

    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger)
    {
        foreach (var p in ContourPoints(origin, rotation))
            arena.PathLineTo(p);
        arena.PathStroke(true, color);
    }

    public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
    {
        yield return ContourPoints(origin, rotation, offset);
    }

    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Cross(origin, rotation + DirectionOffset, Length, HalfWidth);

    private IEnumerable<WPos> ContourPoints(WPos origin, Angle rotation, float offset = 0)
    {
        var dx = (rotation + DirectionOffset).ToDirection();
        var dy = dx.OrthoL();
        var dx1 = dx * (Length + offset);
        var dx2 = dx * (HalfWidth + offset);
        var dy1 = dy * (Length + offset);
        var dy2 = dy * (HalfWidth + offset);
        yield return origin + dx1 - dy2;
        yield return origin + dx2 - dy2;
        yield return origin + dx2 - dy1;
        yield return origin - dx2 - dy1;
        yield return origin - dx2 - dy2;
        yield return origin - dx1 - dy2;
        yield return origin - dx1 + dy2;
        yield return origin - dx2 + dy2;
        yield return origin - dx2 + dy1;
        yield return origin + dx2 + dy1;
        yield return origin + dx2 + dy2;
        yield return origin + dx1 + dy2;
    }
}

// TODO: revise and reconsider, not convinced it needs to be here, and it's not well implemented
public sealed record class AOEShapeTriangle(float SideLength, Angle DirectionOffset = default) : AOEShape
{
    public override bool Check(WPos position, WPos origin, Angle rotation)
    {
        var vertices = CalculateVertices(origin, rotation + DirectionOffset);
        return position.InTri(vertices.p1, vertices.p2, vertices.p3);
    }

    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE)
    {
        var vertices = CalculateVertices(origin, rotation + DirectionOffset);
        arena.AddTriangleFilled(vertices.p1, vertices.p2, vertices.p3, color);
    }

    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger)
    {
        var vertices = CalculateVertices(origin, rotation + DirectionOffset);
        arena.AddTriangle(vertices.p1, vertices.p2, vertices.p3, color);
    }

    public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset = 0, float maxError = 1)
    {
        var vertices = CalculateVertices(origin, rotation + DirectionOffset, offset);
        return [new[] { vertices.p1, vertices.p2, vertices.p3 }];
    }

    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        // Implementing an exact distance calculation for a triangle shape might be complex and is beyond the scope of this basic implementation.
        return p => (p - origin).Length(); // Simplified placeholder
    }

    private (WPos p1, WPos p2, WPos p3) CalculateVertices(WPos origin, Angle rotation, float offset = 0)
    {
        // Calculate vertex positions for an equilateral triangle with origin as one vertex
        var sideOffset = (SideLength + offset) / 2;
        var height = MathF.Sqrt(3) / 2 * (SideLength + offset);
        var direction = rotation.ToDirection();
        var ortho = direction.OrthoR();

        var p1 = origin; // The origin is one of the vertices
        var p2 = origin + direction * height - ortho * sideOffset;
        var p3 = origin + direction * height + ortho * sideOffset;

        return (p1, p2, p3);
    }
}
