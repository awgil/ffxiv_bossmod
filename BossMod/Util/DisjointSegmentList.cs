namespace BossMod;

// list of disjoint segments (defined by min/max values)
public class DisjointSegmentList
{
    private readonly List<(float Min, float Max)> _segments = [];
    public IReadOnlyList<(float Min, float Max)> Segments => _segments;

    public (float Min, float Max) this[Index index] => _segments[index];
    public int Count => _segments.Count;

    public void Add(float min, float max)
    {
        var (index, count) = Intersect(min, max);
        if (count == 0)
        {
            // new segment is disjoint with any existing ones, just insert
            _segments.Insert(index, (min, max));
        }
        else
        {
            // merge new and all intersecting into first
            ref var seg = ref _segments.Ref(index);
            seg.Min = Math.Min(min, seg.Min);
            seg.Max = Math.Max(max, _segments[index + count - 1].Max);

            // remove any other intersecting ones
            _segments.RemoveRange(index + 1, count - 1);
        }
    }

    public void Clear() => _segments.Clear();

    public bool Contains(float x)
    {
        var firstIndex = _segments.FindIndex(s => s.Max >= x);
        return firstIndex != -1 && _segments[firstIndex].Min <= x;
    }

    // if there is intersection - return index of the first intersecting segment and number of subsequent intersecting segments (touching by endpoint is considered an intersection)
    // otherwise - return index of the first segment greater than range (i.e. insertion point) and 0
    public (int first, int count) Intersect(float min, float max)
    {
        var index = _segments.FindIndex(s => s.Max >= min);
        if (index < 0)
            return (_segments.Count, 0); // greater than any existing segments

        if (max < _segments[index].Min)
            return (index, 0); // first or middle non-intersecting

        // count intersections
        var last = index + 1;
        while (last < _segments.Count && _segments[last].Min <= max)
            ++last;
        return (index, last - index);
    }
}
