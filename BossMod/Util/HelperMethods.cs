namespace BossMod;

// a bunch of helper methods that can potentially be reused for different things

public static class Helpers
{
    public const float RadianConversion = MathF.PI / 180;
    public static readonly float sqrt3 = MathF.Sqrt(3);
    private static readonly Dictionary<object, object> cache = [];

    public static (WPos p1, WPos p2, WPos p3) CalculateEquilateralTriangleVertices(WPos origin, Angle rotation, float SideLength, float offset = 0)
    {
        if (cache.TryGetValue((origin, rotation, SideLength, offset), out var cachedResult))
            return ((WPos, WPos, WPos))cachedResult;
        var sideOffset = (SideLength + offset) / 2;
        var height = MathF.Sqrt(3) / 2 * (SideLength + offset);
        var direction = rotation.ToDirection();
        var ortho = direction.OrthoR();

        var p1 = origin;
        var p2 = origin + direction * height - ortho * sideOffset;
        var p3 = origin + direction * height + ortho * sideOffset;
        var result = (p1, p2, p3);
        cache[(origin, rotation, SideLength, offset)] = result;
        return result;
    }

    public static List<WPos> CalculateEquilateralTriangleVertices(WPos Center, float HalfSize)
    {
        if (cache.TryGetValue((Center, HalfSize), out var cachedResult))
            return (List<WPos>)cachedResult;
        var halfSide = HalfSize;
        var height = halfSide * sqrt3;
        var center = Center + new WDir(0, height / 3);

        var points = new List<WPos>
        {
            center + new WDir(-halfSide, height / 3),
            center + new WDir(halfSide, height / 3),
            center + new WDir(0, -2 * height / 3)
        };
        cache[(Center, HalfSize)] = points;
        return points;
    }

    public static WPos RotateAroundOrigin(float rotatebydegrees, WPos origin, WPos caster)
    {
        if (cache.TryGetValue((rotatebydegrees, origin, caster), out var cachedResult))
            return (WPos)cachedResult;
        float x = MathF.Cos(rotatebydegrees * RadianConversion) * (caster.X - origin.X) - MathF.Sin(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        float z = MathF.Sin(rotatebydegrees * RadianConversion) * (caster.X - origin.X) + MathF.Cos(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        var result = new WPos(origin.X + x, origin.Z + z);
        cache[(rotatebydegrees, origin, caster)] = result;
        return result;
    }
}