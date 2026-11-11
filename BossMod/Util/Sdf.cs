using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BossMod;

// TODO: replace me with union when it gets implemented
[StructLayout(LayoutKind.Explicit)]
public readonly struct Sdf
{
    public enum SdfType
    {
        ContinuousSingle,
        DiscreteSingle,
        Indexed,
        Continuous,
        Discrete
    }

    // please bro just give me unions bro please
#nullable disable
    [FieldOffset(0)] private readonly Func<WPos, float> SdfCont1;
    [FieldOffset(0)] private readonly Func<WPos, bool> SdfDisc1;
    [FieldOffset(0)] private readonly Func<WPos, int, bool> SdfIx;
    [FieldOffset(0)] private readonly DistFn SdfCont;
    [FieldOffset(0)] private readonly PointTest SdfDisc;
#nullable restore
    [FieldOffset(8)] public readonly SdfType Type;

    Sdf(Func<WPos, float> d) : this()
    {
        Type = SdfType.ContinuousSingle;
        SdfCont1 = d;
    }
    Sdf(Func<WPos, bool> b) : this()
    {
        Type = SdfType.DiscreteSingle;
        SdfDisc1 = b;
    }
    Sdf(Func<WPos, int, bool> i) : this()
    {
        Type = SdfType.Indexed;
        SdfIx = i;
    }
    Sdf(DistFn dist) : this()
    {
        Type = SdfType.Continuous;
        SdfCont = dist;
    }
    Sdf(PointTest pt) : this()
    {
        Type = SdfType.Discrete;
        SdfDisc = pt;
    }

    public static Sdf ContinuousSingle(Func<WPos, float> d) => new(d);
#if DEBUG
    [Obsolete("Please convert your shit to use the vectored version!")]
#endif
    public static Sdf DiscreteSingle(Func<WPos, bool> b) => new(b);
    public static Sdf Indexed(Func<WPos, int, bool> i) => new(i);
    public static Sdf Continuous(DistFn v) => new(v);
    public static Sdf Discrete(PointTest t) => new(t);

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
