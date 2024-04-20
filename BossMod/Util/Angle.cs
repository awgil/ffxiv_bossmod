namespace BossMod;

// wrapper around float, stores angle in radians, provides type-safety and convenience
// when describing rotation in world, common convention is 0 for 'south'/'down'/(0, -1) and increasing counterclockwise - so +90 is 'east'/'right'/(1, 0)
public struct Angle(float radians = 0)
{
    public const float RadToDeg = 180 / MathF.PI;
    public const float DegToRad = MathF.PI / 180;

    public float Rad = radians;
    public readonly float Deg => Rad * RadToDeg;

    public static Angle FromDirection(WDir dir) => new(MathF.Atan2(dir.X, dir.Z));
    public readonly WDir ToDirection() => new(Sin(), Cos());

    public static Angle operator +(Angle a, Angle b) => new(a.Rad + b.Rad);
    public static Angle operator -(Angle a, Angle b) => new(a.Rad - b.Rad);
    public static Angle operator -(Angle a) => new(-a.Rad);
    public static Angle operator *(Angle a, float b) => new(a.Rad * b);
    public static Angle operator *(float a, Angle b) => new(a * b.Rad);
    public static Angle operator /(Angle a, float b) => new(a.Rad / b);
    public static bool operator >(Angle a, Angle b) => a.Rad > b.Rad;
    public static bool operator <(Angle a, Angle b) => a.Rad < b.Rad;
    public static bool operator >=(Angle a, Angle b) => a.Rad >= b.Rad;
    public static bool operator <=(Angle a, Angle b) => a.Rad <= b.Rad;
    public readonly Angle Abs() => new(Math.Abs(Rad));
    public readonly float Sin() => MathF.Sin(Rad);
    public readonly float Cos() => MathF.Cos(Rad);
    public readonly float Tan() => MathF.Tan(Rad);
    public static Angle Asin(float x) => new(MathF.Asin(x));
    public static Angle Acos(float x) => new(MathF.Acos(x));

    public readonly Angle Normalized()
    {
        var r = Rad;
        while (r < -MathF.PI)
            r += 2 * MathF.PI;
        while (r > MathF.PI)
            r -= 2 * MathF.PI;
        return new(r);
    }

    public readonly bool AlmostEqual(Angle other, float epsRad) => Math.Abs((this - other).Normalized().Rad) <= epsRad;

    public static bool operator ==(Angle l, Angle r) => l.Rad == r.Rad;
    public static bool operator !=(Angle l, Angle r) => l.Rad != r.Rad;
    public override readonly bool Equals(object? obj) =>  obj is Angle angle && this == angle;
    public override readonly int GetHashCode() => Rad.GetHashCode();
    public override readonly string ToString() => Deg.ToString("f0");
}

public static class AngleExtensions
{
    public static Angle Radians(this float radians) => new(radians);
    public static Angle Degrees(this float degrees) => new(degrees * Angle.DegToRad);
    public static Angle Degrees(this int degrees) => new(degrees * Angle.DegToRad);
}
