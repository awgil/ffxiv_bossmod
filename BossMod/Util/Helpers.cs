namespace BossMod;

// helpers that can potentially be reused for different things

public static class Helpers
{
    public const float RadianConversion = MathF.PI / 180;
    public static readonly float sqrt3 = MathF.Sqrt(3);
    private static readonly Dictionary<object, object> cache = [];

    public static WPos RotateAroundOrigin(float rotatebydegrees, WPos origin, WPos caster)
    {
        if (cache.TryGetValue((rotatebydegrees, origin, caster), out var cachedResult))
            return (WPos)cachedResult;
        var x = MathF.Cos(rotatebydegrees * RadianConversion) * (caster.X - origin.X) - MathF.Sin(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        var z = MathF.Sin(rotatebydegrees * RadianConversion) * (caster.X - origin.X) + MathF.Cos(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        var result = new WPos(origin.X + x, origin.Z + z);
        cache[(rotatebydegrees, origin, caster)] = result;
        return result;
    }
}
