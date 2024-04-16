using ClipperLib;

namespace BossMod;

public class VoronoiDiagram
{
    private readonly List<IntPoint> _points;
    private readonly PolyTree _voronoiDiagram;

    public VoronoiDiagram(List<WPos> points, ClipType clipType = ClipType.ctUnion, PolyFillType fillType = PolyFillType.pftPositive)
    {
        _points = points.Select(p => new IntPoint(p.X * 1000, p.Z * 1000)).ToList();

        var clipper = new Clipper();
        _voronoiDiagram = new PolyTree();
        clipper.AddPaths([_points], PolyType.ptSubject, true);
        clipper.Execute(clipType, _voronoiDiagram, fillType);
    }

    public VoronoiRegion? FindRegion(WPos point)
    {
        var intPoint = new IntPoint(point.X * 1000, point.Z * 1000);
        var nearestIndex = -1;
        var nearestDistance = double.MaxValue;
        var quadtree = new Quadtree(new Rectangle(intPoint.X - 1000, intPoint.Y - 1000, 2000, 2000), _points);
        var nearestPoints = quadtree.Query(new Rectangle(intPoint.X - 1000, intPoint.Y - 1000, intPoint.X + 1000, intPoint.Y + 1000));

        foreach (var p in nearestPoints)
        {
            var distance = MathF.Sqrt(MathF.Pow(p.X - intPoint.X, 2) + MathF.Pow(p.Y - intPoint.Y, 2));
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = _points.IndexOf(p);
            }
        }

        if (nearestIndex == -1)
            return null;
        if (_voronoiDiagram.Childs.Count > nearestIndex)
        {
            var region = _voronoiDiagram.Childs[nearestIndex];
            return new VoronoiRegion(region);
        }
        else
            return new VoronoiRegion(new PolyNode());
    }
}

public class VoronoiRegion(PolyNode region)
{
    private readonly PolyNode _region = region;

    public IEnumerable<VoronoiEdge> Edges
    {
        get
        {
            var edges = new List<VoronoiEdge>();
            foreach (var node in _region.Childs)
                edges.Add(new VoronoiEdge(node.Contour[0], node.Contour[1]));
            return edges;
        }
    }
}

public class VoronoiEdge(IntPoint point1, IntPoint point2)
{
    public WPos Point1 { get; private set; } = new(point1.X / 1000, point1.Y / 1000);
    public WPos Point2 { get; private set; } = new(point2.X / 1000, point2.Y / 1000);

    public float Distance(WPos point)
    {
        var dx = Point2.X - Point1.X;
        var dy = Point2.Z - Point1.Z;
        var a = (dx * dx) + (dy * dy);
        var b = 2 * ((dx * (Point1.X - point.X)) + (dy * (Point1.Z - point.Z)));
        var c = (Point1.X * Point1.X) + (Point1.Z * Point1.Z) + (point.X * point.X) + (point.Z * point.Z) - (2 * (Point1.X * point.X)) - (2 * (Point1.Z * point.Z));
        var discriminant = (b * b) - (4 * a * c);
        if (discriminant < 0)
        {
            return float.MaxValue;
        }
        var t = (-b - MathF.Sqrt(discriminant)) / (2 * a);
        if (t < 0 || t > 1)
        {
            return float.MaxValue;
        }
        var closestPoint = new WPos(Point1.X + t * dx, Point1.Z + t * dy);
        return (point - closestPoint).Length();
    }

    public WPos ClosestPoint(WPos point)
    {
        var dx = Point2.X - Point1.X;
        var dy = Point2.Z - Point1.Z;
        var a = (dx * dx) + (dy * dy);
        var b = 2 * ((dx * (Point1.X - point.X)) + (dy * (Point1.Z - point.Z)));
        var c = (Point1.X * Point1.X) + (Point1.Z * Point1.Z) + (point.X * point.X) + (point.Z * point.Z) - (2 * (Point1.X * point.X)) - (2 * (Point1.Z * point.Z));
        var discriminant = (b * b) - (4 * a * c);
        if (discriminant < 0)
        {
            return (point + (Point1 + Point2) / 2) / 2;
        }
        var t = (-b - MathF.Sqrt(discriminant)) / (2 * a);
        if (t < 0 || t > 1)
        {
            return (point + (Point1 + Point2) / 2) / 2;
        }
        return (Point1 + t * dx + 0.001f) * (-dy).Normalized();
    }
}