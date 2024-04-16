using ClipperLib;

namespace BossMod;

public class Quadtree
{
    private readonly Rectangle _boundary;
    private readonly List<IntPoint> _points;
    private readonly Quadtree?[] _nodes;

    public Quadtree(Rectangle boundary, List<IntPoint> points)
    {
        _boundary = boundary;
        _points = points;
        _nodes = new Quadtree?[4];

        if (_points.Count > 1)
            Split();
    }

    public void Split()
    {
        var subWidth = _boundary.Width / 2;
        var subHeight = _boundary.Height / 2;

        _nodes[0] = new Quadtree(new Rectangle(_boundary.X, _boundary.Y, subWidth, subHeight), _points.FindAll(p => p.X >= _boundary.X && p.X < _boundary.X + subWidth && p.Y >= _boundary.Y && p.Y < _boundary.Y + subHeight));
        _nodes[1] = new Quadtree(new Rectangle(_boundary.X + subWidth, _boundary.Y, subWidth, subHeight), _points.FindAll(p => p.X >= _boundary.X + subWidth && p.X < _boundary.X + _boundary.Width && p.Y >= _boundary.Y && p.Y < _boundary.Y + subHeight));
        _nodes[2] = new Quadtree(new Rectangle(_boundary.X, _boundary.Y + subHeight, subWidth, subHeight), _points.FindAll(p => p.X >= _boundary.X && p.X < _boundary.X + subWidth && p.Y >= _boundary.Y + subHeight && p.Y < _boundary.Y + _boundary.Height));
        _nodes[3] = new Quadtree(new Rectangle(_boundary.X + subWidth, _boundary.Y + subHeight, subWidth, subHeight), _points.FindAll(p => p.X >= _boundary.X + subWidth && p.X < _boundary.X + _boundary.Width && p.Y >= _boundary.Y + subHeight && p.Y < _boundary.Y + _boundary.Height));
    }

    public List<IntPoint> Query(Rectangle queryBoundary)
    {
        var result = new List<IntPoint>();

        if (!_boundary.Intersects(queryBoundary))
        {
            return result;
        }

        foreach (var point in _points)
        {
            if (queryBoundary.Contains(point))
            {
                result.Add(point);
            }
        }

        if (_nodes[0] != null)
        {
            result.AddRange(_nodes[0]!.Query(queryBoundary));
            result.AddRange(_nodes[1]!.Query(queryBoundary));
            result.AddRange(_nodes[2]!.Query(queryBoundary));
            result.AddRange(_nodes[3]!.Query(queryBoundary));
        }

        return result;
    }

    public bool Remove(IntPoint point)
    {
        if (!_boundary.Contains(point))
            return false;

        if (_points.Remove(point))
            return true;

        if (_nodes[0] != null)
        {
            if (_nodes[0]!.Remove(point))
                return true;
            if (_nodes[1]!.Remove(point))
                return true;
            if (_nodes[2]!.Remove(point))
                return true;
            if (_nodes[3]!.Remove(point))
                return true;
        }

        return false;
    }

    public void Balance()
    {
        if (_nodes[0] != null)
        {
            _nodes[0]!.Balance();
            _nodes[1]!.Balance();
            _nodes[2]!.Balance();
            _nodes[3]!.Balance();

            if (_nodes[0]!._points.Count < 10 && _nodes[1]!._points.Count < 10 && _nodes[2]!._points.Count < 10 && _nodes[3]!._points.Count < 10)
            {
                Merge();
            }
        }
    }

    public void Merge()
    {
        if (_nodes[0]!= null)
        {
            _points.AddRange(_nodes[0]!._points);
            _points.AddRange(_nodes[1]!._points);
            _points.AddRange(_nodes[2]!._points);
            _points.AddRange(_nodes[3]!._points);

            _nodes[0] = null;
            _nodes[1] = null;
            _nodes[2] = null;
            _nodes[3] = null;
        }
    }
}

public class Rectangle(float x, float y, float width, float height)
{
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
    public float Width { get; set; } = width;
    public float Height { get; set; } = height;

    public bool Intersects(Rectangle other)
    {
        return X < other.X + other.Width && X + Width > other.X && Y < other.Y + other.Height && Y + Height > other.Y;
    }

    public bool Contains(IntPoint point)
    {
        return point.X >= X && point.X < X + Width && point.Y >= Y && point.Y < Y + Height;
    }
}