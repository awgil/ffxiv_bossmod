global using DistFn = System.Func<BossMod.WVec, System.Numerics.Vector<float>>;
global using GoalFn = System.Func<BossMod.WVec, System.Numerics.Vector<float>>;
global using PointTest = System.Func<BossMod.WVec, System.Numerics.Vector<int>>;
global using VBool = System.Numerics.Vector<int>;
global using VFloat = System.Numerics.Vector<float>;
global using VInt = System.Numerics.Vector<uint>;
global using VV = System.Numerics.Vector;

namespace BossMod;

#pragma warning disable CA1708 // bug
public static class VectorExtensions
{
    extension(VFloat source)
    {
        public VFloat Abs() => VV.Abs(source);
        public VFloat Sign() => VV.BitwiseAnd(~VV.IsZero(source), VV.CopySign(VFloat.One, source));
        public VBool Mask() => VV.As<float, int>(source);

        public VBool AlmostEqual(float value, float eps) => (source - value).Abs() < eps;

        public static VFloat operator /(VFloat a, float b) => a / VV.Create(b);
        public static VFloat operator /(float a, VFloat b) => VV.Create(a) / b;
        public static VFloat operator +(VFloat a, float b) => a + VV.Create(b);
        public static VFloat operator -(VFloat a, float b) => a - VV.Create(b);
        public static VFloat operator -(float a, VFloat b) => VV.Create(a) - b;

        public static VBool operator <=(VFloat a, VFloat b) => VV.LessThanOrEqual(a, b);
        public static VBool operator <=(VFloat a, float b) => VV.LessThanOrEqual(a, VV.Create(b));
        public static VBool operator >=(VFloat a, VFloat b) => VV.GreaterThanOrEqual(a, b);
        public static VBool operator >=(VFloat a, float b) => VV.GreaterThanOrEqual(a, VV.Create(b));
        public static VBool operator <(VFloat a, VFloat b) => VV.LessThan(a, b);
        public static VBool operator <(VFloat a, float b) => VV.LessThan(a, VV.Create(b));
        public static VBool operator >(VFloat a, VFloat b) => VV.GreaterThan(a, b);
        public static VBool operator >(VFloat a, float b) => VV.GreaterThan(a, VV.Create(b));
    }

    extension(PointTest _)
    {
        public static PointTest Never => _ => VBool.Zero;
        public static PointTest Always => _ => VBool.AllBitsSet;

        public static PointTest operator |(PointTest a, PointTest b) => p => a(p) | b(p);
        public static PointTest operator &(PointTest a, PointTest b) => p => a(p) & b(p);
        public static PointTest operator ^(PointTest a, PointTest b) => p => a(p) ^ b(p);
    }

    extension<T>(Vector<T> mask)
    {
        public Vector<U> WithValue<U>(Vector<U> values) => mask.As<T, U>() & values;
    }

    extension(DistFn sd)
    {
        public DistFn Inverted() => p => -sd(p);
        public DistFn Inverted(bool inverted) => inverted ? Inverted(sd) : sd;

        public DistFn Grow(float value) => p => sd(p) - value;
        public DistFn Shrink(float value) => p => sd(p) + value;

        public PointTest Discretize() => p => sd(p) < VFloat.Zero;

        public float CallOnce(WPos p) => sd(new(p))[0];

        public static DistFn operator |(DistFn a, DistFn b) => p => VV.Min(a(p), b(p));
        public static DistFn operator &(DistFn a, DistFn b) => p => VV.Max(a(p), b(p));
        public static DistFn operator -(DistFn a) => p => -a(p);
    }

    extension(PointTest st)
    {
        public PointTest Inverted() => p => ~st(p);
        public PointTest Inverted(bool inverted) => inverted ? Inverted(st) : st;

        public bool CallOnce(WPos p) => st(new(p))[0] != 0;
    }
}
