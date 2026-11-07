namespace BossMod;

public static class VisibilityAdapters
{
    public static readonly VisibilityPolygon<WPos>.PointAdapter WPos = new(
        (a, b) => new((float)a, (float)b),
        p => (double)p.X,
        p => (double)p.Z
    );

    public static readonly VisibilityPolygon<Vector3>.PointAdapter Vec3 = new(
        (x, y) => new((float)x, 0, (float)y),
        p => p.X,
        p => p.Z
    );
}

// modernized from https://github.com/xaviergonz/visibility-polygon-csharp
public class VisibilityPolygon<TPoint>(VisibilityPolygon<TPoint>.PointAdapter adapter) where TPoint : struct
{
    public record struct Segment(TPoint P1, TPoint P2)
    {
        public readonly TPoint this[int index] => index == 0 ? P1 : P2;
    }

    public record struct PointAdapter(
        Func<double, double, TPoint> Create,
        Func<TPoint, double> GetX,
        Func<TPoint, double> GetY
    );

    private const double Epsilon = 0.0000001;

    private TPoint NewPoint(double x, double y)
    {
        return adapter.Create(x, y);
    }

    private double X(TPoint point)
    {
        return adapter.GetX(point);
    }

    private double Y(TPoint point)
    {
        return adapter.GetY(point);
    }

    public void ComputeWithOutput(TPoint position, List<Segment> segments, List<TPoint> output, bool clearOutput = true)
    {
        if (clearOutput)
            output.Clear();

        var bounded = new List<Segment>();
        var minX = X(position);
        var minY = Y(position);
        var maxX = X(position);
        var maxY = Y(position);
        for (var i = 0; i < segments.Count; ++i)
        {
            for (var j = 0; j < 2; ++j)
            {
                minX = Math.Min(minX, X(segments[i][j]));
                minY = Math.Min(minY, Y(segments[i][j]));
                maxX = Math.Max(maxX, X(segments[i][j]));
                maxY = Math.Max(maxY, Y(segments[i][j]));
            }
            //bounded.push([[segments[i][0][0], segments[i][0][1]], [segments[i][1][0], segments[i][1][1]]]);
            bounded.Add(new Segment(segments[i][0], segments[i][1]));
        }
        --minX;
        --minY;
        ++maxX;
        ++maxY;

        bounded.Add(new Segment(NewPoint(minX, minY), NewPoint(maxX, minY)));
        bounded.Add(new Segment(NewPoint(maxX, minY), NewPoint(maxX, maxY)));
        bounded.Add(new Segment(NewPoint(maxX, maxY), NewPoint(minX, maxY)));
        bounded.Add(new Segment(NewPoint(minX, maxY), NewPoint(minX, minY)));

        //var polygon = new List<TPoint>();
        var sorted = SortPoints(position, bounded);
        var map = new int[bounded.Count];
        for (var i = 0; i < map.Length; ++i)
        {
            map[i] = -1;
        }

        var heap = new List<int>();
        var start = NewPoint(X(position) + 1, Y(position));
        for (var i = 0; i < bounded.Count; ++i)
        {
            var a1 = Angle(bounded[i][0], position);
            var a2 = Angle(bounded[i][1], position);
            var active = (a1 > -180.0 && a1 <= 0.0 && a2 <= 180.0 && a2 >= 0.0 && a2 - a1 > 180.0) ||
                         (a2 > -180.0 && a2 <= 0.0 && a1 <= 180.0 && a1 >= 0.0 && a1 - a2 > 180.0);
            if (active)
            {
                Insert(i, heap, position, bounded, start, map);
            }
        }

        for (var i = 0; i < sorted.Length;)
        {
            var extend = false;
            var shorten = false;
            var orig = i;
            var vertex = bounded[sorted[i].SegmentIndex][sorted[i].PointIndex];
            var oldSegment = heap[0];
            do
            {
                if (map[sorted[i].SegmentIndex] != -1)
                {
                    if (sorted[i].SegmentIndex == oldSegment)
                    {
                        extend = true;
                        vertex = bounded[sorted[i].SegmentIndex][sorted[i].PointIndex];
                    }
                    Remove(map[sorted[i].SegmentIndex], heap, position, bounded, vertex, map);
                }
                else
                {
                    Insert(sorted[i].SegmentIndex, heap, position, bounded, vertex, map);
                    if (heap[0] != oldSegment)
                    {
                        shorten = true;
                    }
                }
                ++i;
                if (i >= sorted.Length)
                {
                    break;
                }
            } while (sorted[i].Angle < sorted[orig].Angle + Epsilon);

            if (extend)
            {
                output.Add(vertex);
                var cur = IntersectLines(bounded[heap[0]][0], bounded[heap[0]][1], position, vertex);
                if (cur.HasValue && !Equal(cur.Value, vertex))
                {
                    output.Add(cur.Value);
                }
            }
            else if (shorten)
            {
                var add1 = IntersectLines(bounded[oldSegment][0], bounded[oldSegment][1], position, vertex);
                if (add1.HasValue)
                {
                    output.Add(add1.Value);
                }
                var add2 = IntersectLines(bounded[heap[0]][0], bounded[heap[0]][1], position, vertex);
                if (add2.HasValue)
                {
                    output.Add(add2.Value);
                }
            }
        }
        //return polygon;
    }

    public List<TPoint> Compute(TPoint position, List<Segment> segments)
    {
        var output = new List<TPoint>();
        ComputeWithOutput(position, segments, output, false);
        return output;
    }

    public void ComputeViewportWithOutput(TPoint position, List<Segment> segments, TPoint viewportMinCorner, TPoint viewportMaxCorner, List<TPoint> output, bool clearOutput = true)
    {
        var brokenSegments = new List<Segment>();
        List<TPoint> viewport = [viewportMinCorner, NewPoint(X(viewportMaxCorner), Y(viewportMinCorner)), viewportMaxCorner, NewPoint(X(viewportMinCorner), Y(viewportMaxCorner))];
        for (var i = 0; i < segments.Count; ++i)
        {
            if (X(segments[i][0]) < X(viewportMinCorner) && X(segments[i][1]) < X(viewportMinCorner))
                continue;
            if (Y(segments[i][0]) < Y(viewportMinCorner) && Y(segments[i][1]) < Y(viewportMinCorner))
                continue;
            if (X(segments[i][0]) > X(viewportMaxCorner) && X(segments[i][1]) > X(viewportMaxCorner))
                continue;
            if (Y(segments[i][0]) > Y(viewportMaxCorner) && Y(segments[i][1]) > Y(viewportMaxCorner))
                continue;

            var intersections = new List<TPoint>();
            for (var j = 0; j < viewport.Count; ++j)
            {
                var k = j + 1;
                if (k == viewport.Count)
                {
                    k = 0;
                }

                if (DoLineSegmentsIntersect(
                  X(segments[i][0]),
                  Y(segments[i][0]),
                  X(segments[i][1]),
                  Y(segments[i][1]),
                  X(viewport[j]),
                  Y(viewport[j]),
                  X(viewport[k]),
                  Y(viewport[k])))
                {
                    var intersect = IntersectLines(segments[i][0], segments[i][1], viewport[j], viewport[k]);
                    if (intersect == null)
                    {
                        continue;
                    }
                    if (Equal(intersect, segments[i][0]) || Equal(intersect, segments[i][1]))
                    {
                        continue;
                    }
                    intersections.Add(intersect.Value);
                }
            }

            //var start = NewPoint(segments[i][0].X, segments[i][0].Y);
            var start = segments[i][0];
            while (intersections.Count > 0)
            {
                var endIndex = 0;
                var endDis = Distance(start, intersections[0]);
                for (var j = 1; j < intersections.Count; ++j)
                {
                    var dis = Distance(start, intersections[j]);
                    if (dis < endDis)
                    {
                        endDis = dis;
                        endIndex = j;
                    }
                }
                brokenSegments.Add(new Segment(start, intersections[endIndex]));
                //start.X = intersections[endIndex].X;
                //start.Y = intersections[endIndex].Y;
                start = intersections[endIndex];
                //intersections.splice(endIndex, 1);
                Splice(intersections, endIndex, 1);
            }
            //brokenSegments.push([start, [segments[i][1][0], segments[i][1][1]]]);
            brokenSegments.Add(new Segment(start, segments[i][1]));
        }

        var viewportSegments = new List<Segment>();
        for (var i = 0; i < brokenSegments.Count; ++i)
        {
            if (InViewport(brokenSegments[i][0], viewportMinCorner, viewportMaxCorner) &&
                InViewport(brokenSegments[i][1], viewportMinCorner, viewportMaxCorner))
            {
                viewportSegments.Add(new Segment(brokenSegments[i][0], brokenSegments[i][1]));
            }
        }
        var eps = Epsilon * 10.0;
        viewportSegments.Add(new Segment(
          NewPoint(X(viewportMinCorner) - eps, Y(viewportMinCorner) - eps),
          NewPoint(X(viewportMaxCorner) + eps, Y(viewportMinCorner) - eps)
          ));
        viewportSegments.Add(new Segment(
          NewPoint(X(viewportMaxCorner) + eps, Y(viewportMinCorner) - eps),
          NewPoint(X(viewportMaxCorner) + eps, Y(viewportMaxCorner) + eps))
          );
        viewportSegments.Add(new Segment(
          NewPoint(X(viewportMaxCorner) + eps, Y(viewportMaxCorner) + eps),
          NewPoint(X(viewportMinCorner) - eps, Y(viewportMaxCorner) + eps)
          ));
        viewportSegments.Add(new Segment(
          NewPoint(X(viewportMinCorner) - eps, Y(viewportMaxCorner) + eps),
          NewPoint(X(viewportMinCorner) - eps, Y(viewportMinCorner) - eps)
          ));

        ComputeWithOutput(position, viewportSegments, output, clearOutput);
    }

    public List<TPoint> ComputeViewport(TPoint position, List<Segment> segments, TPoint viewportMinCorner, TPoint viewportMaxCorner)
    {
        var output = new List<TPoint>();
        ComputeViewportWithOutput(position, segments, viewportMinCorner, viewportMaxCorner, output, false);
        return output;
    }

    public bool InViewport(TPoint position, TPoint viewportMinCorner, TPoint viewportMaxCorner)
    {
        if (X(position) < X(viewportMinCorner) - Epsilon)
        {
            return false;
        }
        if (Y(position) < Y(viewportMinCorner) - Epsilon)
        {
            return false;
        }
        if (X(position) > X(viewportMaxCorner) + Epsilon)
        {
            return false;
        }
        if (Y(position) > Y(viewportMaxCorner) + Epsilon)
        {
            return false;
        }
        return true;
    }

    private TPoint? IntersectLines(TPoint a1, TPoint a2, TPoint b1, TPoint b2)
    {
        /*
        double uaT = (b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X);
        //var ubT = (a2.X - a1.X) * (a1.Y - b1.Y) - (a2.Y - a1.Y) * (a1.X - b1.X);
        double uB = (b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y);

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (uB != 0.0) {
          double ua = uaT / uB;
          //var ub = ubT / uB;
          return NewPoint(a1.X - ua * (a1.X - a2.X), a1.Y - ua * (a1.Y - a2.Y));
        }
        return null;
        */

        // optimized version
        double dbx = X(b2) - X(b1), dby = Y(b2) - Y(b1), dax = X(a2) - X(a1), day = Y(a2) - Y(a1);
        var uB = dby * dax - dbx * day;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (uB != 0.0)
        {
            var ua = (dbx * (Y(a1) - Y(b1)) - dby * (X(a1) - X(b1))) / uB;
            return NewPoint(X(a1) - ua * -dax, Y(a1) - ua * -day);
        }
        return null;
    }

    private double Distance(TPoint a, TPoint b)
    {
        var dx = X(a) - X(b);
        var dy = Y(a) - Y(b);
        return dx * dx + dy * dy;
    }

    private static bool IsOnSegment(double xi, double yi, double xj, double yj, double xk, double yk)
    {
        return (xi <= xk || xj <= xk) && (xk <= xi || xk <= xj) &&
               (yi <= yk || yj <= yk) && (yk <= yi || yk <= yj);
    }

    private static int ComputeDirection(double xi, double yi, double xj, double yj, double xk, double yk)
    {
        var a = (xk - xi) * (yj - yi);
        var b = (xj - xi) * (yk - yi);
        return a < b ? -1 : a > b ? 1 : 0;
    }

    private static bool DoLineSegmentsIntersect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
    {
        var d1 = ComputeDirection(x3, y3, x4, y4, x1, y1);
        var d2 = ComputeDirection(x3, y3, x4, y4, x2, y2);
        var d3 = ComputeDirection(x1, y1, x2, y2, x3, y3);
        var d4 = ComputeDirection(x1, y1, x2, y2, x4, y4);
        return (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0))) ||
               (d1 == 0 && IsOnSegment(x3, y3, x4, y4, x1, y1)) ||
               (d2 == 0 && IsOnSegment(x3, y3, x4, y4, x2, y2)) ||
               (d3 == 0 && IsOnSegment(x1, y1, x2, y2, x3, y3)) ||
               (d4 == 0 && IsOnSegment(x1, y1, x2, y2, x4, y4));
    }

    private static int Parent(int index)
    {
        //return Math.Floor((index - 1) / 2);      
        return (index - 1) / 2;
    }

    private static int Child(int index)
    {
        return 2 * index + 1;
    }

    private double Angle2(TPoint a, TPoint b, TPoint c)
    {
        var a1 = Angle(a, b);
        var a2 = Angle(b, c);
        var a3 = a1 - a2;
        if (a3 < 0.0)
        {
            a3 += 360.0;
        }
        if (a3 > 360.0)
        {
            a3 -= 360.0;
        }
        //a3 %= 360.0; // NOT equivalent
        return a3;
    }

    private SegmentPointAngle[] SortPoints(TPoint position, List<Segment> segments)
    {
        var segCount = segments.Count;
        var points = new SegmentPointAngle[segCount * 2];
        for (var i = 0; i < segCount; ++i)
        {
            for (var j = 0; j < 2; ++j)
            {
                var a = Angle(segments[i][j], position);
                points[2 * i + j] = new SegmentPointAngle(i, j, a);
            }
        }

        // sort by angle
        //points.sort(function(a,b) {return a[2]-b[2];});
        Array.Sort(points, (a, b) => a.Angle.CompareTo(b.Angle));

        return points;
    }

    private double Angle(TPoint a, TPoint b)
    {
        return Math.Atan2(Y(b) - Y(a), X(b) - X(a)) * 180.0 / Math.PI;
    }

    private bool Equal(TPoint? aNull, TPoint? bNull)
    {
        // fix
        if (!aNull.HasValue || !bNull.HasValue)
        {
            return false;
        }

        var a = aNull.Value;
        var b = bNull.Value;
        // end of fix

        return Math.Abs(X(a) - X(b)) < Epsilon && Math.Abs(Y(a) - Y(b)) < Epsilon;
    }

    private bool LessThan(int index1, int index2, TPoint position, List<Segment> segments,
      TPoint destination)
    {
        var inter1Null = IntersectLines(segments[index1][0], segments[index1][1], position, destination);
        var inter2Null = IntersectLines(segments[index2][0], segments[index2][1], position, destination);

        // fix
        if (!inter1Null.HasValue || !inter2Null.HasValue)
        {
            return false;
        }
        // end of fix

        var inter1 = inter1Null.Value;
        var inter2 = inter2Null.Value;
        if (!Equal(inter1, inter2))
        {
            var d1 = Distance(inter1, position);
            var d2 = Distance(inter2, position);
            return d1 < d2;
        }

        var end1 = 0;
        if (Equal(inter1, segments[index1][0]))
        {
            end1 = 1;
        }

        var end2 = 0;
        if (Equal(inter2, segments[index2][0]))
        {
            end2 = 1;
        }

        var a1 = Angle2(segments[index1][end1], inter1, position);
        var a2 = Angle2(segments[index2][end2], inter2, position);
        if (a1 < 180.0)
        {
            if (a2 > 180.0)
            {
                return true;
            }
            return a2 < a1;
        }
        return a1 < a2;
    }

    public static void ConvertToSegmentsWithOutput(List<List<TPoint>> polygons, List<Segment> output, bool clearOutput = true)
    {
        if (clearOutput)
            output.Clear();

        //var segments = new List<Segment>();
        var segments = output;
        foreach (var polygon in polygons)
        {
            for (var j = 0; j < polygon.Count; ++j)
            {
                var k = j + 1;
                if (k == polygon.Count)
                {
                    k = 0;
                }

                // segments.push([[polygons[i][j][0], polygons[i][j][1]], [polygons[i][k][0], polygons[i][k][1]]]);
                segments.Add(new Segment(polygon[j], polygon[k]));
            }
        }
        //return segments;
    }

    public static List<Segment> ConvertToSegments(List<List<TPoint>> polygons)
    {
        var output = new List<Segment>();
        ConvertToSegmentsWithOutput(polygons, output, false);
        return output;
    }

    public bool InPolygon(TPoint position, List<TPoint> polygon)
    {
        var val = X(polygon[0]);
        foreach (var point in polygon)
        {
            val = Math.Min(X(point), val);
            val = Math.Min(Y(point), val);
        }

        var edge = NewPoint(val - 1, val - 1);
        var parity = 0;
        for (var i = 0; i < polygon.Count; ++i)
        {
            var j = i + 1;
            if (j == polygon.Count)
            {
                j = 0;
            }
            if (DoLineSegmentsIntersect(X(edge), Y(edge), X(position), Y(position), X(polygon[i]), Y(polygon[i]), X(polygon[j]),
              Y(polygon[j])))
            {
                // intersect should have a value if we got here
                var intersect = IntersectLines(edge, position, polygon[i], polygon[j]);
                if (Equal(position, intersect))
                {
                    return true;
                }
                if (Equal(intersect, polygon[i]))
                {
                    if (Angle2(position, edge, polygon[j]) < 180.0)
                    {
                        ++parity;
                    }
                }
                else if (Equal(intersect, polygon[j]))
                {
                    if (Angle2(position, edge, polygon[i]) < 180.0)
                    {
                        ++parity;
                    }
                }
                else
                {
                    ++parity;
                }
            }
        }
        return parity % 2 != 0;
    }

    private static void Splice<T>(List<T> source, int index, int count)
    {
        source.RemoveRange(index, count);
    }

    public void BreakIntersectionsWithOutput(List<Segment> segments, List<Segment> output, bool clearOutput = true)
    {
        if (clearOutput)
            output.Clear();

        for (var i = 0; i < segments.Count; ++i)
        {
            var intersections = new List<TPoint>();
            for (var j = 0; j < segments.Count; ++j)
            {
                if (i == j)
                {
                    continue;
                }
                if (DoLineSegmentsIntersect(X(segments[i][0]), Y(segments[i][0]), X(segments[i][1]), Y(segments[i][1]),
                  X(segments[j][0]), Y(segments[j][0]), X(segments[j][1]), Y(segments[j][1])))
                {
                    var intersect = IntersectLines(segments[i][0], segments[i][1], segments[j][0], segments[j][1]);
                    if (intersect == null)
                    {
                        continue;
                    }
                    if (Equal(intersect.Value, segments[i][0]) || Equal(intersect.Value, segments[i][1]))
                    {
                        continue;
                    }
                    intersections.Add(intersect.Value);
                }
            }

            //TPoint start = NewPoint(segments[i][0].X, segments[i][0].Y);
            var start = segments[i][0];
            while (intersections.Count > 0)
            {
                var endIndex = 0;
                var endDis = Distance(start, intersections[0]);
                for (var j = 1; j < intersections.Count; ++j)
                {
                    var dis = Distance(start, intersections[j]);
                    if (dis < endDis)
                    {
                        endDis = dis;
                        endIndex = j;
                    }
                }
                output.Add(new Segment(start, intersections[endIndex]));
                start = intersections[endIndex];
                //intersections.splice(endIndex, 1);
                Splice(intersections, endIndex, 1);
            }
            // output.push([start, [segments[i][1][0], segments[i][1][1]]]);
            output.Add(new Segment(start, segments[i][1]));
        }
    }

    public List<Segment> BreakIntersections(List<Segment> segments)
    {
        var output = new List<Segment>();
        BreakIntersectionsWithOutput(segments, output, false);
        return output;
    }

    private static int Pop(List<int> heap)
    {
        if (heap.Count > 0)
        {
            var index = heap.Count - 1;
            var val = heap[index];
            heap.RemoveAt(index);
            return val;
        }
        //throw new InvalidOperationException();
        return 0;
    }

    private void Remove(int index, List<int> heap, TPoint position, List<Segment> segments, TPoint destination, int[] map)
    {
        map[heap[index]] = -1;
        if (index == heap.Count - 1)
        {
            Pop(heap);
            return;
        }
        heap[index] = Pop(heap);
        map[heap[index]] = index;
        var cur = index;
        var parent = Parent(cur);
        if (cur != 0 && LessThan(heap[cur], heap[parent], position, segments, destination))
        {
            while (cur > 0)
            {
                parent = Parent(cur);
                if (!LessThan(heap[cur], heap[parent], position, segments, destination))
                {
                    break;
                }
                map[heap[parent]] = cur;
                map[heap[cur]] = parent;
                (heap[parent], heap[cur]) = (heap[cur], heap[parent]);
                cur = parent;
            }
        }
        else
        {
            while (true)
            {
                var left = Child(cur);
                var right = left + 1;
                if (left < heap.Count && LessThan(heap[left], heap[cur], position, segments, destination) &&
                    (right == heap.Count || LessThan(heap[left], heap[right], position, segments, destination)))
                {
                    map[heap[left]] = cur;
                    map[heap[cur]] = left;
                    (heap[cur], heap[left]) = (heap[left], heap[cur]);
                    cur = left;
                }
                else if (right < heap.Count && LessThan(heap[right], heap[cur], position, segments, destination))
                {
                    map[heap[right]] = cur;
                    map[heap[cur]] = right;
                    (heap[cur], heap[right]) = (heap[right], heap[cur]);
                    cur = right;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private void Insert(int index, List<int> heap, TPoint position, List<Segment> segments, TPoint destination, int[] map)
    {
        var intersect = IntersectLines(segments[index][0], segments[index][1], position, destination);
        if (intersect == null)
        {
            return;
        }
        var cur = heap.Count;
        heap.Add(index);
        map[index] = cur;
        while (cur > 0)
        {
            var parent = Parent(cur);
            if (!LessThan(heap[cur], heap[parent], position, segments, destination))
            {
                break;
            }
            map[heap[parent]] = cur;
            map[heap[cur]] = parent;
            (heap[parent], heap[cur]) = (heap[cur], heap[parent]);
            cur = parent;
        }
    }

    record struct SegmentPointAngle(int SegmentIndex, int PointIndex, double Angle);
}

