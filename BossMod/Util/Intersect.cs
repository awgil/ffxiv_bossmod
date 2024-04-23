namespace BossMod;

// ray-shape intersection functions return parameter along ray dir of intersection point; if intersection does not happen, they return float.MaxValue
public static class Intersect
{
    public static float RayCircle(WPos rayOrigin, WDir rayDir, WPos circleCenter, float circleRadius)
    {
        var ccToRO = rayOrigin - circleCenter;
        var halfB = ccToRO.Dot(rayDir);
        var halfDSq = halfB * halfB - ccToRO.LengthSq() + circleRadius * circleRadius;
        if (halfDSq < 0)
            return float.MaxValue; // never intersects

        var sqrtHalfDSq = MathF.Sqrt(halfDSq);
        var t1 = -halfB + sqrtHalfDSq;
        var t2 = -halfB - sqrtHalfDSq;
        if (t1 >= 0 && t2 >= 0)
            return Math.Min(t1, t2);
        else if (t1 >= 0)
            return t1;
        else if (t2 >= 0)
            return t2;
        else
            return float.MaxValue;
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

    public static WPos ClosestPointOnSegment(WPos a, WPos b, WPos p)
    {
        WDir ab = b - a;
        float t = WDir.Dot(p - a, ab) / ab.LengthSq();
        if (t < 0)
            return a;
        if (t > 1)
            return b;
        return a + t * ab;
    }

    public static WPos LineSegment(WPos a, WPos b, WPos p, WPos q)
    {
        WDir ab = b - a;
        WDir pq = q - p;
        WDir ap = p - a;
        float dot1 = WDir.Dot(ab, pq);
        float dot2 = WDir.Dot(ab, ap);
        float dot3 = WDir.Dot(pq, ap);
        if (dot1 == 0)
            return default;
        float t = (dot2 - dot3) / dot1;
        if (t < 0 || t > 1)
            return default;
        return a + t * ab;
    }

    public static WPos IntersectLineSegment(WPos p1, WPos p2, WPos p3, WPos p4)
    {
        var d = (p4.Z - p3.Z) * (p1.X - p3.X) + (-p4.X + p3.X) * (p1.Z - p3.Z);
        var n = (p4.Z - p3.Z) * (p2.X - p1.X) + (-p4.X + p3.X) * (p2.Z - p1.Z);
        var u = d / n;
        return new WPos(p1.X + u * (p2.X - p1.X), p1.Z + u * (p2.Z - p1.Z));
    }
}
