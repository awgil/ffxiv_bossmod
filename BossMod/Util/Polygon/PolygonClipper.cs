using Clipper2Lib;

namespace BossMod;

// utility for simplifying and performing boolean operations on complex polygons
[SkipLocalsInit]
public sealed class PolygonClipper
{
    public const float Scale = 1024f * 1024f; // note: we need at least 10 bits for integer part (-1024 to 1024 range); using 11 bits leaves 20 bits for fractional part; power-of-two scale should reduce rounding issues
    public const float InvScale = 1f / Scale;

    // reusable representation of the complex polygon ready for boolean operations
    public sealed class Operand
    {
        public Operand() { }
        public Operand(ReadOnlySpan<WDir> contour, bool isOpen = false) => AddContour(contour, isOpen);
        public Operand(RelPolygonWithHoles polygon) => AddPolygon(polygon);
        public Operand(RelSimplifiedComplexPolygon polygon) => AddPolygon(polygon);

        private readonly ReuseableDataContainer64 _data = new();

        public void Clear() => _data.Clear();

        public void AddContour(ReadOnlySpan<WDir> contour, bool isOpen = false)
        {
            var count = contour.Length;
            Path64 path = [with(count)];
            for (var i = 0; i < count; ++i)
            {
                path.Add(ConvertPoint(contour[i]));
            }
            AddContour(path, isOpen);
        }

        public void AddPolygon(RelPolygonWithHoles polygon)
        {
            AddContour(polygon.Exterior);
            var countH = polygon.HoleStarts.Count;
            for (var i = 0; i < countH; ++i)
            {
                AddContour(polygon.Interior(i));
            }
        }

        public void AddPolygon(RelSimplifiedComplexPolygon polygon)
        {
            var parts = polygon.Parts;
            var count = parts.Count;
            for (var i = 0; i < count; ++i)
            {
                AddPolygon(parts[i]);
            }
        }

        public void Assign(Clipper64 clipper, PathType role) => clipper.AddReuseableData(_data, role);

        private void AddContour(Path64 contour, bool isOpen) => _data.AddPaths([contour], PathType.Subject, isOpen);
    }

    private readonly Clipper64 _clipper = new() { PreserveCollinear = false };

    public RelSimplifiedComplexPolygon Simplify(Operand poly, FillRule fillRule = FillRule.NonZero)
    {
        poly.Assign(_clipper, PathType.Subject);
        return Execute(ClipType.Union, fillRule);
    }

    public RelSimplifiedComplexPolygon Intersect(Operand p1, Operand p2, FillRule fillRule = FillRule.NonZero) => Execute(ClipType.Intersection, fillRule, p1, p2);
    public RelSimplifiedComplexPolygon Union(Operand p1, Operand p2, FillRule fillRule = FillRule.NonZero) => Execute(ClipType.Union, fillRule, p1, p2);
    public RelSimplifiedComplexPolygon Difference(Operand starting, Operand remove, FillRule fillRule = FillRule.NonZero) => Execute(ClipType.Difference, fillRule, starting, remove);
    public RelSimplifiedComplexPolygon Xor(Operand p1, Operand p2, FillRule fillRule = FillRule.NonZero) => Execute(ClipType.Xor, fillRule, p1, p2);

    private RelSimplifiedComplexPolygon Execute(ClipType operation, FillRule fillRule, Operand subject, Operand clip)
    {
        subject.Assign(_clipper, PathType.Subject);
        clip.Assign(_clipper, PathType.Clip);
        return Execute(operation, fillRule);
    }

    private RelSimplifiedComplexPolygon Execute(ClipType operation, FillRule fillRule)
    {
        var solution = new PolyTree64();
        _clipper.Execute(operation, fillRule, solution);
        _clipper.Clear();

        var result = new RelSimplifiedComplexPolygon();
        BuildResult(result, solution);
        return result;
    }

    public static void BuildResult(RelSimplifiedComplexPolygon result, PolyPath64 parent)
    {
        var countP = parent.Count;
        for (var i = 0; i < countP; ++i)
        {
            var exterior = parent[i];
            if (exterior.Polygon == null || exterior.Polygon.Count == 0)
            {
                continue;
            }

            var extPolygon = exterior.Polygon;
            var countExt = exterior.Polygon.Count;
            var polygonPoints = new List<WDir>(countExt);
            for (var j = 0; j < countExt; ++j)
            {
                polygonPoints.Add(ConvertPoint(extPolygon[j]));
            }

            var poly = new RelPolygonWithHoles(polygonPoints);
            result.Parts.Add(poly);
            var countExt2 = exterior.Count;
            for (var j = 0; j < countExt2; ++j)
            {
                var interior = exterior[j];
                if (interior.Polygon == null || interior.Polygon.Count == 0)
                {
                    continue;
                }

                var holePoints = new List<WDir>(interior.Polygon.Count);
                var intPolygon = interior.Polygon;
                var countInt = intPolygon.Count;
                for (var k = 0; k < countInt; ++k)
                {
                    holePoints.Add(ConvertPoint(intPolygon[k]));
                }

                poly.AddHole(holePoints);
                BuildResult(result, interior);
            }
        }
    }

    private static Point64 ConvertPoint(WDir pt) => new(pt.X * Scale, pt.Z * Scale);
    private static WDir ConvertPoint(Point64 pt) => new(pt.X * InvScale, pt.Y * InvScale);
}
