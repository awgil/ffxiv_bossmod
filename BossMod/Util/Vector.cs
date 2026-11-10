namespace BossMod;

public static class VectorExtensions
{
    extension(VFloat source)
    {
        public VFloat Abs() => VV.Abs(source);
        public VFloat Sign() => VV.BitwiseAnd(~VV.IsZero(source), VV.CopySign(VFloat.One, source));
        public VBool Mask() => VV.As<float, int>(source);

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
}
