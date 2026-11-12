using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BossMod;

// TODO: replace me with union when it gets implemented
[StructLayout(LayoutKind.Explicit)]
public readonly struct Sdf
{
    public enum SdfType
    {
        Continuous,
        Discrete
    }

    // please bro just give me unions bro please
#nullable disable
    [FieldOffset(0)] private readonly DistFn SdfCont;
    [FieldOffset(0)] private readonly PointTest SdfDisc;
#nullable restore
    [FieldOffset(8)] public readonly SdfType Type;

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

    public static Sdf Continuous(DistFn v) => new(v);
    public static Sdf Discrete(PointTest t) => new(t);

#pragma warning disable IDE0066 // Convert switch statement to expression
    public readonly bool Check(WPos p)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont.CallOnce(p) < 0;

            case SdfType.Discrete:
                return SdfDisc.CallOnce(p);

            default:
                throw new UnreachableException();
        }
    }

    public readonly VFloat Distance(WVec p)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont(p);

            case SdfType.Discrete:
                return VV.ConditionalSelect(SdfDisc(p), VV.Create(float.MinValue), VV.Create(float.MaxValue));

            default:
                throw new UnreachableException();
        }
    }

    public readonly float Distance(WPos p)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont.CallOnce(p);

            case SdfType.Discrete:
                return SdfDisc.CallOnce(p) ? float.MinValue : float.MaxValue;

            default:
                throw new UnreachableException();
        }
    }
#pragma warning restore IDE0066 // Convert switch statement to expression
}
