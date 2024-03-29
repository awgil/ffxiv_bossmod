namespace BossMod;

// list of disjoint segments (defined by min/max values)
public class DisjointSegmentList
{
    private List<(float Min, float Max)> _segments = new();
    public IReadOnlyList<(float Min, float Max)> Segments => _segments;

    public void Add(float min, float max)
    {
        // find position of new (or modified) segment - skip segments that don't intersect (their max < new min)
        var index = _segments.FindIndex(s => s.Max >= min);
        if (index == -1)
        {
            // new segment is to be the last one
            _segments.Add((min, max));
            return;
        }
        var s = _segments[index];
        if (max < s.Min)
        {
            // new segment is disjoint with any existing ones, just insert
            _segments.Insert(index, (min, max));
            return;
        }

        // new segment intersects with segment at 'index' - merge them
        min = Math.Min(min, s.Min);
        max = Math.Max(max, s.Max);

        // find first segment that won't intersect merged one
        if (index + 1 < _segments.Count)
        {
            var nextIndex = _segments.FindIndex(index + 1, s => s.Min > max);
            if (nextIndex == -1)
                nextIndex = _segments.Count;
            max = Math.Max(max, _segments[nextIndex - 1].Max);
            _segments.RemoveRange(index + 1, nextIndex - (index + 1));
        }

        // update merged segment
        _segments[index] = (min, max);
    }

    public void Clear() => _segments.Clear();

    public bool Contains(float x)
    {
        var firstIndex = _segments.FindIndex(s => s.Max >= x);
        return firstIndex != -1 && _segments[firstIndex].Min <= x;
    }
}
