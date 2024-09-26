namespace BossMod;

public abstract record class Shape
{
    public const float MaxApproxError = 0.01f;

    public abstract List<WDir> Contour(WPos center);

    public RelSimplifiedComplexPolygon ToPolygon(WPos center) => new((List<RelPolygonWithHoles>)[new(Contour(center))]);
}

public record class Circle(WPos Center, float Radius) : Shape
{
    public override List<WDir> Contour(WPos center) => CurveApprox.Circle(Radius, MaxApproxError).Select(p => p + (Center - center)).ToList();
    public override string ToString() => $"{nameof(Circle)}:{Center.X},{Center.Z},{Radius}";
}

// for custom polygons, automatically checking if convex or concave
public record class PolygonCustom(IEnumerable<WPos> Vertices) : Shape
{
    public override List<WDir> Contour(WPos center) => Vertices.Select(v => v - center).ToList();
    public override string ToString() => $"{nameof(PolygonCustom)}:{string.Join(",", Vertices.Select(v => $"{v.X},{v.Z}"))}";
}

public record class Donut(WPos Center, float InnerRadius, float OuterRadius) : Shape
{
    public override List<WDir> Contour(WPos center) => CurveApprox.Donut(InnerRadius, OuterRadius, MaxApproxError).Select(p => p + (Center - center)).ToList();
    public override string ToString() => $"{nameof(Donut)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius}";
}

// for rectangles defined by a center, halfwidth, halfheight and optionally rotation
public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);
        return
        [
            new WDir(HalfWidth * cos - HalfHeight * sin, HalfWidth * sin + HalfHeight * cos) + (Center - center),
            new WDir(HalfWidth * cos + HalfHeight * sin, HalfWidth * sin - HalfHeight * cos) + (Center - center),
            new WDir(-HalfWidth * cos + HalfHeight * sin, -HalfWidth * sin - HalfHeight * cos) + (Center - center),
            new WDir(-HalfWidth * cos - HalfHeight * sin, -HalfWidth * sin + HalfHeight * cos) + (Center - center)
        ];
    }
    public override string ToString() => $"{nameof(Rectangle)}:{Center.X},{Center.Z},{HalfWidth},{HalfHeight},{Rotation}";
}

// for rectangles defined by a start point, end point and halfwidth
public record class RectangleSE(WPos Start, WPos End, float HalfWidth) : Rectangle(
    Center: new WPos((Start.X + End.X) / 2, (Start.Z + End.Z) / 2),
    HalfWidth: HalfWidth,
    HalfHeight: (End - Start).Length() / 2,
    Rotation: new Angle(MathF.Atan2(End.Z - Start.Z, End.X - Start.X)) + 90.Degrees()
);

public record class Square(WPos Center, float HalfSize, Angle Rotation = default) : Rectangle(Center, HalfSize, HalfSize, Rotation);

public record class Cross(WPos Center, float Length, float HalfWidth, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var dx = Rotation.ToDirection();
        var dy = dx.OrthoL();
        var dx1 = dx * Length;
        var dx2 = dx * HalfWidth;
        var dy1 = dy * Length;
        var dy2 = dy * HalfWidth;
        return
        [
            Center + dx1 + dy2 - center,
            Center + dx2 + dy2 - center,
            Center + dx2 + dy1 - center,
            Center - dx2 + dy1 - center,
            Center - dx2 + dy2 - center,
            Center - dx1 + dy2 - center,
            Center - dx1 - dy2 - center,
            Center - dx2 - dy2 - center,
            Center - dx2 - dy1 - center,
            Center + dx2 - dy1 - center,
            Center + dx2 - dy2 - center,
            Center + dx1 - dy2 - center
        ];
    }
    public override string ToString() => $"{nameof(Cross)}:{Center.X},{Center.Z},{Length},{HalfWidth},{Rotation}";
}

// Equilateral triangle defined by center, sidelength and rotation
public record class TriangleE(WPos Center, float SideLength, Angle Rotation = default) : Shape
{
    private static readonly float heightFactor = MathF.Sqrt(3) / 2;
    public override List<WDir> Contour(WPos center)
    {
        var height = SideLength * heightFactor;
        List<WDir> vertices = [new(SideLength / 2, height / 2), new(-SideLength / 2, height / 2), new(0, -height / 2)];
        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);
        vertices = vertices.Select(v => new WDir(v.X * cos - v.Z * sin, v.X * sin + v.Z * cos)).ToList();
        return vertices.Select(v => v + (Center - center)).ToList();
    }
    public override string ToString() => $"{nameof(TriangleE)}:{Center.X},{Center.Z},{SideLength},{Rotation}";
}

// for polygons defined by a radius and n amount of vertices
public record class Polygon(WPos Center, float Radius, int Vertices, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var angleIncrement = 2 * MathF.PI / Vertices;
        var initialRotation = Rotation.Rad;
        var vertices = new List<WDir>();
        for (var i = 0; i < Vertices; i++)
        {
            var angle = i * angleIncrement + initialRotation;
            var x = Center.X + Radius * MathF.Cos(angle);
            var z = Center.Z + Radius * MathF.Sin(angle);
            vertices.Add(new WDir(x - center.X, z - center.Z));
        }
        vertices.Reverse();
        return vertices;
    }
    public override string ToString() => $"{nameof(Polygon)}:{Center.X},{Center.Z},{Radius},{Vertices},{Rotation}";
}

// for cones defined by radius, start angle and end angle
public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center) => CurveApprox.CircleSector(Center, Radius, StartAngle, EndAngle, MaxApproxError).Select(p => p - center).ToList();
    public override string ToString() => $"{nameof(Cone)}:{Center.X},{Center.Z},{Radius},{StartAngle},{EndAngle}";
}

// for cones defined by radius, direction and half angle
public record class ConeHA(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle) : Cone(Center, Radius, CenterDir - HalfAngle, CenterDir + HalfAngle);

// for donut segments defined by inner and outer radius, direction, start angle and end angle
public record class DonutSegment(WPos Center, float InnerRadius, float OuterRadius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center) => CurveApprox.DonutSector(InnerRadius, OuterRadius, StartAngle, EndAngle, MaxApproxError).Select(p => p + (Center - center)).ToList();
    public override string ToString() => $"{nameof(DonutSegment)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{StartAngle},{EndAngle}";
}

// for donut segments defined by inner and outer radius, direction and half angle
public record class DonutSegmentHA(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle) : DonutSegment(Center, InnerRadius, OuterRadius,
CenterDir - HalfAngle, CenterDir + HalfAngle);
