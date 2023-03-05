using System;
using System.Numerics;

namespace BossMod
{
    // 2d vector that represents world-space direction on XZ plane
    public struct WDir
    {
        public float X;
        public float Z;

        public WDir(float x = 0, float z = 0) { X = x; Z = z; }
        public WDir(Vector2 v) { X = v.X; Z = v.Y; }
        public Vector2 ToVec2() => new(X, Z);
        public Vector3 ToVec3() => new(X, 0, Z);

        public static WDir operator +(WDir a, WDir b) => new(a.X + b.X, a.Z + b.Z);
        public static WDir operator -(WDir a, WDir b) => new(a.X - b.X, a.Z - b.Z);
        public static WDir operator -(WDir a) => new(-a.X, -a.Z);
        public static WDir operator *(WDir a, float b) => new(a.X * b, a.Z * b);
        public static WDir operator *(float a, WDir b) => new(a * b.X, a * b.Z);
        public static WDir operator /(WDir a, float b) => new(a.X / b, a.Z / b);
        public WDir Abs() => new(Math.Abs(X), Math.Abs(Z));
        public WDir OrthoL() => new(Z, -X); // CCW, same length
        public WDir OrthoR() => new(-Z, X); // CW, same length
        public static float Dot(WDir a, WDir b) => a.X * b.X + a.Z * b.Z;
        public float Dot(WDir a) => X * a.X + Z * a.Z;
        public float LengthSq() => X * X + Z * Z;
        public float Length() => MathF.Sqrt(LengthSq());
        public static WDir Normalize(WDir a) => a / a.Length();
        public WDir Normalized() => this / Length();
        public static bool AlmostZero(WDir a, float eps) => Math.Abs(a.X) <= eps && Math.Abs(a.Z) <= eps;
        public bool AlmostZero(float eps) => AlmostZero(this, eps);
        public static bool AlmostEqual(WDir a, WDir b, float eps) => AlmostZero(a - b, eps);
        public bool AlmostEqual(WDir b, float eps) => AlmostZero(this - b, eps);

        public static bool operator ==(WDir l, WDir r) => l.X == r.X && l.Z == r.Z;
        public static bool operator !=(WDir l, WDir r) => l.X != r.X || l.Z != r.Z;
        public override bool Equals(object? obj) => obj is WDir && this == (WDir)obj;
        public override int GetHashCode() => (X, Z).GetHashCode();
        public override string ToString() => $"({X:f3}, {Z:f3})";
    }

    // 2d vector that represents world-space position on XZ plane
    public struct WPos
    {
        public float X;
        public float Z;

        public WPos(float x = 0, float z = 0) { X = x; Z = z; }
        public WPos(Vector2 v) { X = v.X; Z = v.Y; }
        public Vector2 ToVec2() => new(X, Z);

        public static WPos operator +(WPos a, WDir b) => new(a.X + b.X, a.Z + b.Z);
        public static WPos operator +(WDir a, WPos b) => new(a.X + b.X, a.Z + b.Z);
        public static WPos operator -(WPos a, WDir b) => new(a.X - b.X, a.Z - b.Z);
        public static WDir operator -(WPos a, WPos b) => new(a.X - b.X, a.Z - b.Z);
        public static bool AlmostEqual(WPos a, WPos b, float eps) => (a - b).AlmostZero(eps);
        public bool AlmostEqual(WPos b, float eps) => (this - b).AlmostZero(eps);
        public static WPos Lerp(WPos from, WPos to, float progress) => new(from.ToVec2() * (1 - progress) + to.ToVec2() * progress);

        public static bool operator ==(WPos l, WPos r) => l.X == r.X && l.Z == r.Z;
        public static bool operator !=(WPos l, WPos r) => l.X != r.X || l.Z != r.Z;
        public override bool Equals(object? obj) => obj is WPos && this == (WPos)obj;
        public override int GetHashCode() => (X, Z).GetHashCode();
        public override string ToString() => $"[{X:f3}, {Z:f3}]";

        // area checks
        public bool InTri(WPos v1, WPos v2, WPos v3)
        {
            var s = (v2.X - v1.X) * (Z - v1.Z) - (v2.Z - v1.Z) * (X - v1.X);
            var t = (v3.X - v2.X) * (Z - v2.Z) - (v3.Z - v2.Z) * (X - v2.X);
            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;
            var d = (v1.X - v3.X) * (Z - v3.Z) - (v1.Z - v3.Z) * (X - v3.X);
            return d == 0 || (d < 0) == (s + t <= 0);
        }

        public bool InRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth)
        {
            var offset = this - origin;
            var normal = direction.OrthoL();
            var dotDir = offset.Dot(direction);
            var dotNormal = offset.Dot(normal);
            return dotDir >= -lenBack && dotDir <= lenFront && MathF.Abs(dotNormal) <= halfWidth;
        }

        public bool InRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
        {
            return InRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
        }

        public bool InRect(WPos origin, WDir startToEnd, float halfWidth)
        {
            var len = startToEnd.Length();
            return InRect(origin, startToEnd / len, len, 0, halfWidth);
        }

        public bool InCircle(WPos origin, float radius)
        {
            return (this - origin).LengthSq() <= radius * radius;
        }

        public bool InDonut(WPos origin, float innerRadius, float outerRadius)
        {
            return InCircle(origin, outerRadius) && !InCircle(origin, innerRadius);
        }

        public bool InCone(WPos origin, WDir direction, Angle halfAngle)
        {
            return (this - origin).Normalized().Dot(direction) >= halfAngle.Cos();
        }

        public bool InCone(WPos origin, Angle direction, Angle halfAngle)
        {
            return InCone(origin, direction.ToDirection(), halfAngle);
        }

        public bool InCircleCone(WPos origin, float radius, WDir direction, Angle halfAngle)
        {
            return InCircle(origin, radius) && InCone(origin, direction, halfAngle);
        }

        public bool InCircleCone(WPos origin, float radius, Angle direction, Angle halfAngle)
        {
            return InCircle(origin, radius) && InCone(origin, direction, halfAngle);
        }

        public bool InDonutCone(WPos origin, float innerRadius, float outerRadius, WDir direction, Angle halfAngle)
        {
            return InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);
        }

        public bool InDonutCone(WPos origin, float innerRadius, float outerRadius, Angle direction, Angle halfAngle)
        {
            return InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);
        }
    }
}
