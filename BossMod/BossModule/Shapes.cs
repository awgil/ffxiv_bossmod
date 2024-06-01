namespace BossMod;

public abstract record class Shape
{
    public static readonly Dictionary<object, RelSimplifiedComplexPolygon> StaticCache = [];
    public abstract RelSimplifiedComplexPolygon ToPolygon(WPos center);
    public const float MaxApproxError = 0.01f;
    public abstract string ComputeHash();

    public static string ComputeSHA512(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA512.HashData(bytes);
        return BitConverter.ToString(hash).Replace("-", "", StringComparison.Ordinal);
    }
}

public record class Circle(WPos Center, float Radius) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;
        var vertices = CurveApprox.Circle(Radius, MaxApproxError).Select(p => p + (Center - center)).ToList();
        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(vertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(Circle)}:{Center.X},{Center.Z},{Radius}");
}

// for custom polygons defined by a list of vertices
public record class PolygonCustom(IEnumerable<WPos> Vertices) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;
        var relativeVertices = Vertices.Select(v => v - center).ToList();
        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(relativeVertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash()
    {
        var verticesHash = string.Join(",", Vertices.Select(v => $"{v.X},{v.Z}"));
        return ComputeSHA512($"{nameof(PolygonCustom)}:{verticesHash}");
    }
}

public record class Donut(WPos Center, float InnerRadius, float OuterRadius) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;
        var vertices = CurveApprox.Donut(InnerRadius, OuterRadius, MaxApproxError).Select(p => p + (Center - center)).ToList();
        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(vertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(Donut)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius}");
}

// for rectangles defined by a center, halfwidth, halfheight and optionally rotation
public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;
        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);
        var vertices = new List<WDir>
        {
            new WDir(HalfWidth * cos - HalfHeight * sin, HalfWidth * sin + HalfHeight * cos) + (Center - center),
            new WDir(HalfWidth * cos + HalfHeight * sin, HalfWidth * sin - HalfHeight * cos) + (Center - center),
            new WDir(-HalfWidth * cos + HalfHeight * sin, -HalfWidth * sin - HalfHeight * cos) + (Center - center),
            new WDir(-HalfWidth * cos - HalfHeight * sin, -HalfWidth * sin + HalfHeight * cos) + (Center - center)
        };
        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(vertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

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
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;
        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);

        var verticalVertices = new List<WDir>
        {
            new WDir(HalfWidth * cos - Length * sin, HalfWidth * sin + Length * cos) + (Center - center),
            new WDir(HalfWidth * cos + Length * sin, HalfWidth * sin - Length * cos) + (Center - center),
            new WDir(-HalfWidth * cos + Length * sin, -HalfWidth * sin - Length * cos) + (Center - center),
            new WDir(-HalfWidth * cos - Length * sin, -HalfWidth * sin + Length * cos) + (Center - center)
        };

        var horizontalVertices = new List<WDir>
        {
            new WDir(Length * cos - HalfWidth * sin, Length * sin + HalfWidth * cos) + (Center - center),
            new WDir(Length * cos + HalfWidth * sin, Length * sin - HalfWidth * cos) + (Center - center),
            new WDir(-Length * cos + HalfWidth * sin, -Length * sin - HalfWidth * cos) + (Center - center),
            new WDir(-Length * cos - HalfWidth * sin, -Length * sin + HalfWidth * cos) + (Center - center)
        };

        var polygons = new List<RelPolygonWithHoles>
        {
            new(verticalVertices),
            new(horizontalVertices)
        };
        var result = new RelSimplifiedComplexPolygon(polygons);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(Cross)}:{Center.X},{Center.Z},{Length},{HalfWidth},{Rotation.Rad}");
}

// Equilateral triangle defined by center, radius and rotation
public record class TriangleE(WPos Center, float Radius, Angle Rotation = default) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;

        var sqrt3 = MathF.Sqrt(3);
        var halfSide = Radius;
        var height = halfSide * sqrt3;
        var centerTriangle = Center + new WDir(0, height / 3);

        var vertices = new List<WDir>
        {
            centerTriangle + new WDir(-halfSide, height / 3) - center,
            centerTriangle + new WDir(halfSide, height / 3) - center,
            centerTriangle + new WDir(0, -2 * height / 3) - center
        };

        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);
        var rotatedVertices = vertices.Select(v => new WDir(v.X * cos - v.Z * sin, v.X * sin + v.Z * cos)).ToList();

        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(rotatedVertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(TriangleE)}:{Center.X},{Center.Z},{Radius},{Rotation.Rad}");
}

// custom Triangle defined by three sides and rotation, mind the triangle inequality, side1 + side2 >= side3 
public record class TriangleS(WPos Center, float SideA, float SideB, float SideC, Angle Rotation = default) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;

        var sides = new[] { SideA, SideB, SideC }.OrderByDescending(s => s).ToArray();
        var a = sides[0];
        var b = sides[1];
        var c = sides[2];
        var vertex1 = new WPos(0, 0);
        var vertex2 = new WPos(a, 0);
        var cosC = (b * b + a * a - c * c) / (2 * a * b);
        var sinC = MathF.Sqrt(1 - cosC * cosC);
        var vertex3 = new WPos(b * cosC, b * sinC);
        var centroid = new WPos((vertex1.X + vertex2.X + vertex3.X) / 3, (vertex1.Z + vertex2.Z + vertex3.Z) / 3);

        var vertices = new List<WDir>
        {
            new(vertex3.X - centroid.X, vertex3.Z - centroid.Z),
            new(vertex2.X - centroid.X, vertex2.Z - centroid.Z),
            new(vertex1.X - centroid.X, vertex1.Z - centroid.Z)
        };

        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);
        var rotatedVertices = vertices.Select(v => new WDir(v.X * cos - v.Z * sin, v.X * sin + v.Z * cos)).ToList();
        var adjustedVertices = rotatedVertices.Select(v => v + (Center - center)).ToList();

        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(adjustedVertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(TriangleS)}:{Center.X},{Center.Z},{SideA},{SideB},{SideC},{Rotation.Rad}");
}

// Triangle definded by base length and angle at the apex, apex points north by default
public record class TriangleA(WPos Center, float BaseLength, Angle ApexAngle, Angle Rotation = default) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;

        var apexAngleRad = ApexAngle.Rad;
        var height = BaseLength / 2 / MathF.Tan(apexAngleRad / 2);

        var halfBase = BaseLength / 2;
        var vertex1 = new WPos(-halfBase, 0);
        var vertex2 = new WPos(halfBase, 0);
        var vertex3 = new WPos(0, -height);

        var centroid = new WPos((vertex1.X + vertex2.X + vertex3.X) / 3, (vertex1.Z + vertex2.Z + vertex3.Z) / 3);

        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);
        var vertices = new List<WDir>
            {
                new(vertex1.X - centroid.X, vertex1.Z - centroid.Z),
                new(vertex2.X - centroid.X, vertex2.Z - centroid.Z),
                new(vertex3.X - centroid.X, vertex3.Z - centroid.Z)
            };

        var rotatedVertices = vertices.Select(v => new WDir(v.X * cos - v.Z * sin, v.X * sin + v.Z * cos)).ToList();
        var adjustedVertices = rotatedVertices.Select(v => v + (Center - center)).ToList();

        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(adjustedVertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(TriangleA)}:{Center.X},{Center.Z},{BaseLength},{ApexAngle.Rad},{Rotation.Rad}");
}

// for polygons defined by a radius and n amount of vertices
public record class Polygon(WPos Center, float Radius, int Vertices, Angle Rotation = default) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;

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

        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(vertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(Polygon)}:{Center.X},{Center.Z},{Radius},{Vertices},{Rotation.Rad}");
}

public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;

        var vertices = CurveApprox.CircleSector(Center, Radius, StartAngle, EndAngle, MaxApproxError).Select(p => p - center).ToList();
        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(vertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(Cone)}:{Center.X},{Center.Z},{Radius},{StartAngle.Rad},{EndAngle.Rad}");
}

public record class DonutSegment(WPos Center, float InnerRadius, float OuterRadius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
    {
        if (StaticCache.TryGetValue((ComputeHash(), center), out var cachedResult))
            return cachedResult;

        var vertices = CurveApprox.DonutSector(InnerRadius, OuterRadius, StartAngle, EndAngle, MaxApproxError).Select(p => p + (Center - center)).ToList();
        var result = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(vertices)]);
        StaticCache[(ComputeHash(), center)] = result;
        return result;
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(DonutSegment)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{StartAngle.Rad},{EndAngle.Rad}");
}
