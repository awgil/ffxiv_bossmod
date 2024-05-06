using Clipper2Lib;
using EarcutNet;

// currently we use Clipper2 library (based on Vatti algorithm) for boolean operations and Earcut.net library (earcutting) for triangulating
// note: the major user of these primitives is bounds clipper; since they operate in 'local' coordinates, we use WDir everywhere (offsets from center) and call that 'relative polygons' - i'm not quite happy with that, it's not very intuitive
namespace BossMod;

// a triangle; as basic as it gets
public readonly record struct RelTriangle(WDir A, WDir B, WDir C);

// a complex polygon that is a single simple-polygon exterior minus 0 or more simple-polygon holes; all edges are assumed to be non intersecting
// hole-starts list contains starting index of each hole
public record class RelPolygonWithHoles(List<WDir> Vertices, List<int> HoleStarts)
{
    // constructor for simple polygon
    public RelPolygonWithHoles(List<WDir> simpleVertices) : this(simpleVertices, []) { }

    public ReadOnlySpan<WDir> AllVertices => Vertices.AsSpan();
    public ReadOnlySpan<WDir> Exterior => AllVertices[..ExteriorEnd];
    public ReadOnlySpan<WDir> Interior(int index) => AllVertices[HoleStarts[index]..HoleEnd(index)];
    public IEnumerable<int> Holes => Enumerable.Range(0, HoleStarts.Count);
    public IEnumerable<(WDir, WDir)> ExteriorEdges => PolygonUtil.EnumerateEdges(Vertices.Take(ExteriorEnd));
    public IEnumerable<(WDir, WDir)> InteriorEdges(int index) => PolygonUtil.EnumerateEdges(Vertices.Skip(HoleStarts[index]).Take(HoleEnd(index) - HoleStarts[index]));

    public bool IsSimple => HoleStarts.Count == 0;
    public bool IsConvex => IsSimple && PolygonUtil.IsConvex(Exterior);

    private int ExteriorEnd => HoleStarts.Count > 0 ? HoleStarts[0] : Vertices.Count;
    private int HoleEnd(int index) => index + 1 < HoleStarts.Count ? HoleStarts[index + 1] : Vertices.Count;

    // add new hole; input is assumed to be a simple polygon
    public void AddHole(ReadOnlySpan<WDir> simpleHole)
    {
        HoleStarts.Add(Vertices.Count);
        Vertices.AddRange(simpleHole);
    }
    public void AddHole(IEnumerable<WDir> simpleHole)
    {
        HoleStarts.Add(Vertices.Count);
        Vertices.AddRange(simpleHole);
    }

    // build a triangulation of the polygon
    public bool Triangulate(List<RelTriangle> result)
    {
        var pts = new List<double>(Vertices.Count * 2);
        foreach (var p in Vertices)
        {
            pts.Add(p.X);
            pts.Add(p.Z);
        }

        var tess = Earcut.Tessellate(pts, HoleStarts);
        for (int i = 0; i < tess.Count; i += 3)
            result.Add(new(Vertices[tess[i]], Vertices[tess[i + 1]], Vertices[tess[i + 2]]));
        return tess.Count > 0;
    }

    public List<RelTriangle> Triangulate()
    {
        List<RelTriangle> result = [];
        Triangulate(result);
        return result;
    }

    // point-in-polygon test; point is defined as offset from shape center
    public bool Contains(WDir p)
    {
        if (!InSimplePolygon(p, ExteriorEdges))
            return false;
        foreach (var h in Holes)
            if (InSimplePolygon(p, InteriorEdges(h)))
                return false;
        return true;
    }

    private static bool InSimplePolygon(WDir p, IEnumerable<(WDir, WDir)> edges)
    {
        // for simple polygons, it doesn't matter which rule (even-odd, non-zero, etc) we use
        // so let's just use non-zero rule and calculate winding order
        // we need to select arbitrary direction to count winding intersections - let's select unit X
        int winding = 0;
        foreach (var (a, b) in edges)
        {
            // see whether edge ab intersects our test ray - it has to intersect the infinite line on the correct side
            var pa = a - p;
            var pb = b - p;
            // if pa.Z and pb.Z have same signs, the edge is fully above or below the test ray
            if (pa.Z <= 0)
            {
                if (pb.Z > 0 && pa.Cross(pb) > 0)
                    ++winding;
            }
            else
            {
                if (pb.Z <= 0 && pa.Cross(pb) < 0)
                    --winding;
            }
        }
        return winding != 0;
    }
}

// generic 'simplified' complex polygon that consists of 0 or more non-intersecting polygons with holes (note however that some polygons could be fully inside other polygon's hole)
public record class RelSimplifiedComplexPolygon(List<RelPolygonWithHoles> Parts)
{
    public bool IsSimple => Parts.Count == 1 && Parts[0].IsSimple;
    public bool IsConvex => Parts.Count == 1 && Parts[0].IsConvex;

    public RelSimplifiedComplexPolygon() : this([]) { }

    // build a triangulation of the polygon
    public List<RelTriangle> Triangulate()
    {
        List<RelTriangle> result = [];
        foreach (var p in Parts)
            p.Triangulate(result);
        return result;
    }

    // point-in-polygon test; point is defined as offset from shape center
    public bool Contains(WDir p) => Parts.Any(part => part.Contains(p));
}

// utility for simplifying and performing boolean operations on complex polygons
public class PolygonClipper
{
    private const float _scale = 1024 * 1024; // note: we need at least 10 bits for integer part (-1024 to 1024 range); using 11 bits leaves 20 bits for fractional part; power-of-two scale should reduce rounding issues
    private const float _invScale = 1 / _scale;

    // reusable representation of the complex polygon ready for boolean operations
    public record class Operand
    {
        public Operand() { }
        public Operand(ReadOnlySpan<WDir> contour, bool isOpen = false) => AddContour(contour, isOpen);
        public Operand(IEnumerable<WDir> contour, bool isOpen = false) => AddContour(contour, isOpen);
        public Operand(RelPolygonWithHoles polygon) => AddPolygon(polygon);
        public Operand(RelSimplifiedComplexPolygon polygon) => AddPolygon(polygon);

        private readonly ReuseableDataContainer64 _data = new();

        public void Clear() => _data.Clear();

        public void AddContour(ReadOnlySpan<WDir> contour, bool isOpen = false)
        {
            Path64 path = new(contour.Length);
            foreach (var p in contour)
                path.Add(ConvertPoint(p));
            AddContour(path, isOpen);
        }

        public void AddContour(IEnumerable<WDir> contour, bool isOpen = false) => AddContour([.. contour.Select(ConvertPoint)], isOpen);

        public void AddPolygon(RelPolygonWithHoles polygon)
        {
            AddContour(polygon.Exterior);
            foreach (var i in polygon.Holes)
                AddContour(polygon.Interior(i));
        }

        public void AddPolygon(RelSimplifiedComplexPolygon polygon) => polygon.Parts.ForEach(AddPolygon);

        public void Assign(Clipper64 clipper, PathType role) => clipper.AddReuseableData(_data, role);

        private void AddContour(Path64 contour, bool isOpen) => _data.AddPaths([contour], PathType.Subject, isOpen);
    }

    private readonly Clipper64 _clipper = new() { PreserveCollinear = false };

    public RelSimplifiedComplexPolygon Simplify(Operand poly, FillRule fillRule = FillRule.EvenOdd)
    {
        poly.Assign(_clipper, PathType.Subject);
        return Execute(ClipType.Union, fillRule);
    }

    public RelSimplifiedComplexPolygon Intersect(Operand p1, Operand p2, FillRule fillRule = FillRule.EvenOdd) => Execute(ClipType.Intersection, fillRule, p1, p2);
    public RelSimplifiedComplexPolygon Union(Operand p1, Operand p2, FillRule fillRule = FillRule.EvenOdd) => Execute(ClipType.Union, fillRule, p1, p2);
    public RelSimplifiedComplexPolygon Difference(Operand starting, Operand remove, FillRule fillRule = FillRule.EvenOdd) => Execute(ClipType.Difference, fillRule, starting, remove);
    public RelSimplifiedComplexPolygon Xor(Operand p1, Operand p2, FillRule fillRule = FillRule.EvenOdd) => Execute(ClipType.Xor, fillRule, p1, p2);

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

    private static void BuildResult(RelSimplifiedComplexPolygon result, PolyPath64 parent)
    {
        for (var i = 0; i < parent.Count; ++i)
        {
            var exterior = parent[i];
            RelPolygonWithHoles poly = new([.. exterior.Polygon?.Select(ConvertPoint) ?? throw new InvalidOperationException("Unexpected null polygon list")]);
            result.Parts.Add(poly);
            for (var j = 0; j < exterior.Count; ++j)
            {
                var interior = exterior[j];
                poly.AddHole(interior.Polygon?.Select(ConvertPoint) ?? throw new InvalidOperationException("Unexpected null hole list"));

                // add nested polygons
                BuildResult(result, interior);
            }
        }
    }

    private static Point64 ConvertPoint(WDir pt) => new(pt.X * _scale, pt.Z * _scale);
    private static WDir ConvertPoint(Point64 pt) => new(pt.X * _invScale, pt.Y * _invScale);
}

public static class PolygonUtil
{
    public static IEnumerable<(T, T)> EnumerateEdges<T>(IEnumerable<T> contour) where T : struct, IEquatable<T>
    {
        using var e = contour.GetEnumerator();
        if (!e.MoveNext())
            yield break;

        var prev = e.Current;
        var first = prev;
        while (e.MoveNext())
        {
            var curr = e.Current;
            yield return (prev, curr);
            prev = curr;
        }
        if (!first.Equals(prev))
            yield return (prev, first);
    }

    public static bool IsConvex(ReadOnlySpan<WDir> contour)
    {
        // polygon is convex if cross-product of all successive edges has same sign
        if (contour.Length < 3)
            return false;

        var prevEdge = contour[0] - contour[^1];
        float cross = (contour[^1] - contour[^2]).Cross(prevEdge);
        if (contour.Length > 3)
        {
            for (int i = 1; i < contour.Length; ++i)
            {
                var currEdge = contour[i] - contour[i - 1];
                var curCross = prevEdge.Cross(currEdge);
                prevEdge = currEdge;
                if (curCross == 0)
                    continue;
                else if (cross == 0)
                    cross = curCross;
                else if ((cross < 0) != (curCross < 0))
                    return false;
            }
        }
        return cross != 0;
    }
}
