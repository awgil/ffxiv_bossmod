namespace BossMod;

public abstract record class Shape
{
    public static readonly Dictionary<object, List<WDir>> StaticContourCache = [];
    public static readonly Dictionary<object, RelSimplifiedComplexPolygon> StaticPolygonCache = [];
    public const float MaxApproxError = 0.01f;

    public abstract List<WDir> Contour(WPos center);
    public abstract string ComputeHash();

    public RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new(Contour(center))]));

    protected List<WDir> GetOrCreateContour(WPos center, Func<List<WDir>> createContour)
    {
        var key = (ComputeHash(), center);
        if (StaticContourCache.TryGetValue(key, out var cachedResult))
            return cachedResult;
        var result = createContour();
        StaticContourCache[key] = result;
        return result;
    }

    protected RelSimplifiedComplexPolygon GetOrCreatePolygon(WPos center, Func<RelSimplifiedComplexPolygon> createPolygon)
    {
        var key = (ComputeHash(), center);
        if (StaticPolygonCache.TryGetValue(key, out var cachedResult))
            return cachedResult;
        var result = createPolygon();
        StaticPolygonCache[key] = result;
        return result;
    }

    public abstract Func<WPos, float> Distance();

    public static string ComputeSHA512(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA512.HashData(bytes);
        return BitConverter.ToString(hash).Replace("-", "", StringComparison.Ordinal);
    }
}

public record class Circle(WPos Center, float Radius) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
            CurveApprox.Circle(Radius, MaxApproxError).Select(p => p + (Center - center)).ToList());

    public override Func<WPos, float> Distance()
        => ShapeDistance.Circle(Center, Radius);
    public override string ComputeHash() => ComputeSHA512($"{nameof(Circle)}:{Center.X},{Center.Z},{Radius}");
}

// for custom polygons, automatically checking if convex or concave
public record class PolygonCustom(IEnumerable<WPos> Vertices) : Shape
{
    private static readonly Dictionary<string, bool> propertyCache = [];

    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () => Vertices.Select(v => v - center).ToList());

    private bool IsConvex()
    {
        var hash = ComputeHash() + "IsConvex";
        if (propertyCache.TryGetValue(hash, out var isConvex))
            return isConvex;

        var vertices = Vertices.ToList();
        var n = vertices.Count;
        isConvex = true;
        for (var i = 0; i < n; i++)
        {
            var p0 = vertices[i];
            var p1 = vertices[(i + 1) % n];
            var p2 = vertices[(i + 2) % n];

            var crossProduct = (p1.X - p0.X) * (p2.Z - p1.Z) - (p1.Z - p0.Z) * (p2.X - p1.X);
            if (i == 0)
                isConvex = crossProduct > 0;
            else
                if ((crossProduct > 0) != isConvex)
                return propertyCache[hash] = false;
        }
        propertyCache[hash] = isConvex;
        return isConvex;
    }

    private bool IsCounterClockwise()
    {
        var hash = ComputeHash() + "IsCounterClockwise";
        if (propertyCache.TryGetValue(hash, out var isCounterClockwise))
            return isCounterClockwise;

        var vertices = Vertices.ToList();
        float area = 0;
        for (var i = 0; i < vertices.Count; i++)
        {
            var p0 = vertices[i];
            var p1 = vertices[(i + 1) % vertices.Count];
            area += (p1.X - p0.X) * (p1.Z + p0.Z);
        }
        isCounterClockwise = area > 0;
        propertyCache[hash] = isCounterClockwise;
        return isCounterClockwise;
    }

    public override Func<WPos, float> Distance() => IsConvex() ? ShapeDistance.ConvexPolygon(Vertices, !IsCounterClockwise()) : ShapeDistance.ConcavePolygon(Vertices);

    public override string ComputeHash()
    {
        var verticesHash = string.Join(",", Vertices.Select(v => $"{v.X},{v.Z}"));
        return ComputeSHA512($"{nameof(PolygonCustom)}:{verticesHash}");
    }
}

public record class Donut(WPos Center, float InnerRadius, float OuterRadius) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
            CurveApprox.Donut(InnerRadius, OuterRadius, MaxApproxError).Select(p => p + (Center - center)).ToList());

    public override Func<WPos, float> Distance()
        => ShapeDistance.Donut(Center, InnerRadius, OuterRadius);

    public override string ComputeHash() => ComputeSHA512($"{nameof(Donut)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius}");
}

// for rectangles defined by a center, halfwidth, halfheight and optionally rotation
public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
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
        });

    public override Func<WPos, float> Distance()
        => ShapeDistance.Rect(Center, Rotation, HalfHeight, HalfHeight, HalfWidth);

    public override string ComputeHash() => ComputeSHA512($"{nameof(Rectangle)}:{Center.X},{Center.Z},{HalfWidth},{HalfHeight},{Rotation.Rad}");
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
        => GetOrCreateContour(center, () =>
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
        });

    public override Func<WPos, float> Distance()
        => ShapeDistance.Cross(Center, Rotation, Length, HalfWidth);

    public override string ComputeHash() => ComputeSHA512($"{nameof(Cross)}:{Center.X},{Center.Z},{Length},{HalfWidth},{Rotation.Rad}");
}

// Equilateral triangle defined by center, sidelength and rotation
public record class TriangleE(WPos Center, float SideLength, Angle Rotation = default) : Shape

{
    private static readonly float heightFactor = MathF.Sqrt(3) / 2;
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
        {
            var height = SideLength * heightFactor;
            var vertices = new List<WDir> { new(SideLength / 2, height / 2), new(-SideLength / 2, height / 2), new(0, -height / 2) };
            var cos = MathF.Cos(Rotation.Rad);
            var sin = MathF.Sin(Rotation.Rad);
            vertices = vertices.Select(v => new WDir(v.X * cos - v.Z * sin, v.X * sin + v.Z * cos)).ToList();
            return vertices.Select(v => v + (Center - center)).ToList();
        });

    public override Func<WPos, float> Distance() => ShapeDistance.ConvexPolygon(Contour(Center).Select(dir => dir + Center), cw: true);

    public override string ComputeHash() => ComputeSHA512($"{nameof(TriangleE)}:{Center.X},{Center.Z},{SideLength},{Rotation.Rad}");
}

// for polygons defined by a radius and n amount of vertices
public record class Polygon(WPos Center, float Radius, int Vertices, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
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
            return vertices;
        });

    public override Func<WPos, float> Distance() => ShapeDistance.ConvexPolygon(Contour(Center).Select(dir => dir + Center), cw: true);

    public override string ComputeHash() => ComputeSHA512($"{nameof(Polygon)}:{Center.X},{Center.Z},{Radius},{Vertices},{Rotation.Rad}");
}

// for cones defined by radius, start angle and end angle
public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
            CurveApprox.CircleSector(Center, Radius, StartAngle, EndAngle, MaxApproxError).Select(p => p - center).ToList());

    public override Func<WPos, float> Distance()
        => ShapeDistance.Cone(Center, Radius, (StartAngle + EndAngle) / 2, (EndAngle - StartAngle) / 2);

    public override string ComputeHash() => ComputeSHA512($"{nameof(Cone)}:{Center.X},{Center.Z},{Radius},{StartAngle.Rad},{EndAngle.Rad}");
}

// for cones defined by radius, direction and half angle
public record class ConeHA(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle) : Cone(Center, Radius, CenterDir - HalfAngle, CenterDir + HalfAngle);

// for donut segments defined by inner and outer radius, direction, start angle and end angle
public record class DonutSegment(WPos Center, float InnerRadius, float OuterRadius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
            CurveApprox.DonutSector(InnerRadius, OuterRadius, StartAngle, EndAngle, MaxApproxError).Select(p => p + (Center - center)).ToList());

    public override Func<WPos, float> Distance()
        => ShapeDistance.DonutSector(Center, InnerRadius, OuterRadius, (StartAngle + EndAngle) / 2, (EndAngle - StartAngle) / 2);

    public override string ComputeHash() => ComputeSHA512($"{nameof(DonutSegment)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{StartAngle.Rad},{EndAngle.Rad}");
}

// for donut segments defined by inner and outer radius, direction and half angle
public record class DonutSegmentHA(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle) : DonutSegment(Center, InnerRadius, OuterRadius,
CenterDir - HalfAngle, CenterDir + HalfAngle);
