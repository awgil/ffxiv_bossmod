using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Threading.Tasks;

namespace BossMod;

// TODO: replace me with union when it gets implemented
[StructLayout(LayoutKind.Explicit)]
public struct Sdf
{
    public enum SdfType
    {
        Continuous,
        Discrete,
        Indexed,
        Vectored
    }

    [FieldOffset(0)] private Func<WPos, float> SdfCont;
    [FieldOffset(0)] private Func<WPos, bool> SdfDisc;
    [FieldOffset(0)] private Func<WPos, int, bool> SdfIx;
    [FieldOffset(0)] private ShapeFun SdfVectored;
    [FieldOffset(8)] public SdfType Type;

    public static Sdf Continuous(Func<WPos, float> d) => new() { Type = SdfType.Continuous, SdfCont = d };
    public static Sdf Discrete(Func<WPos, bool> b) => new() { Type = SdfType.Discrete, SdfDisc = b };
    public static Sdf Indexed(Func<WPos, int, bool> i) => new() { Type = SdfType.Indexed, SdfIx = i };
    public static Sdf Vectored(ShapeFun v) => new() { Type = SdfType.Vectored, SdfVectored = v };

    public readonly Sdf Inverted()
    {
        switch (Type)
        {
            case SdfType.Continuous:
                var fc = SdfCont;
                return Continuous(p => -fc(p));
            case SdfType.Discrete:
                var fd = SdfDisc;
                return Discrete(p => !fd(p));
            case SdfType.Indexed:
                var fi = SdfIx;
                return Indexed((p, i) => !fi(p, i));
            case SdfType.Vectored:
                var fv = SdfVectored;
                return Vectored(p => -fv(p));
            default:
                throw new UnreachableException();
        }
    }

#pragma warning disable IDE0066 // Convert switch statement to expression
    public readonly bool Check(WPos p)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont(p) < 0;

            case SdfType.Discrete:
                return SdfDisc(p);

            case SdfType.Indexed:
                return SdfIx(p, -1);

            case SdfType.Vectored:
                return SdfVectored(new(p))[0] < 0;

            default:
                throw new UnreachableException();
        }
    }

    public readonly V16 DistanceV(WVec p)
    {
        if (Type == SdfType.Vectored)
            return SdfVectored(p);

        var xa = new float[16];
        var xb = new float[16];
        p.X.CopyTo(xa);
        p.Z.CopyTo(xb);

        var d = new float[16];
        var s = this;

        Parallel.For(0, 16, i =>
        {
            d[i] = s.Distance(new(xa[i], xb[i]));
        });

        return Vector512.Create(d);
    }

    public readonly float Distance(WPos p)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont(p);

            case SdfType.Discrete:
                return SdfDisc(p) ? float.MinValue : float.MaxValue;

            case SdfType.Indexed:
                return SdfIx(p, -1) ? float.MinValue : float.MaxValue;

            case SdfType.Vectored:
                return SdfVectored(new(p))[0];

            default:
                throw new UnreachableException();
        }
    }

    public readonly bool CheckIndexed(WPos p, int index)
    {
        switch (Type)
        {
            case SdfType.Continuous:
                return SdfCont(p) < 0;

            case SdfType.Discrete:
                return SdfDisc(p);

            case SdfType.Indexed:
                return SdfIx(p, index);

            case SdfType.Vectored:
                return SdfVectored(new(p))[0] < 0;

            default:
                throw new UnreachableException();
        }
    }
#pragma warning restore IDE0066 // Convert switch statement to expression

    public readonly bool IsContinuous => Type == SdfType.Continuous;
}
