namespace BossMod;

// clipping shapes to bounds and triangulating them is a serious time sink, so we want to cache that
// to avoid requiring tracking cache lifetime by users, we use a heuristic - we assume that if something isn't drawn for a frame, it's no longer relevant
[SkipLocalsInit]
public sealed class PolygonCache
{
    private struct CacheEntry
    {
        public RelSimplifiedComplexPolygon? Polygon;
        public int LastSeenFrame;
    }

    private readonly Dictionary<int, CacheEntry> _cache = [];
    private int _frame;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref RelSimplifiedComplexPolygon? GetByHash(int keyHash)
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_cache, keyHash, out var exists);
        if (!exists)
        {
            entry.LastSeenFrame = _frame;
            return ref entry.Polygon;
        }

        entry.LastSeenFrame = _frame;
        return ref entry.Polygon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetKeyHash<T1>(int keyType, T1 p1)
        where T1 : notnull
        => HashCode.Combine(keyType, p1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetKeyHash<T1, T2>(int keyType, T1 p1, T2 p2)
        where T1 : notnull where T2 : notnull
        => HashCode.Combine(keyType, p1, p2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetKeyHash<T1, T2, T3>(int keyType, T1 p1, T2 p2, T3 p3)
        where T1 : notnull where T2 : notnull where T3 : notnull
        => HashCode.Combine(keyType, p1, p2, p3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetKeyHash<T1, T2, T3, T4>(int keyType, T1 p1, T2 p2, T3 p3, T4 p4)
        where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull
        => HashCode.Combine(keyType, p1, p2, p3, p4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetKeyHash<T1, T2, T3, T4, T5>(int keyType, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull where T5 : notnull
        => HashCode.Combine(keyType, p1, p2, p3, p4, p5);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref RelSimplifiedComplexPolygon? Get<T1>(int keyType, T1 p1)
        where T1 : notnull
        => ref GetByHash(GetKeyHash(keyType, p1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref RelSimplifiedComplexPolygon? Get<T1, T2>(int keyType, T1 p1, T2 p2)
        where T1 : notnull where T2 : notnull
        => ref GetByHash(GetKeyHash(keyType, p1, p2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref RelSimplifiedComplexPolygon? Get<T1, T2, T3>(int keyType, T1 p1, T2 p2, T3 p3)
        where T1 : notnull where T2 : notnull where T3 : notnull
        => ref GetByHash(GetKeyHash(keyType, p1, p2, p3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref RelSimplifiedComplexPolygon? Get<T1, T2, T3, T4>(int keyType, T1 p1, T2 p2, T3 p3, T4 p4)
        where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull
        => ref GetByHash(GetKeyHash(keyType, p1, p2, p3, p4));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref RelSimplifiedComplexPolygon? Get<T1, T2, T3, T4, T5>(int keyType, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull where T5 : notnull
        => ref GetByHash(GetKeyHash(keyType, p1, p2, p3, p4, p5));

    public void NextFrame()
    {
        var frameJustRendered = _frame;
        ++_frame;

        List<int> toRemove = [with(_cache.Count)];
        foreach (var kvp in _cache)
        {
            if (kvp.Value.LastSeenFrame != frameJustRendered)
            {
                toRemove.Add(kvp.Key);
            }
        }
        var count = toRemove.Count;
        for (var i = 0; i < count; ++i)
        {
            _cache.Remove(toRemove[i]);
        }
    }

    public void Invalidate() => _cache.Clear();
}
