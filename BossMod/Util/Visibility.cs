namespace BossMod;

// adapted from https://github.com/trylock/visibility
public static class Visibility
{
    public record struct LineSegment(WPos A, WPos B);

    record struct LineSegmentDistCompare(WPos Origin) : IComparer<LineSegment>
    {
        public readonly int Compare(LineSegment x, LineSegment y) => LessThan(x, y) ? -1 : LessThan(y, x) ? 1 : 0;

        private readonly bool LessThan(LineSegment x, LineSegment y)
        {
            var a = x.A;
            var b = x.B;
            var c = y.A;
            var d = y.B;

            if (ComputeOrientation(Origin, a, b) == Orientation.Collinear)
                throw new ArgumentException("AB must not be collinear with origin");
            if (ComputeOrientation(Origin, c, d) == Orientation.Collinear)
                throw new ArgumentException("CD must not be collinear with origin");

            if (ApproxEqual(b, c) || ApproxEqual(b, d))
                (a, b) = (b, a);
            if (ApproxEqual(a, d))
                (c, d) = (d, c);

            if (ApproxEqual(a, c))
            {
                var oad = ComputeOrientation(Origin, a, d);
                var oab = ComputeOrientation(Origin, a, b);
                if (ApproxEqual(b, d) || oad != oab)
                    return false;
                return ComputeOrientation(a, b, d) != ComputeOrientation(a, b, Origin);
            }

            var cda = ComputeOrientation(c, d, a);
            var cdb = ComputeOrientation(c, d, b);
            if (cdb == Orientation.Collinear && cda == Orientation.Collinear)
                return (Origin - a).LengthSq() < (Origin - c).LengthSq();
            else if (cda == cdb || cda == Orientation.Collinear || cdb == Orientation.Collinear)
            {
                var cdo = ComputeOrientation(c, d, Origin);
                return cdo == cda || cdo == cdb;
            }
            else
            {
                var abo = ComputeOrientation(a, b, Origin);
                return abo != ComputeOrientation(a, b, c);
            }
        }
    }

    record struct AngleCompare(WPos Vertex) : IComparer<WPos>
    {
        public readonly int Compare(WPos x, WPos y) => LessThan(x, y) ? -1 : 1;

        private readonly bool LessThan(WPos a, WPos b)
        {
            var is_a_left = StrictlyLess(a.X, Vertex.X);
            var is_b_left = StrictlyLess(b.X, Vertex.X);
            if (is_a_left != is_b_left)
                return is_b_left;

            if (ApproxEqual(a.X, Vertex.X) && ApproxEqual(b.X, Vertex.X))
            {
                if (!StrictlyLess(a.Z, Vertex.Z) || !StrictlyLess(b.Z, Vertex.Z))
                    return StrictlyLess(b.Z, a.Z);
                return StrictlyLess(a.Z, b.Z);
            }

            var oa = a - Vertex;
            var ob = b - Vertex;
            var det = WDir.Cross(oa, ob);
            if (ApproxEqual(det, 0))
                return oa.LengthSq() < ob.LengthSq();
            return det < 0;
        }
    }

    public enum Orientation
    {
        LeftTurn = 1,
        RightTurn = -1,
        Collinear = 0
    }

    public static Orientation ComputeOrientation(WPos a, WPos b, WPos c)
    {
        var det = WDir.Cross(b - a, c - a);
        var a1 = StrictlyLess(0, det) ? 1 : 0;
        var a2 = StrictlyLess(det, 0) ? 1 : 0;
        return (Orientation)(a1 - a2);
    }

    private static bool StrictlyLess(float a, float b) => (b - a) > MathF.Max(MathF.Abs(a), MathF.Abs(b)) * float.Epsilon;
    private static bool ApproxEqual(float a, float b) => MathF.Abs(a - b) <= MathF.Max(MathF.Abs(a), MathF.Abs(b)) * float.Epsilon;
    private static bool ApproxEqual(WPos a, WPos b) => ApproxEqual(a.X, b.X) && ApproxEqual(a.Z, b.Z);

    enum EventType
    {
        Start,
        End
    }

    record struct Vevent(EventType Type, LineSegment Segment)
    {
        public readonly WPos Point => Segment.A;
    }

    public static List<WPos> VisibilityPolygon(WPos point, IEnumerable<LineSegment> obstacles)
    {
        var cmpDist = new LineSegmentDistCompare(point);
        var state = new SortedSet<LineSegment>(cmpDist);
        var events = new List<Vevent>();

        foreach (var segment in obstacles)
        {
            var pab = ComputeOrientation(point, segment.A, segment.B);
            if (pab == Orientation.Collinear)
                continue;
            else if (pab == Orientation.RightTurn)
            {
                events.Add(new(EventType.Start, segment));
                events.Add(new(EventType.End, new(segment.B, segment.A)));
            }
            else
            {
                events.Add(new(EventType.Start, new(segment.B, segment.A)));
                events.Add(new(EventType.End, segment));
            }

            WPos a = segment.A, b = segment.B;
            if (a.X > b.X)
                (a, b) = (b, a);

            var abp = ComputeOrientation(a, b, point);
            if (abp == Orientation.RightTurn && (ApproxEqual(b.X, point.X) || (a.X < point.X && point.X < b.X)))
                state.Add(segment);
        }

        var cmpAngle = new AngleCompare(point);
        events.Sort((a, b) =>
        {
            if (ApproxEqual(a.Point, b.Point))
                return a.Type == EventType.End && b.Type == EventType.Start ? -1 : 1;
            return cmpAngle.Compare(a.Point, b.Point);
        });

        var vertices = new List<WPos>();
        foreach (var evt in events)
        {
            if (evt.Type == EventType.End)
                state.RemoveWhere(s => cmpDist.Compare(s, evt.Segment) == 0);

            if (state.Count == 0)
                vertices.Add(evt.Point);
            else if (cmpDist.Compare(evt.Segment, state.Min!) < 0)
            {
                var ray = new Ray(point, evt.Point - point);
                var nearest_segment = state.Min!;
                var intersects = ray.Intersects(nearest_segment, out var intersection);
                if (!intersects)
                    throw new ArgumentException("ray intersects line segment L iff L is in the state");
                if (evt.Type == EventType.Start)
                {
                    vertices.Add(intersection);
                    vertices.Add(evt.Point);
                }
                else
                {
                    vertices.Add(evt.Point);
                    vertices.Add(intersection);
                }
            }

            if (evt.Type == EventType.Start)
                state.Add(evt.Segment);
        }

        var top = 0;
        for (var it = 0; it < vertices.Count; it++)
        {
            var prev = top == 0 ? vertices.Count - 1 : top - 1;
            var next = it + 1 == vertices.Count ? 0 : it + 1;
            if (ComputeOrientation(vertices[prev], vertices[it], vertices[next]) != Orientation.Collinear)
                vertices[top++] = vertices[it];
        }

        return [.. vertices.Take(top)];
    }

    record struct Ray(WPos Origin, WDir Direction)
    {
        public readonly bool Intersects(LineSegment segment, out WPos outPoint)
        {
            outPoint = default;
            var ao = Origin - segment.A;
            var ab = segment.B - segment.A;
            var det = WDir.Cross(ab, Direction);
            if (ApproxEqual(det, 0))
            {
                var abo = ComputeOrientation(segment.A, segment.B, Origin);
                if (abo != Orientation.Collinear)
                    return false;
                var dist_a = WDir.Dot(ao, Direction);
                var dist_b = WDir.Dot(Origin - segment.B, Direction);
                if (dist_a > 0 && dist_b > 0)
                    return false;
                else if ((dist_a > 0) != (dist_b > 0))
                    outPoint = Origin;
                else if (dist_a > dist_b)
                    outPoint = segment.A;
                else
                    outPoint = segment.B;
                return true;
            }

            var u = WDir.Cross(ao, Direction) / det;
            if (StrictlyLess(u, 0) || StrictlyLess(1, u))
                return false;

            var t = -WDir.Cross(ab, ao) / det;
            outPoint = Origin + t * Direction;
            return ApproxEqual(t, 0) || t > 0;
        }
    }
}
