namespace BossMod;

// list of disjoint segments (defined by min/max values)
public class DisjointSegmentList
{
    public List<(float Min, float Max)> Segments { get; private init; } = [];

    public (float Min, float Max) this[Index index] => Segments[index];
    public int Count => Segments.Count;

    public DisjointSegmentList Clone() => new()
    {
        Segments = [.. Segments]
    };

    public void Add(float min, float max)
    {
        var (index, count) = Intersect(min, max);
        if (count == 0)
        {
            // new segment is disjoint with any existing ones, just insert
            Segments.Insert(index, (min, max));
        }
        else
        {
            // merge new and all intersecting into first
            ref var seg = ref Segments.Ref(index);
            seg.Min = Math.Min(min, seg.Min);
            seg.Max = Math.Max(max, Segments[index + count - 1].Max);

            // remove any other intersecting ones
            Segments.RemoveRange(index + 1, count - 1);
        }
    }

    public void Clear() => Segments.Clear();

    public bool Contains(float x)
    {
        var firstIndex = Segments.FindIndex(s => s.Max >= x);
        return firstIndex != -1 && Segments[firstIndex].Min <= x;
    }

    // if there is intersection - return index of the first intersecting segment and number of subsequent intersecting segments (touching by endpoint is considered an intersection)
    // otherwise - return index of the first segment greater than range (i.e. insertion point) and 0
    public (int first, int count) Intersect(float min, float max)
    {
        var index = Segments.FindIndex(s => s.Max >= min);
        if (index < 0)
            return (Segments.Count, 0); // greater than any existing segments

        if (max < Segments[index].Min)
            return (index, 0); // first or middle non-intersecting

        // count intersections
        var last = index + 1;
        while (last < Segments.Count && Segments[last].Min <= max)
            ++last;
        return (index, last - index);
    }
}
