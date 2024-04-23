namespace BossMod;

// 64-bit mask; out-of-range accesses are safe and well-defined ('get' always returns 0, 'set' is no-op)
// this is often used e.g. to store per-player flags (in such case only 8 lowest bits are used for 'normal' raids, or 24 lowest for full alliances)
public record struct BitMask(ulong Raw)
{
    public bool this[int index]
    {
#pragma warning disable IDE0251 // for whatever reason, marking getter as readonly triggers CS0649 when bitmask is modified by a setter
        get => (Raw & MaskForBit(index)) != 0;
#pragma warning restore IDE0251
        set
        {
            if (value)
                Set(index);
            else
                Clear(index);
        }
    }

    public void Reset() => Raw = 0;
    public void Set(int index) => Raw |= MaskForBit(index);
    public void Clear(int index) => Raw &= ~MaskForBit(index);
    public void Toggle(int index) => Raw ^= MaskForBit(index);

    public readonly bool Any() => Raw != 0;
    public readonly bool None() => Raw == 0;
    public readonly int NumSetBits() => BitOperations.PopCount(Raw);
    public readonly int LowestSetBit() => BitOperations.TrailingZeroCount(Raw); // returns out-of-range value (64) if no bits are set
    public readonly int HighestSetBit() => 63 - BitOperations.LeadingZeroCount(Raw); // returns out-of-range value (-1) if no bits are set

    public readonly BitMask WithBit(int index) => new(Raw | MaskForBit(index));
    public readonly BitMask WithoutBit(int index) => new(Raw & ~MaskForBit(index));

    public static BitMask operator ~(BitMask a) => new(~a.Raw);
    public static BitMask operator &(BitMask a, BitMask b) => new(a.Raw & b.Raw);
    public static BitMask operator |(BitMask a, BitMask b) => new(a.Raw | b.Raw);
    public static BitMask operator ^(BitMask a, BitMask b) => new(a.Raw ^ b.Raw);

    public readonly IEnumerable<int> SetBits()
    {
        ulong v = Raw;
        while (v != 0)
        {
            int index = BitOperations.TrailingZeroCount(v);
            yield return index;
            v &= ~(1ul << index);
        }
    }

    private static ulong MaskForBit(int index) => (uint)index < 64 ? (1ul << index) : 0;

    public override readonly string ToString() => $"{Raw:X}";
}
