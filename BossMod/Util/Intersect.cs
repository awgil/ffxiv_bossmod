namespace BossMod;

// ray-shape intersection functions return parameter along ray dir of intersection point; if intersection does not happen, they return float.MaxValue
public static class Intersect
{
    public static float RayCircle(WPos rayOrigin, WDir rayDir, WPos circleCenter, float circleRadius)
    {
        var ccToRO = rayOrigin - circleCenter;
        // (ccToRO + t * rayDir) ^ 2 = R^2 => t^2 + 2 * t * ccToRO dot rayDir + ccToRO^2 - R^2 = 0
        var halfB = ccToRO.Dot(rayDir);
        var halfDSq = halfB * halfB - ccToRO.LengthSq() + circleRadius * circleRadius;
        if (halfDSq < 0)
            return float.MaxValue; // never intersects
        var t = -halfB + MathF.Sqrt(halfDSq);
        return t >= 0 ? t : float.MaxValue;
    }

    public static float RaySegment(WPos rayOrigin, WDir rayDir, WPos a, WPos b)
    {
        // rayOrigin + t * rayDir = a + u * (b - a)
        // mul by n = (b - a).normal: t * rayDir.dot(n) = (a - rayOrigin).dot(n)
        var lineDir = b - a;
        var n = lineDir.OrthoL(); // don't bother with normalization here
        var ddn = rayDir.Dot(n);
        var oan = (a - rayOrigin).Dot(n);
        if (ddn == 0)
            return oan == 0 ? 0 : float.MaxValue;
        var t = oan / ddn;
        if (t < 0)
            return float.MaxValue;
        var p = rayOrigin + t * rayDir;
        var u = lineDir.Dot(p - a);
        return u >= 0 && u <= lineDir.LengthSq() ? t : float.MaxValue;
    }

    public static float RayQuad(WPos rayOrigin, WDir rayDir, WPos a, WPos b, WPos c, WPos d)
    {
        var t1 = RaySegment(rayOrigin, rayDir, a, b);
        var t2 = RaySegment(rayOrigin, rayDir, b, c);
        var t3 = RaySegment(rayOrigin, rayDir, c, d);
        var t4 = RaySegment(rayOrigin, rayDir, d, a);
        return Math.Min(Math.Min(t1, t2), Math.Min(t3, t4));
    }

    public static float RayRect(WPos rayOrigin, WDir rayDir, WPos rectCenter, WDir hDir, float halfWidth, float halfHeight)
    {
        var h = hDir * halfHeight;
        var w = hDir.OrthoL() * halfWidth;
        return RayQuad(rayOrigin, rayDir, rectCenter + h + w, rectCenter + h - w, rectCenter - h - w, rectCenter - h + w);
    }
}
