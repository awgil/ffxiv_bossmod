namespace BossMod;

public abstract record class AOEShape
{
    public abstract bool Check(WPos position, WPos origin, Angle rotation);
    public abstract void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE);
    public abstract void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger);
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
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Cone(origin, Radius, rotation + DirectionOffset, HalfAngle);
}

public sealed record class AOEShapeCircle(float Radius) : AOEShape
{
    public override string ToString() => $"Circle: r={Radius:f3}";
    public override bool Check(WPos position, WPos origin, Angle rotation = new()) => position.InCircle(origin, Radius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.AOE) => arena.ZoneCircle(origin, Radius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.Danger) => arena.AddCircle(origin, Radius, color);
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
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Donut(origin, InnerRadius, OuterRadius);
}

public sealed record class AOEShapeDonutSector(float InnerRadius, float OuterRadius, Angle HalfAngle, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Donut sector: r={InnerRadius:f3}-{OuterRadius:f3}, angle={HalfAngle * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger) => arena.AddDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.DonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
}

public sealed record class AOEShapeRect(float LengthFront, float HalfWidth, float LengthBack = 0, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Rect: l={LengthFront:f3}+{LengthBack:f3}, w={HalfWidth * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger) => arena.AddRect(origin, (rotation + DirectionOffset).ToDirection(), LengthFront, LengthBack, HalfWidth, color);
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Rect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
}

public sealed record class AOEShapeCross(float Length, float HalfWidth, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"Cross: l={Length:f3}, w={HalfWidth * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, Length, Length, HalfWidth) || position.InRect(origin, rotation + DirectionOffset, HalfWidth, HalfWidth, Length);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZonePoly((GetType(), origin, rotation + DirectionOffset, Length, HalfWidth), ContourPoints(origin, rotation), color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger)
    {
        foreach (var p in ContourPoints(origin, rotation))
            arena.PathLineTo(p);
        MiniArena.PathStroke(true, color);
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

// note: it's very rare, not sure it needs to be a common utility - it's an isosceles triangle, a cone with flat base
public sealed record class AOEShapeTriCone(float SideLength, Angle HalfAngle, Angle DirectionOffset = default) : AOEShape
{
    public override string ToString() => $"TriCone: side={SideLength:f3}, angle={HalfAngle * 2}, off={DirectionOffset}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InTri(origin, origin + SideLength * (rotation + HalfAngle).ToDirection(), origin + SideLength * (rotation - HalfAngle).ToDirection());
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneTri(origin, origin + SideLength * (rotation + HalfAngle).ToDirection(), origin + SideLength * (rotation - HalfAngle).ToDirection(), color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger) => arena.AddTriangle(origin, origin + SideLength * (rotation + HalfAngle).ToDirection(), origin + SideLength * (rotation - HalfAngle).ToDirection(), color);
    public override Func<WPos, float> Distance(WPos origin, Angle rotation) => ShapeDistance.Tri(origin, new(default, SideLength * (rotation + HalfAngle).ToDirection(), SideLength * (rotation - HalfAngle).ToDirection()));
}

public sealed record class AOEShapeCustom(List<Shape> UnionShapes, List<Shape> DifferenceShapes) : AOEShape
{
    private static readonly Dictionary<string, List<RelTriangle>> _triangulationCache = [];
    private static readonly Dictionary<object, object> _polygonCacheStatic = [];
    private readonly Dictionary<object, object> _polygonCache = [];
    private static readonly Dictionary<(string, WPos, Angle), Func<WPos, float>> _distanceFuncCache = [];

    private RelSimplifiedComplexPolygon GetCombinedPolygon(WPos origin)
    {
        var cacheKey = CreateCacheKey(UnionShapes, DifferenceShapes);
        if (_polygonCacheStatic.TryGetValue(cacheKey, out var cachedResult))
            return (RelSimplifiedComplexPolygon)cachedResult;

        var unionOperands = new PolygonClipper.Operand();
        foreach (var shape in UnionShapes)
            unionOperands.AddPolygon(shape.ToPolygon(origin));

        var differenceOperands = new PolygonClipper.Operand();
        foreach (var shape in DifferenceShapes)
            differenceOperands.AddPolygon(shape.ToPolygon(origin));

        var clipper = new PolygonClipper();
        var finalResult = clipper.Difference(unionOperands, differenceOperands);

        _polygonCacheStatic[cacheKey] = finalResult;
        return finalResult;
    }

    public override bool Check(WPos position, WPos origin, Angle rotation)
    {
        var cacheKey = (CreateCacheKey(UnionShapes, DifferenceShapes), position, origin, rotation);
        if (_polygonCache.TryGetValue(cacheKey, out var cachedResult))
            return (bool)cachedResult;
        var combinedPolygon = GetCombinedPolygon(origin);
        var relativePosition = position - origin;
        var result = combinedPolygon.Contains(new WDir(relativePosition.X, relativePosition.Z));
        _polygonCache[cacheKey] = result;
        return result;
    }

    private static string CreateCacheKey(IEnumerable<Shape> UnionShapes, IEnumerable<Shape> DifferenceShapes)
    {
        using var sha512 = SHA512.Create();
        var unionKey = string.Join(",", UnionShapes.Select(s => ComputeShapeHash(s, sha512)));
        var differenceKey = string.Join(",", DifferenceShapes.Select(s => ComputeShapeHash(s, sha512)));
        return $"{unionKey}|{differenceKey}";
    }

    private static string ComputeShapeHash(Shape shape, SHA512 sha512)
    {
        var data = Encoding.UTF8.GetBytes(shape.ToString());
        var hash = sha512.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "", StringComparison.Ordinal);
    }

    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE)
    {
        var cacheKey = CreateCacheKey(UnionShapes, DifferenceShapes);
        if (!_triangulationCache.TryGetValue(cacheKey, out var triangles))
        {
            var combinedPolygon = GetCombinedPolygon(origin);
            triangles = combinedPolygon.Triangulate();
            _triangulationCache[cacheKey] = triangles;
        }

        foreach (var triangle in triangles)
        {
            arena.ZoneTri(origin + triangle.A, origin + triangle.B, origin + triangle.C, color); // probably not very efficient to split the polygon into triangles, there must be a better way
        }
    }

    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger)
    {
        var combinedPolygon = GetCombinedPolygon(origin);
        foreach (var part in combinedPolygon.Parts)
        {
            foreach (var (start, end) in part.ExteriorEdges)
            {
                arena.PathLineTo(origin + start);
                arena.PathLineTo(origin + end);
            }
            MiniArena.PathStroke(true, color);
            foreach (var holeIndex in part.Holes)
            {
                foreach (var (start, end) in part.InteriorEdges(holeIndex))
                {
                    arena.PathLineTo(origin + start);
                    arena.PathLineTo(origin + end);
                }
                MiniArena.PathStroke(true, color);
            }
        }
    }

    public override Func<WPos, float> Distance(WPos origin, Angle rotation) // probably not a very efficient way, will make AI pathfinding lag
    {
        var funcCacheKey = (CreateCacheKey(UnionShapes, DifferenceShapes), origin, rotation);
        if (_distanceFuncCache.TryGetValue(funcCacheKey, out var cachedFunc))
            return cachedFunc;

        float distanceFunc(WPos position)
        {
            var combinedPolygon = GetCombinedPolygon(origin);
            var relativePosition = position - origin;
            var distances = new List<float>();
            var inside = combinedPolygon.Contains(relativePosition);

            foreach (var part in combinedPolygon.Parts)
            {
                distances.AddRange(part.ExteriorEdges.Select(edge => DistanceToEdge(relativePosition, edge)));
                foreach (var holeIndex in part.Holes)
                    distances.AddRange(part.InteriorEdges(holeIndex).Select(edge => DistanceToEdge(relativePosition, edge)));
            }

            var minDistance = distances.Min();
            var finalDistance = inside ? -minDistance : minDistance;
            return finalDistance;
        }

        _distanceFuncCache[funcCacheKey] = distanceFunc;
        return distanceFunc;
    }

    private float DistanceToEdge(WDir point, (WDir, WDir) edge)
    {
        var (a, b) = edge;
        var ab = b - a;
        var ap = point - a;
        var abLengthSquared = ab.LengthSq();
        var t = Math.Clamp(Vector2.Dot(ap.ToVec2(), ab.ToVec2()) / abLengthSquared, 0, 1);
        var projection = a + ab * t;
        return (point - projection).Length();
    }
}
