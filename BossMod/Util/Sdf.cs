using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BossMod;

// TODO: replace me with union when it gets implemented
[StructLayout(LayoutKind.Explicit)]
public struct Sdf
{
    public enum SdfType
    {
        ContinuousSingle,
        DiscreteSingle,
        Indexed,
        Continuous,
        Discrete
    }

    [FieldOffset(0)] private Func<WPos, float> SdfCont1;
    [FieldOffset(0)] private Func<WPos, bool> SdfDisc1;
    [FieldOffset(0)] private Func<WPos, int, bool> SdfIx;
    [FieldOffset(0)] private ShapeDist SdfCont;
    [FieldOffset(0)] private ShapeTest SdfDisc;
    [FieldOffset(8)] public SdfType Type;

    public static Sdf ContinuousSingle(Func<WPos, float> d) => new() { Type = SdfType.ContinuousSingle, SdfCont1 = d };
#if DEBUG
    [Obsolete("Please convert your shit to use the vectored version!")]
#endif
    public static Sdf DiscreteSingle(Func<WPos, bool> b) => new() { Type = SdfType.DiscreteSingle, SdfDisc1 = b };
    public static Sdf Indexed(Func<WPos, int, bool> i) => new() { Type = SdfType.Indexed, SdfIx = i };
    public static Sdf Continuous(ShapeDist v) => new() { Type = SdfType.Continuous, SdfCont = v };
    public static Sdf Discrete(ShapeTest t) => new() { Type = SdfType.Discrete, SdfDisc = t };

#pragma warning disable IDE0066 // Convert switch statement to expression
    public readonly bool Check(WPos p)
    {
        switch (Type)
        {
            case SdfType.ContinuousSingle:
                return SdfCont1(p) < 0;

            case SdfType.DiscreteSingle:
                return SdfDisc1(p);

            case SdfType.Indexed:
                return SdfIx(p, -1);

            case SdfType.Continuous:
                return SdfCont(new(p))[0] < 0;

            case SdfType.Discrete:
                return SdfDisc(new(p))[0] != 0;

            default:
                throw new UnreachableException();
        }
    }

    public readonly VFloat Distance(WVec p)
    {
        if (Type == SdfType.Continuous)
            return SdfCont(p);

        if (Type == SdfType.Discrete)
            return Vector.ConditionalSelect(SdfDisc(p), VV.Create(float.MinValue), VV.Create(float.MaxValue));

        var xa = new float[VFloat.Count];
        var xb = new float[VFloat.Count];
        p.X.CopyTo(xa);
        p.Z.CopyTo(xb);

        var d = new float[VFloat.Count];
        var s = this;

        Parallel.For(0, VFloat.Count, i =>
        {
            d[i] = s.Distance(new WPos(xa[i], xb[i]));
        });

        return VV.Create(d.AsSpan());
    }

    public readonly float Distance(WPos p)
    {
        switch (Type)
        {
            case SdfType.ContinuousSingle:
                return SdfCont1(p);

            case SdfType.DiscreteSingle:
                return SdfDisc1(p) ? float.MinValue : float.MaxValue;

            case SdfType.Indexed:
                return SdfIx(p, -1) ? float.MinValue : float.MaxValue;

            case SdfType.Continuous:
                return SdfCont(new(p))[0];

            case SdfType.Discrete:
                return SdfDisc(new(p))[0] != 0 ? float.MinValue : float.MaxValue;

            default:
                throw new UnreachableException();
        }
    }

    public readonly bool CheckIndexed(WPos p, int index)
    {
        return Type == SdfType.Indexed ? SdfIx(p, index) : Check(p);
    }
#pragma warning restore IDE0066 // Convert switch statement to expression
}
