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
