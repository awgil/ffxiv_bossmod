namespace BossMod;

// clipping shapes to bounds and triangulating them is a serious time sink, so we want to cache that
// to avoid requiring tracking cache lifetime by users, we use a heuristic - we assume that if something isn't drawn for a frame, it's no longer relevant
// for that, we keep two lists (prev/curr frame cache), every frame discard old entries, every time we retrieve entry from prev frame cache we move it to curr
public sealed class TriangulationCache
{
    private List<(object key, List<RelTriangle>? triangulation)> _prev = [];
    private List<(object key, List<RelTriangle>? triangulation)> _curr = [];
    private int _numRequests;
    private int _numReuse;

    // the typical usage is: var triangulation = cache[hash] ??= BuildTriangulation(...)
    public ref List<RelTriangle>? this[object key]
    {
        get
        {
            ++_numRequests;
            var iCurr = _curr.FindIndex(kv => kv.key.Equals(key));
            if (iCurr < 0)
            {
                List<RelTriangle>? entry = null;

                // see if there is entry in prev
                var iPrev = _prev.FindIndex(kv => kv.key.Equals(key));
                if (iPrev >= 0)
                {
                    ++_numReuse;
                    entry = _prev[iPrev].triangulation;
                    // swap-remove
                    if (iPrev + 1 < _prev.Count)
                        _prev[iPrev] = _prev[^1];
                    _prev.RemoveAt(_prev.Count - 1);
                }

                iCurr = _curr.Count;
                _curr.Add((key, entry));
            }
            return ref _curr.Ref(iCurr).triangulation;
        }
    }

    public void NextFrame()
    {
        (_prev, _curr) = (_curr, _prev);
        _curr.Clear();
        _numRequests = _numReuse = 0;
    }

    public void Invalidate()
    {
        _prev.Clear();
        _curr.Clear();
        _numRequests = _numReuse = 0;
    }

    public string Stats() => $"Evict={_prev.Count}, Reuse={_numReuse}, Reqs={_numRequests} ({_curr.Count} unique)";
}
