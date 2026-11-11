namespace BossMod;

// 2d vector that represents world-space direction on XZ plane
public record struct WDir(float X, float Z)
{
    public WDir(Vector2 v) : this(v.X, v.Y) { }
    public readonly Vector2 ToVec2() => new(X, Z);
    public readonly Vector3 ToVec3(float y = 0) => new(X, y, Z);
    public readonly Vector4 ToVec4(float y = 0, float w = 0) => new(X, y, Z, w);
    public readonly WPos ToWPos() => new(X, Z);
    public readonly Angle ToAngle() => Angle.FromDirection(this);

    public static WDir operator +(WDir a, WDir b) => new(a.X + b.X, a.Z + b.Z);
    public static WDir operator -(WDir a, WDir b) => new(a.X - b.X, a.Z - b.Z);
    public static WDir operator -(WDir a) => new(-a.X, -a.Z);
    public static WDir operator *(WDir a, float b) => new(a.X * b, a.Z * b);
    public static WDir operator *(float a, WDir b) => new(a * b.X, a * b.Z);
    public static WDir operator /(WDir a, float b) => new(a.X / b, a.Z / b);
    public readonly WDir Abs() => new(Math.Abs(X), Math.Abs(Z));
    public readonly WDir Sign() => new(Math.Sign(X), Math.Sign(Z));
    public readonly WDir OrthoL() => new(Z, -X); // CCW, same length
    public readonly WDir OrthoR() => new(-Z, X); // CW, same length
    public readonly WDir MirrorX() => new(-X, Z);
    public readonly WDir MirrorZ() => new(X, -Z);
    public static float Dot(WDir a, WDir b) => a.X * b.X + a.Z * b.Z;
    public readonly float Dot(WDir a) => X * a.X + Z * a.Z;
    public readonly VFloat Dot(WVec a) => X * a.X + Z * a.Z;
    public static float Cross(WDir a, WDir b) => a.X * b.Z - a.Z * b.X;
    public readonly float Cross(WDir b) => Cross(this, b);
    public readonly WDir Rotate(WDir dir) => new(X * dir.Z + Z * dir.X, Z * dir.Z - X * dir.X);
    public readonly WDir Rotate(Angle dir) => Rotate(dir.ToDirection());
    public readonly float LengthSq() => X * X + Z * Z;
    public readonly float Length() => MathF.Sqrt(LengthSq());
    public static WDir Normalize(WDir a, float zeroThreshold = 0) => a.Length() is var len && len > zeroThreshold ? a / len : default;
    public readonly WDir Normalized(float zeroThreshold = 0) => Normalize(this, zeroThreshold);
    public static bool AlmostZero(WDir a, float eps) => Math.Abs(a.X) <= eps && Math.Abs(a.Z) <= eps;
    public readonly bool AlmostZero(float eps) => AlmostZero(this, eps);
    public static bool AlmostEqual(WDir a, WDir b, float eps) => AlmostZero(a - b, eps);
    public readonly bool AlmostEqual(WDir b, float eps) => AlmostZero(this - b, eps);
    public readonly WDir Scaled(float multiplier) => new(X * multiplier, Z * multiplier);
    public readonly WDir Rounded() => new(MathF.Round(X), MathF.Round(Z));
    public readonly WDir Rounded(float precision) => Scaled(1.0f / precision).Rounded().Scaled(precision);
    public readonly WDir Floor() => new(MathF.Floor(X), MathF.Floor(Z));

    public override readonly string ToString() => $"({X:f3}, {Z:f3})";
    public override readonly int GetHashCode() => (X, Z).GetHashCode(); // TODO: this is a hack, the default should be good enough, but for whatever reason (X, -Z).GetHashCode() == (-X, Z).GetHashCode()...

    // area checks, assuming this is an offset from shape's center
    public readonly bool InRect(WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var dotDir = Dot(direction);
        var dotNormal = Dot(direction.OrthoL());
        return dotDir >= -lenBack && dotDir <= lenFront && MathF.Abs(dotNormal) <= halfWidth;
    }
}

// 2d vector that represents world-space position on XZ plane
[Serializable]
public record struct WPos(float X, float Z)
{
    public WPos(Vector2 v) : this(v.X, v.Y) { }
    public readonly Vector2 ToVec2() => new(X, Z);
    public readonly Vector3 ToVec3(float y = 0) => new(X, y, Z);
    public readonly Vector4 ToVec4(float y = 0, float w = 0) => new(X, y, Z, w);
    public readonly WDir ToWDir() => new(X, Z);

    public static WPos operator +(WPos a, WDir b) => new(a.X + b.X, a.Z + b.Z);
    public static WPos operator +(WDir a, WPos b) => new(a.X + b.X, a.Z + b.Z);
    public static WPos operator -(WPos a, WDir b) => new(a.X - b.X, a.Z - b.Z);
    public static WDir operator -(WPos a, WPos b) => new(a.X - b.X, a.Z - b.Z);
    public static bool AlmostEqual(WPos a, WPos b, float eps) => (a - b).AlmostZero(eps);
    public readonly bool AlmostEqual(WPos b, float eps) => (this - b).AlmostZero(eps);
    public readonly WPos Scaled(float multiplier) => new(X * multiplier, Z * multiplier);
    public readonly WPos Rounded() => new(MathF.Round(X), MathF.Round(Z));
    public readonly WPos Rounded(float precision) => Scaled(1.0f / precision).Rounded().Scaled(precision);
    public static WPos Lerp(WPos from, WPos to, float progress) => new(from.ToVec2() * (1 - progress) + to.ToVec2() * progress);

    public override readonly string ToString() => $"[{X:f3}, {Z:f3}]";
    public override readonly int GetHashCode() => (X, Z).GetHashCode(); // TODO: this is a hack, the default should be good enough, but for whatever reason (X, -Z).GetHashCode() == (-X, Z).GetHashCode()...

    // area checks
    public readonly bool InTri(WPos v1, WPos v2, WPos v3)
    {
        var s = (v2.X - v1.X) * (Z - v1.Z) - (v2.Z - v1.Z) * (X - v1.X);
        var t = (v3.X - v2.X) * (Z - v2.Z) - (v3.Z - v2.Z) * (X - v2.X);
        if ((s < 0) != (t < 0) && s != 0 && t != 0)
            return false;
        var d = (v1.X - v3.X) * (Z - v3.Z) - (v1.Z - v3.Z) * (X - v3.X);
        return d == 0 || (d < 0) == (s + t <= 0);
    }

    public readonly bool InRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth) => (this - origin).InRect(direction, lenFront, lenBack, halfWidth);
    public readonly bool InRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => (this - origin).InRect(direction.ToDirection(), lenFront, lenBack, halfWidth);
    public readonly bool InRect(WPos origin, WDir startToEnd, float halfWidth)
    {
        var len = startToEnd.Length();
        return InRect(origin, startToEnd / len, len, 0, halfWidth);
    }

    public readonly bool InCircle(WPos origin, float radius) => (this - origin).LengthSq() <= radius * radius;
    public readonly bool InDonut(WPos origin, float innerRadius, float outerRadius) => InCircle(origin, outerRadius) && !InCircle(origin, innerRadius);

    public readonly bool InCone(WPos origin, WDir direction, Angle halfAngle) => (this - origin).Normalized().Dot(direction) >= halfAngle.Cos();
    public readonly bool InCone(WPos origin, Angle direction, Angle halfAngle) => InCone(origin, direction.ToDirection(), halfAngle);

    public readonly bool InCircleCone(WPos origin, float radius, WDir direction, Angle halfAngle) => InCircle(origin, radius) && InCone(origin, direction, halfAngle);
    public readonly bool InCircleCone(WPos origin, float radius, Angle direction, Angle halfAngle) => InCircle(origin, radius) && InCone(origin, direction, halfAngle);

    public readonly bool InDonutCone(WPos origin, float innerRadius, float outerRadius, WDir direction, Angle halfAngle) => InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);
    public readonly bool InDonutCone(WPos origin, float innerRadius, float outerRadius, Angle direction, Angle halfAngle) => InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);
}

public record struct WVec(VFloat X, VFloat Z)
{
    public WVec(float x, float z) : this(VV.Create(x), VV.Create(z)) { }
    public WVec(WDir wd) : this(wd.X, wd.Z) { }
    public WVec(WPos wp) : this(wp.X, wp.Z) { }

    public static readonly WVec Invalid = new(float.NaN, float.NaN);

    public readonly VBool IsValid() => (VV.IsFinite(X) & VV.IsFinite(Z)).Mask();
    public readonly WVec Rotate(WVec dir) => new(X * dir.Z + Z * dir.X, Z * dir.Z - X * dir.X);
    public readonly WVec OrthoL() => new(Z, -X); // CCW, same length
    public readonly WVec OrthoR() => new(-Z, X); // CW, same length

    public readonly VFloat LengthSq() => X * X + Z * Z;
    public readonly VFloat Length() => VV.SquareRoot(LengthSq());
    public readonly WVec Normalized() => this / Length();
    public readonly VFloat Dot(WVec a) => X * a.X + Z * a.Z;
    public readonly VFloat Dot(WDir a) => X * a.X + Z * a.Z;
    public static VFloat Cross(WVec a, WVec b) => a.X * b.Z - a.Z * b.X;
    public readonly VFloat Cross(WVec b) => Cross(this, b);

    public readonly WVec Abs() => new(VV.Abs(X), VV.Abs(Z));
    public readonly WVec ClampPositive() => new(VV.Max(X, VFloat.Zero), VV.Max(Z, VFloat.Zero));
    public readonly VFloat MaxComponent() => VV.Max(X, Z);

    public readonly WVec MirrorX() => new(-X, Z);
    public readonly WVec MirrorZ() => new(X, -Z);

    // exchanges all pairs where Z > X
    public readonly WVec AlignX() => Select(X > Z, new(X, Z), new(Z, X));

    public static WVec Select(VBool bits, WVec left, WVec right) => new(VV.ConditionalSelect(bits, left.X, right.X), VV.ConditionalSelect(bits, left.Z, right.Z));

    public readonly WVec Min(WVec other) => new(VV.Min(X, other.X), VV.Min(Z, other.Z));

    public readonly WPos First() => new(X[0], Z[0]);

    public static VBool AlmostZero(WVec a, VFloat eps) => (VV.Abs(a.X) <= eps) & (VV.Abs(a.Z) <= eps);
    public readonly VBool AlmostZero(VFloat eps) => AlmostZero(this, eps);
    public readonly VBool AlmostZero(float eps) => AlmostZero(this, VV.Create(eps));
    public static VBool AlmostEqual(WVec a, WVec b, VFloat eps) => AlmostZero(a - b, eps);
    public readonly VBool AlmostEqual(WVec b, VFloat eps) => AlmostZero(this - b, eps);
    public readonly VBool AlmostEqual(WPos b, float eps) => AlmostZero(this - b, VV.Create(eps));

    public static WVec Lerp(WVec a, WVec b, VFloat progress) => new(VV.Lerp(a.X, b.X, progress), VV.Lerp(a.Z, b.Z, progress));

    public readonly VBool InRect(WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var dotDir = Dot(direction);
        var dotNormal = Dot(direction.OrthoL());
        return (dotDir >= -lenBack)
            & (dotDir <= lenFront)
            & (dotNormal.Abs() <= halfWidth);
    }
    public readonly VBool InRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth) => (this - origin).InRect(direction, lenFront, lenBack, halfWidth);
    public readonly VBool InRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => (this - origin).InRect(direction.ToDirection(), lenFront, lenBack, halfWidth);
    public readonly VBool InRect(WPos origin, WDir startToEnd, float halfWidth)
    {
        var len = startToEnd.Length();
        return InRect(origin, startToEnd / len, len, 0, halfWidth);
    }

    public readonly VBool InCircle(WPos origin, float radius) => (this - origin).LengthSq() <= radius * radius;
    public readonly VBool InDonut(WPos origin, float innerRadius, float outerRadius) => InCircle(origin, outerRadius) & ~InCircle(origin, innerRadius);

    public readonly VBool InCone(WPos origin, WDir direction, Angle halfAngle) => (this - origin).Normalized().Dot(direction) >= halfAngle.Cos();
    public readonly VBool InCone(WPos origin, Angle direction, Angle halfAngle) => InCone(origin, direction.ToDirection(), halfAngle);

    public readonly WPos this[int index] => new(X[index], Z[index]);

    public static WVec operator +(WVec a, WVec b) => new(a.X + b.X, a.Z + b.Z);
    public static WVec operator +(WVec a, WDir b) => new(a.X + b.X, a.Z + b.Z);
    public static WVec operator -(WVec a, WVec b) => new(a.X - b.X, a.Z - b.Z);
    public static WVec operator -(WVec a, WPos b) => new(a.X - b.X, a.Z - b.Z);
    public static WVec operator -(WPos a, WVec b) => new(a.X - b.X, a.Z - b.Z);
    public static WVec operator -(WVec a, WDir b) => new(a.X - b.X, a.Z - b.Z);
    public static WVec operator *(WVec a, VFloat b) => new(a.X * b, a.Z * b);
    public static WVec operator *(WVec a, float b) => new(a.X * b, a.Z * b);
    public static WVec operator *(float a, WVec b) => b * a;
    public static WVec operator *(VFloat a, WVec b) => b * a;
    public static WVec operator /(WVec a, VFloat b) => new(a.X / b, a.Z / b);
    public static WVec operator -(WVec a) => new(-a.X, -a.Z);
}
