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
        var vertexCount = Vertices.Count;
        var pts = new double[vertexCount * 2];

        for (var i = 0; i < vertexCount; i++)
        {
            pts[i * 2] = Vertices[i].X;
            pts[i * 2 + 1] = Vertices[i].Z;
        }

        var tess = Earcut.Tessellate([.. pts[..(vertexCount * 2)]], HoleStarts);
        for (var i = 0; i < tess.Count; i += 3)
            result.Add(new RelTriangle(Vertices[tess[i]], Vertices[tess[i + 1]], Vertices[tess[i + 2]]));
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
        if (!InSimplePolygon(p, Exterior))
            return false;
        for (var i = 0; i < HoleStarts.Count; i++)
        {
            if (InSimplePolygon(p, Interior(i)))
                return false;
        }
        return true;
    }

    private static bool InSimplePolygon(WDir p, ReadOnlySpan<WDir> contour)
    {
        var inside = false;
        var count = contour.Length;
        float x = p.X, y = p.Z;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            var xi = contour[i].X;
            var yi = contour[i].Z;
            var xj = contour[j].X;
            var yj = contour[j].Z;

            if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi + 1e-8f) + xi)
            {
                inside = !inside;
            }
        }
        return inside;
    }

    public static Func<WPos, float> CacheFunction(Func<WPos, float> func)
    {
        var cache = new ConcurrentDictionary<WPos, float>();
        return p => cache.GetOrAdd(p, func);
    }

    public static Func<WPos, float> PolygonWithHoles(WPos origin, RelSimplifiedComplexPolygon polygon)
    {
        List<Edge> edges = [];
        foreach (var part in polygon.Parts)
        {
            AddEdgesFromPart(part, origin, edges);
        }

        var edgeArray = edges.ToArray();
        var spatialIndex = new SpatialIndex(edgeArray, 1);

        float distanceFunc(WPos p)
        {
            var localPoint = new WDir(p.X - origin.X, p.Z - origin.Z);
            var isInside = polygon.Contains(localPoint);

            var minDistanceSq = float.MaxValue;
            foreach (var i in spatialIndex.Query(p.X, p.Z))
            {
                minDistanceSq = Math.Min(minDistanceSq, edgeArray[i].GetClosestDistance(p.X, p.Z));
            }

            var minDistance = MathF.Sqrt(minDistanceSq);
            return isInside ? -minDistance : minDistance;
        }
        return CacheFunction(distanceFunc);
    }

    private static void AddEdgesFromPart(RelPolygonWithHoles part, WPos origin, List<Edge> edges)
    {
        foreach (var edge in PolygonUtil.GetEdges(part.Exterior, origin))
            edges.Add(edge);

        foreach (var holeIndex in part.Holes)
        {
            foreach (var edge in PolygonUtil.GetEdges(part.Interior(holeIndex), origin))
                edges.Add(edge);
        }
    }

    public static Func<WPos, float> InvertedPolygonWithHoles(WPos origin, RelSimplifiedComplexPolygon polygon)
    {
        var polygonWithHoles = PolygonWithHoles(origin, polygon);
        return p => -polygonWithHoles(p);
    }
}

// generic 'simplified' complex polygon that consists of 0 or more non-intersecting polygons with holes (note however that some polygons could be fully inside other polygon's hole)
public record class RelSimplifiedComplexPolygon(List<RelPolygonWithHoles> Parts)
{
    public bool IsSimple => Parts.Count == 1 && Parts[0].IsSimple;
    public bool IsConvex => Parts.Count == 1 && Parts[0].IsConvex;

    public RelSimplifiedComplexPolygon() : this(new List<RelPolygonWithHoles>()) { }

    // constructors for simple polygon
    public RelSimplifiedComplexPolygon(List<WDir> simpleVertices) : this([new RelPolygonWithHoles(simpleVertices)]) { }
    public RelSimplifiedComplexPolygon(IEnumerable<WDir> simpleVertices) : this([new RelPolygonWithHoles([.. simpleVertices])]) { }

    // build a triangulation of the polygon
    public List<RelTriangle> Triangulate()
    {
        List<RelTriangle> result = [];
        foreach (var p in Parts)
            p.Triangulate(result);
        return result;
    }

    // point-in-polygon test; point is defined as offset from shape center
    public bool Contains(WDir p)
    {
        foreach (var part in Parts)
            if (part.Contains(p))
                return true;
        return false;
    }

    // positive offsets inflate, negative shrink polygon
    public RelSimplifiedComplexPolygon Offset(float offset)
    {
        var clipperOffset = new ClipperOffset();
        var exteriorPaths = new List<Path64>();
        var holePaths = new List<Path64>();

        foreach (var part in Parts)
        {
            exteriorPaths.Add(ToPath64(part.Exterior));
            foreach (var i in part.Holes)
                holePaths.Add(ToPath64(part.Interior(i)));
        }

        var exteriorSolution = new Paths64();
        clipperOffset.AddPaths(new Paths64(exteriorPaths), JoinType.Miter, EndType.Polygon);
        clipperOffset.Execute(offset * PolygonClipper.Scale, exteriorSolution);

        clipperOffset.Clear();
        var holeSolution = new Paths64();
        clipperOffset.AddPaths(new Paths64(holePaths), JoinType.Miter, EndType.Polygon);
        clipperOffset.Execute(-offset * PolygonClipper.Scale, holeSolution);

        var result = new RelSimplifiedComplexPolygon();

        foreach (var path in exteriorSolution)
        {
            var vertices = path.Select(pt => new WDir(pt.X * PolygonClipper.InvScale, pt.Y * PolygonClipper.InvScale)).ToList();
            result.Parts.Add(new RelPolygonWithHoles(vertices));
        }

        if (result.Parts.Count > 0)
        {
            foreach (var hole in holeSolution)
            {
                var holeVertices = hole.Select(pt => new WDir(pt.X * PolygonClipper.InvScale, pt.Y * PolygonClipper.InvScale)).ToList();
                result.Parts.Last().AddHole(holeVertices);
            }
        }

        return result;
    }

    private static Path64 ToPath64(ReadOnlySpan<WDir> vertices)
    {
        var path = new Path64(vertices.Length);
        foreach (var vertex in vertices)
            path.Add(new Point64(vertex.X * PolygonClipper.Scale, vertex.Z * PolygonClipper.Scale));
        return path;
    }
}

// utility for simplifying and performing boolean operations on complex polygons
public class PolygonClipper
{
    public const float Scale = 1024 * 1024; // note: we need at least 10 bits for integer part (-1024 to 1024 range); using 11 bits leaves 20 bits for fractional part; power-of-two scale should reduce rounding issues
    public const float InvScale = 1 / Scale;

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

    private static void BuildResult(RelSimplifiedComplexPolygon result, PolyPath64 parent)
    {
        for (var i = 0; i < parent.Count; ++i)
        {
            var exterior = parent[i];
            if (exterior.Polygon == null || exterior.Polygon.Count == 0)
                continue;
            var polygonPoints = new List<WDir>(exterior.Polygon.Count);
            foreach (var pt in exterior.Polygon)
                polygonPoints.Add(ConvertPoint(pt));

            var poly = new RelPolygonWithHoles(polygonPoints);
            result.Parts.Add(poly);

            for (var j = 0; j < exterior.Count; ++j)
            {
                var interior = exterior[j];
                if (interior.Polygon == null || interior.Polygon.Count == 0)
                    continue;
                var holePoints = new List<WDir>(interior.Polygon.Count);
                foreach (var pt in interior.Polygon)
                {
                    holePoints.Add(ConvertPoint(pt));
                }
                poly.AddHole(holePoints);
                BuildResult(result, interior);
            }
        }
    }

    private static Point64 ConvertPoint(WDir pt) => new(pt.X * Scale, pt.Z * Scale);
    private static WDir ConvertPoint(Point64 pt) => new(pt.X * InvScale, pt.Y * InvScale);
}

public static class PolygonUtil
{
    public static Edge[] GetEdges(ReadOnlySpan<WDir> vertices, WPos origin)
    {
        var vertexArray = vertices.ToArray();
        var count = vertexArray.Length;

        var edges = new List<Edge>();

        if (count == 0)
            return [];

        var prev = vertexArray[count - 1];
        for (var i = 0; i < count; i++)
        {
            var curr = vertexArray[i];
            edges.Add(new Edge(origin.X + prev.X, origin.Z + prev.Z, curr.X - prev.X, curr.Z - prev.Z));
            prev = curr;
        }

        return [.. edges];
    }

    public static IEnumerable<(T, T)> EnumerateEdges<T>(IEnumerable<T> contour) where T : struct, IEquatable<T>
    {
        var contourList = contour as IList<T> ?? contour.ToList();
        var count = contourList.Count;
        if (count == 0)
            yield break;

        for (var i = 0; i < count; i++)
        {
            yield return (contourList[i], contourList[(i + 1) % count]);
        }
    }

    public static bool IsConvex(ReadOnlySpan<WDir> contour)
    {
        var isPositive = false;
        for (var i = 0; i < contour.Length; i++)
        {
            var dx1 = contour[(i + 2) % contour.Length].X - contour[(i + 1) % contour.Length].X;
            var dy1 = contour[(i + 2) % contour.Length].Z - contour[(i + 1) % contour.Length].Z;
            var dx2 = contour[i].X - contour[(i + 1) % contour.Length].X;
            var dy2 = contour[i].Z - contour[(i + 1) % contour.Length].Z;
            var cross = dx1 * dy2 - dy1 * dx2;
            if (i == 0)
                isPositive = cross > 0;
            else if ((cross > 0) != isPositive)
                return false;
        }
        return true;
    }
}

public struct Edge(float ax, float ay, float dx, float dy)
{
    public float Ax = ax, Ay = ay, Dx = dx, Dy = dy;
    public float LengthSq = dx * dx + dy * dy + 1e-8f;

    public readonly float GetClosestDistance(float px, float py)
    {
        var t = ((px - Ax) * Dx + (py - Ay) * Dy) / LengthSq;
        t = Math.Clamp(t, 0, 1);

        var closestX = Ax + t * Dx;
        var closestY = Ay + t * Dy;

        var distX = px - closestX;
        var distY = py - closestY;

        return distX * distX + distY * distY;
    }
}

public class SpatialIndex
{
    private readonly Dictionary<(int, int), List<int>> _gridDictionary;
    private readonly float _cellSize;
    private readonly Edge[] _edges;

    public SpatialIndex(Edge[] edges, float cellSize)
    {
        _edges = edges;
        _cellSize = cellSize;
        _gridDictionary = [];

        BuildIndex();
    }

    public void BuildIndex()
    {
        Parallel.For(0, _edges.Length, i =>
        {
            var minX = Math.Min(_edges[i].Ax, _edges[i].Ax + _edges[i].Dx);
            var maxX = Math.Max(_edges[i].Ax, _edges[i].Ax + _edges[i].Dx);
            var minY = Math.Min(_edges[i].Ay, _edges[i].Ay + _edges[i].Dy);
            var maxY = Math.Max(_edges[i].Ay, _edges[i].Ay + _edges[i].Dy);

            var x0 = (int)Math.Floor(minX / _cellSize);
            var x1 = (int)Math.Floor(maxX / _cellSize);
            var y0 = (int)Math.Floor(minY / _cellSize);
            var y1 = (int)Math.Floor(maxY / _cellSize);

            for (var x = x0; x <= x1; x++)
            {
                for (var y = y0; y <= y1; y++)
                {
                    var key = (x, y);
                    lock (_gridDictionary)
                    {
                        if (!_gridDictionary.TryGetValue(key, out var list))
                        {
                            list = [];
                            _gridDictionary[key] = list;
                        }
                        list.Add(i);
                    }
                }
            }
        });
    }

    public List<int> Query(float px, float py)
    {
        var cellX = (int)Math.Floor(px / _cellSize);
        var cellY = (int)Math.Floor(py / _cellSize);
        var key = (cellX, cellY);

        return _gridDictionary.TryGetValue(key, out var list) ? list : [];
    }
}
