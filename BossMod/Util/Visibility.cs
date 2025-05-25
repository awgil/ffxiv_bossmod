namespace BossMod;

public static class Visibility
{
    public record struct Segment(WPos A, WPos B)
    {
        public readonly WPos this[int index] => index == 0 ? A : index == 1 ? B : throw new InvalidOperationException($"index {index} out of bounds");
    }
    record struct SegmentPointAngle(int SegmentIndex, int PointIndex, float Angle);

    public record struct Tri(WPos A, WPos B, WPos C);

    private const float Epsilon = 0.0000001f;

    public static List<WPos> Compute(WPos position, List<(WPos start, WPos end)> segments) => Compute(position, segments.Select(s => new Segment(s.start, s.end)).ToList());

    public static List<WPos> Compute(WPos position, List<Segment> segments)
    {
        List<Segment> bounded = [];
        float minX = position.X, minY = position.Z, maxX = position.X, maxY = position.Z;
        for (var i = 0; i < segments.Count; i++)
        {
            for (var j = 0; j < 2; j++)
            {
                minX = MathF.Min(minX, segments[i][j].X);
                minY = MathF.Min(minY, segments[i][j].Z);
                maxX = MathF.Max(maxX, segments[i][j].X);
                maxY = MathF.Max(maxY, segments[i][j].Z);
            }
            bounded.Add(new(segments[i][0], segments[i][1]));
        }
        --minX;
        --minY;
        ++maxX;
        ++maxY;

        bounded.Add(new(new(minX, minY), new(maxX, minY)));
        bounded.Add(new(new(maxX, minY), new(maxX, maxY)));
        bounded.Add(new(new(maxX, maxY), new(minX, maxY)));
        bounded.Add(new(new(minX, maxY), new(minX, minY)));

        var sorted = SortPoints(position, bounded);
        var map = Utils.MakeArray(bounded.Count, -1);

        var heap = new List<int>();
        var start = new WPos(position.X + 1, position.Z);
        for (var i = 0; i < bounded.Count; i++)
        {
            var a1 = Angle_(bounded[i][0], position);
            var a2 = Angle_(bounded[i][1], position);
            var active = (a1 > -180.0 && a1 <= 0.0 && a2 <= 180.0 && a2 >= 0.0 && a2 - a1 > 180.0) ||
                 (a2 > -180.0 && a2 <= 0.0 && a1 <= 180.0 && a1 >= 0.0 && a1 - a2 > 180.0);
            if (active)
                Insert(i, heap, position, bounded, start, map);
        }

        var output = new List<WPos>();

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

        return output;
    }

    private static SegmentPointAngle[] SortPoints(WPos pos, List<Segment> segments)
    {
        var segCount = segments.Count;
        var points = new SegmentPointAngle[segCount * 2];
        for (var i = 0; i < segCount; i++)
        {
            for (var j = 0; j < 2; ++j)
            {
                var a = Angle_(segments[i][j], pos);
                points[2 * i + j] = new(i, j, a);
            }
        }

        points.SortBy(p => p.Angle);
        return points;
    }

    private static float Angle_(WPos a, WPos b) => MathF.Atan2(b.Z - a.Z, b.X - a.X) * 180f / MathF.PI;
    private static float Angle2(WPos a, WPos b, WPos c)
    {
        var a1 = Angle_(a, b);
        var a2 = Angle_(b, c);
        var a3 = a1 - a2;
        if (a3 < 0)
            a3 += 360;
        if (a3 > 360)
            a3 -= 360;
        return a3;
    }

    private static void Insert(int index, List<int> heap, WPos position, List<Segment> segments, WPos destination, int[] map)
    {
        var intersect = IntersectLines(segments[index][0], segments[index][1], position, destination);
        if (intersect == null)
            return;
        var cur = heap.Count;
        heap.Add(index);
        map[index] = cur;
        while (cur > 0)
        {
            var parent = Parent(cur);
            if (!LessThan(heap[cur], heap[parent], position, segments, destination))
                break;
            map[heap[parent]] = cur;
            map[heap[cur]] = parent;
            var temp = heap[cur];
            heap[cur] = heap[parent];
            heap[parent] = temp;
            cur = parent;
        }
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

    private static void Remove(int index, List<int> heap, WPos position, List<Segment> segments, WPos destination, int[] map)
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
                var temp = heap[cur];
                heap[cur] = heap[parent];
                heap[parent] = temp;
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
                    var temp = heap[left];
                    heap[left] = heap[cur];
                    heap[cur] = temp;
                    cur = left;
                }
                else if (right < heap.Count && LessThan(heap[right], heap[cur], position, segments, destination))
                {
                    map[heap[right]] = cur;
                    map[heap[cur]] = right;
                    var temp = heap[right];
                    heap[right] = heap[cur];
                    heap[cur] = temp;
                    cur = right;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private static int Parent(int index) => (index - 1) / 2;
    private static int Child(int index) => 2 * index + 1;

    private static bool LessThan(int index1, int index2, WPos position, List<Segment> segments, WPos destination)
    {
        var inter1Null = IntersectLines(segments[index1][0], segments[index1][1], position, destination);
        var inter2Null = IntersectLines(segments[index2][0], segments[index2][1], position, destination);
        if (inter1Null == null || inter2Null == null)
            return false;

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
            end1 = 1;
        var end2 = 0;
        if (Equals(inter2, segments[index2][0]))
            end2 = 1;

        var a1 = Angle2(segments[index1][end1], inter1, position);
        var a2 = Angle2(segments[index2][end2], inter2, position);
        if (a1 < 180)
        {
            if (a2 > 180)
                return true;
            return a2 < a1;
        }
        return a1 < a2;
    }

    private static bool Equal(WPos a, WPos b) => a.AlmostEqual(b, Epsilon);
    private static float Distance(WPos a, WPos b)
    {
        var dx = a.X - b.X;
        var dy = a.Z - b.Z;
        return dx * dx + dy * dy;
    }

    private static WPos? IntersectLines(WPos a1, WPos a2, WPos b1, WPos b2)
    {
        float dbx = b2.X - b1.X, dby = b2.Z - b1.Z, dax = a2.X - a1.X, day = a2.Z - a1.Z;
        var uB = dby * dax - dbx * day;
        if (uB != 0)
        {
            var ua = (dbx * (a1.Z - b1.Z) - dby * (a1.X - b1.X)) / uB;
            return new(a1.X - ua * -dax, a1.Z - ua * -day);
        }
        return null;
    }
}
