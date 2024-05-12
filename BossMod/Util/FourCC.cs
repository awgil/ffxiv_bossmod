using System.Text;

namespace BossMod;

// four character code - used as tags for data serialization
public readonly record struct FourCC(uint Value)
{
    public FourCC(ReadOnlySpan<byte> v) : this(Cast(v)) { }

    public unsafe override string ToString()
    {
        Span<byte> span = stackalloc byte[4];
        fixed (byte* mem = &span[0])
        {
            *(uint*)mem = Value;
            return Encoding.UTF8.GetString(span);
        }
    }

    private static unsafe uint Cast(ReadOnlySpan<byte> span)
    {
        if (span.Length != 4)
            throw new ArgumentException("FourCC should be 4 characters long");
        fixed (byte* mem = &span[0])
            return *(uint*)mem;
    }
}
