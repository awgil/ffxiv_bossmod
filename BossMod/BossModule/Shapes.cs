namespace BossMod;

public abstract record class Shape
{
    public static readonly Dictionary<object, List<WDir>> StaticContourCache = [];
    public static readonly Dictionary<object, RelSimplifiedComplexPolygon> StaticPolygonCache = [];
    public const float MaxApproxError = 0.01f;

    public abstract List<WDir> Contour(WPos center);
    public abstract RelSimplifiedComplexPolygon ToPolygon(WPos center);
    public abstract string ComputeHash();

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

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));
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

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

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

    public override Func<WPos, float> Distance()
    {
        return IsConvex() ? IsCounterClockwise() ? ShapeDistance.ConvexPolygon(Vertices, false) : ShapeDistance.ConvexPolygon(Vertices, true)
            : ShapeDistance.ConcavePolygon(Vertices);
    }

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

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

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

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

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

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

    public override Func<WPos, float> Distance()
        => ShapeDistance.Cross(Center, Rotation, Length, HalfWidth);

    public override string ComputeHash() => ComputeSHA512($"{nameof(Cross)}:{Center.X},{Center.Z},{Length},{HalfWidth},{Rotation.Rad}");
}

// Equilateral triangle defined by center, radius and rotation
public record class TriangleE(WPos Center, float Radius, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
        {
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
            return vertices.Select(v => new WDir(v.X * cos - v.Z * sin, v.X * sin + v.Z * cos)).ToList();
        });

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

    public override Func<WPos, float> Distance()
    {
        var sqrt3 = MathF.Sqrt(3);
        var halfSide = Radius;
        var height = halfSide * sqrt3;
        var a = new WDir(-halfSide, height / 3);
        var b = new WDir(halfSide, height / 3);
        var c = new WDir(0, -2 * height / 3);

        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);

        var rotatedA = new WDir(a.X * cos - a.Z * sin, a.X * sin + a.Z * cos);
        var rotatedB = new WDir(b.X * cos - b.Z * sin, b.X * sin + b.Z * cos);
        var rotatedC = new WDir(c.X * cos - c.Z * sin, c.X * sin + c.Z * cos);

        var relTriangle = new RelTriangle(rotatedA, rotatedB, rotatedC);

        return ShapeDistance.Tri(Center, relTriangle);
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(TriangleE)}:{Center.X},{Center.Z},{Radius},{Rotation.Rad}");
}

// custom Triangle defined by three sides and rotation, mind the triangle inequality, side1 + side2 >= side3 
public record class TriangleS(WPos Center, float SideA, float SideB, float SideC, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
        {
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
            return vertices.Select(v => new WDir(v.X * cos - v.Z * sin, v.X * sin + v.Z * cos)).ToList();
        });

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

    public override Func<WPos, float> Distance()
    {
        var sides = new[] { SideA, SideB, SideC }.OrderByDescending(s => s).ToArray();
        var a = sides[0];
        var b = sides[1];
        var c = sides[2];
        var vertex1 = new WDir(0, 0);
        var vertex2 = new WDir(a, 0);
        var cosC = (b * b + a * a - c * c) / (2 * a * b);
        var sinC = MathF.Sqrt(1 - cosC * cosC);
        var vertex3 = new WDir(b * cosC, b * sinC);

        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);

        var rotatedVertex1 = new WDir(vertex1.X * cos - vertex1.Z * sin, vertex1.X * sin + vertex1.Z * cos);
        var rotatedVertex2 = new WDir(vertex2.X * cos - vertex2.Z * sin, vertex2.X * sin + vertex2.Z * cos);
        var rotatedVertex3 = new WDir(vertex3.X * cos - vertex3.Z * sin, vertex3.X * sin + vertex3.Z * cos);

        var relTriangle = new RelTriangle(rotatedVertex1, rotatedVertex2, rotatedVertex3);

        return ShapeDistance.Tri(Center, relTriangle);
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(TriangleS)}:{Center.X},{Center.Z},{SideA},{SideB},{SideC},{Rotation.Rad}");
}

// Triangle definded by base length and angle at the apex, apex points north by default
public record class TriangleA(WPos Center, float BaseLength, Angle ApexAngle, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
        {
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

            return vertices.Select(v => new WDir(v.X * cos - v.Z * sin, v.X * sin + v.Z * cos)).ToList();
        });

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

    public override Func<WPos, float> Distance()
    {
        var apexAngleRad = ApexAngle.Rad;
        var height = BaseLength / 2 / MathF.Tan(apexAngleRad / 2);
        var halfBase = BaseLength / 2;
        var vertex1 = new WDir(-halfBase, 0);
        var vertex2 = new WDir(halfBase, 0);
        var vertex3 = new WDir(0, -height);

        var cos = MathF.Cos(Rotation.Rad);
        var sin = MathF.Sin(Rotation.Rad);

        var rotatedVertex1 = new WDir(vertex1.X * cos - vertex1.Z * sin, vertex1.X * sin + vertex1.Z * cos);
        var rotatedVertex2 = new WDir(vertex2.X * cos - vertex2.Z * sin, vertex2.X * sin + vertex2.Z * cos);
        var rotatedVertex3 = new WDir(vertex3.X * cos - vertex3.Z * sin, vertex3.X * sin + vertex3.Z * cos);

        var relTriangle = new RelTriangle(rotatedVertex1, rotatedVertex2, rotatedVertex3);

        return ShapeDistance.Tri(Center, relTriangle);
    }

    public override string ComputeHash() => ComputeSHA512($"{nameof(TriangleA)}:{Center.X},{Center.Z},{BaseLength},{ApexAngle.Rad},{Rotation.Rad}");
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

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

    public override Func<WPos, float> Distance()
        => ShapeDistance.ConvexPolygon(Contour(Center).Select(dir => dir + Center), cw: true);

    public override string ComputeHash() => ComputeSHA512($"{nameof(Polygon)}:{Center.X},{Center.Z},{Radius},{Vertices},{Rotation.Rad}");
}

// for cones defined by radius, start angle and end angle
public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
        => GetOrCreateContour(center, () =>
            CurveApprox.CircleSector(Center, Radius, StartAngle, EndAngle, MaxApproxError).Select(p => p - center).ToList());

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

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

    public override RelSimplifiedComplexPolygon ToPolygon(WPos center)
        => GetOrCreatePolygon(center, () => new RelSimplifiedComplexPolygon([new RelPolygonWithHoles(Contour(center))]));

    public override Func<WPos, float> Distance()
        => ShapeDistance.DonutSector(Center, InnerRadius, OuterRadius, (StartAngle + EndAngle) / 2, (EndAngle - StartAngle) / 2);

    public override string ComputeHash() => ComputeSHA512($"{nameof(DonutSegment)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{StartAngle.Rad},{EndAngle.Rad}");
}

// for donut segments defined by inner and outer radius, direction and half angle
public record class DonutSegmentHA(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle) : DonutSegment(Center, InnerRadius, OuterRadius,
CenterDir - HalfAngle, CenterDir + HalfAngle);
