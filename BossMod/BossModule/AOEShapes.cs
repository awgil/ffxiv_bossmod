namespace BossMod;

public abstract record class AOEShape
{
    public abstract bool Check(WPos position, WPos origin, Angle rotation);
    public abstract void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0);
    public abstract void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0);

    public bool Check(WPos position, Actor? origin)
    {
        return origin != null && Check(position, origin.Position, origin.Rotation);
    }

    public Func<WPos, bool> CheckFn(WPos origin, Angle rotation) => p => Check(p, origin, rotation);

    public void Draw(MiniArena arena, Actor? origin, uint color = 0)
    {
        if (origin != null)
            Draw(arena, origin.Position, origin.Rotation, color);
    }

    public void Outline(MiniArena arena, Actor? origin, uint color = 0)
    {
        if (origin != null)
            Outline(arena, origin.Position, origin.Rotation, color);
    }
}

public sealed record class AOEShapeCone(float Radius, Angle HalfAngle, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Cone: r={Radius:f3}, angle={HalfAngle * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCircleCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.ZoneCone(origin, 0, Radius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.AddCone(origin, Radius, rotation + DirectionOffset, HalfAngle, color);
}

public sealed record class AOEShapeCircle(float Radius) : AOEShape
{
    public override string ToString() => $"Circle: r={Radius:f3}";
    public override bool Check(WPos position, WPos origin, Angle rotation = new()) => position.InCircle(origin, Radius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = new(), uint color = 0) => arena.ZoneCircle(origin, Radius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = new(), uint color = 0) => arena.AddCircle(origin, Radius, color);
}

public sealed record class AOEShapeDonut(float InnerRadius, float OuterRadius) : AOEShape
{
    public override string ToString() => $"Donut: r={InnerRadius:f3}-{OuterRadius:f3}";
    public override bool Check(WPos position, WPos origin, Angle rotation = new()) => position.InDonut(origin, InnerRadius, OuterRadius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = new(), uint color = 0) => arena.ZoneDonut(origin, InnerRadius, OuterRadius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = new(), uint color = 0)
    {
        arena.AddCircle(origin, InnerRadius, color);
        arena.AddCircle(origin, OuterRadius, color);
    }
}

public sealed record class AOEShapeDonutSector(float InnerRadius, float OuterRadius, Angle HalfAngle, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Donut sector: r={InnerRadius:f3}-{OuterRadius:f3}, angle={HalfAngle * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.ZoneCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.AddDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
}

public sealed record class AOEShapeRect(float LengthFront, float HalfWidth, float LengthBack = 0, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Rect: l={LengthFront:f3}+{LengthBack:f3}, w={HalfWidth * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.ZoneRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.AddRect(origin, (rotation + DirectionOffset).ToDirection(), LengthFront, LengthBack, HalfWidth, color);
}

public sealed record class AOEShapeCross(float Length, float HalfWidth, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Cross: l={Length:f3}, w={HalfWidth * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, Length, Length, HalfWidth) || position.InRect(origin, rotation + DirectionOffset, HalfWidth, HalfWidth, Length);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.ZonePoly((GetType(), origin, rotation + DirectionOffset, Length, HalfWidth), ContourPoints(origin, rotation), color);

    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0)
    {
        foreach (var p in ContourPoints(origin, rotation))
            arena.PathLineTo(p);
        arena.PathStroke(true, color);
    }

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

// note: it's very rare, not sure it needs to be a common utility - it's an isosceles triangle, a cone with flat base
public sealed record class AOEShapeTriCone(float SideLength, Angle HalfAngle, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"TriCone: side={SideLength:f3}, angle={HalfAngle * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InTri(origin, origin + SideLength * (rotation + HalfAngle).ToDirection(), origin + SideLength * (rotation - HalfAngle).ToDirection());
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.ZoneTri(origin, origin + SideLength * (rotation + HalfAngle).ToDirection(), origin + SideLength * (rotation - HalfAngle).ToDirection(), color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.AddTriangle(origin, origin + SideLength * (rotation + HalfAngle).ToDirection(), origin + SideLength * (rotation - HalfAngle).ToDirection(), color);
}

public sealed record class AOEShapeCustom(RelSimplifiedComplexPolygon Poly, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Custom: off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => Poly.Contains((position - origin).Rotate(-rotation - DirectionOffset));
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.ZoneComplex(origin, rotation + DirectionOffset, Poly, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.AddComplexPolygon(origin, (rotation + DirectionOffset).ToDirection(), Poly, color);
}
