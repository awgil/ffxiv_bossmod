namespace BossMod;

// clipping shapes to bounds and triangulating them is a serious time sink, so we want to cache that
// to avoid requiring tracking cache lifetime by users, we use a heuristic - we assume that if something isn't drawn for a frame, it's no longer relevant
// for that, we keep two lists (prev/curr frame cache), every frame discard old entries, every time we retrieve entry from prev frame cache we move it to curr
public sealed class TriangulationCache
{
    private List<(int hash, List<RelTriangle>? triangulation)> _prev = [];
    private List<(int hash, List<RelTriangle>? triangulation)> _curr = [];

    // the typical usage is: var triangulation = cache[hash] ??= BuildTriangulation(...)
    public ref List<RelTriangle>? this[int hash]
    {
        get
        {
            var iCurr = _curr.FindIndex(kv => kv.hash == hash);
            if (iCurr < 0)
            {
                List<RelTriangle>? entry = null;

                // see if there is entry in prev
                var iPrev = _prev.FindIndex(kv => kv.hash == hash);
                if (iPrev >= 0)
                {
                    entry = _prev[iPrev].triangulation;
                    // swap-remove
                    if (iPrev + 1 < _prev.Count)
                        _prev[iPrev] = _prev[^1];
                    _prev.RemoveAt(_prev.Count - 1);
                }

                iCurr = _curr.Count;
                _curr.Add((hash, entry));
            }
            return ref _curr.Ref(iCurr).triangulation;
        }
    }

    public void NextFrame()
    {
        (_prev, _curr) = (_curr, _prev);
        _curr.Clear();
    }

    public void Invalidate()
    {
        _prev.Clear();
        _curr.Clear();
    }
}
