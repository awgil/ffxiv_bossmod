namespace BossMod;

// ray-shape intersection functions return parameter along ray dir of intersection point; if intersection does not happen, they return float.MaxValue
// rayDir is assumed to be normalized
// WDir rayOriginOffset overload for symmetrical shapes uses offset from shape center for ray origin
public static class Intersect
{
    public static float RayCircle(WDir rayOriginOffset, WDir rayDir, float circleRadius)
    {
        // (rayOriginOffset + t * rayDir) ^ 2 = R^2 => t^2 + 2 * t * rayOriginOffset dot rayDir + rayOriginOffset^2 - R^2 = 0
        var halfB = rayOriginOffset.Dot(rayDir);
        var halfDSq = halfB * halfB - rayOriginOffset.LengthSq() + circleRadius * circleRadius;
        if (halfDSq < 0)
            return float.MaxValue; // never intersects
        var t = -halfB + MathF.Sqrt(halfDSq);
        return t >= 0 ? t : float.MaxValue;
    }
    public static float RayCircle(WPos rayOrigin, WDir rayDir, WPos circleCenter, float circleRadius) => RayCircle(rayOrigin - circleCenter, rayDir, circleRadius);

    // halfWidth is along X, halfHeight is along Z
    public static float RayAABB(WDir rayOriginOffset, WDir rayDir, float halfWidth, float halfHeight)
    {
        // see https://tavianator.com/2022/ray_box_boundary.html
        // rayOriginOffset.i + t.i * rayDir.i = +- halfSize.i => t.i = (+-halfSize.i - rayOriginOffset.i) / rayDir.i
        var invX = 1.0f / rayDir.X; // could be +- inf
        var invZ = 1.0f / rayDir.Z;
        float tmin = -float.Epsilon;
        float tmax = float.MaxValue;

        // if rayDir.i == 0, inv.i == +- inf
        // then if ray origin is outside box, ti1 = ti2 = +-inf (infinities of same sign)
        // if it's inside box, ti1 = -ti2 = +-inf (infinities of different sign)
        // if it's exactly on bound, either one of the ti is infinity, other is NaN
        float tx1 = (-halfWidth - rayOriginOffset.X) * invX;
        float tx2 = (+halfWidth - rayOriginOffset.X) * invX;
        float tz1 = (-halfHeight - rayOriginOffset.Z) * invZ;
        float tz2 = (+halfHeight - rayOriginOffset.Z) * invZ;

        // naive version - works fine for infinities, but not for nans - clip 'ray segment' to part between two lines
        // tmin = max(tmin, min(t1, t2));
        // tmax = min(tmax, max(t1, t2));
        static float min(float x, float y) => x < y ? x : y; // min(x, NaN) = NaN; min(NaN, x) = x
        static float max(float x, float y) => x > y ? x : y; // max(x, NaN) = NaN; max(NaN, x) = x
        tmin = min(max(tx1, tmin), max(tx2, tmin));
        tmax = max(min(tx1, tmax), min(tx2, tmax));
        tmin = min(max(tz1, tmin), max(tz2, tmin));
        tmax = max(min(tz1, tmax), min(tz2, tmax));
        // explanation:
        // tx1 = NaN => tx2 = +-inf => tmin = min(tmin, tmin -or- +inf) = tmin, tmax = max(tmax, tmax -or- -inf) = tmax
        // tx2 = NaN => tx1 = +-inf => tmin = min(tmin -or- +inf, tmin) = tmin, tmax = min(tmax -or- +inf, tmax) = tmax
        // so NaN's don't change 'clipped' ray segment
        return tmin > tmax ? float.MaxValue : tmin >= 0 ? tmin : tmax >= 0 ? tmax : float.MaxValue;
    }
    public static float RayAABB(WPos rayOrigin, WDir rayDir, WPos boxCenter, float halfWidth, float halfHeight) => RayAABB(rayOrigin - boxCenter, rayDir, halfWidth, halfHeight);

    // if rotation is 0, half-width is along X and half-height is along Z
    public static float RayRect(WDir rayOriginOffset, WDir rayDir, WDir rectRotation, float halfWidth, float halfHeight)
    {
        var rectX = rectRotation.OrthoL();
        WDir rotate(WDir d) => new(d.Dot(rectX), d.Dot(rectRotation));
        return RayAABB(rotate(rayOriginOffset), rotate(rayDir), halfWidth, halfHeight);
    }
    public static float RayRect(WPos rayOrigin, WDir rayDir, WPos rectCenter, WDir rectRotation, float halfWidth, float halfHeight) => RayRect(rayOrigin - rectCenter, rayDir, rectRotation, halfWidth, halfHeight);

    // infinite line intersection; 'center of symmetry' is any point on line
    // note that 'line' doesn't have to be normalized
    public static float RayLine(WDir rayOriginOffset, WDir rayDir, WDir line)
    {
        // rayOriginOffset + t * rayDir = u * line
        // mul by n = line.ortho: rayOriginOffset dot n + t * rayDir dot n = 0 => t = -(rayOriginOffset dot n) / (rayDir dot n)
        var n = line.OrthoL(); // don't bother with normalization here
        var ddn = rayDir.Dot(n);
        var odn = rayOriginOffset.Dot(n);
        if (ddn == 0)
            return odn == 0 ? 0 : float.MaxValue; // ray parallel to line
        var t = -odn / ddn;
        return t >= 0 ? t : float.MaxValue;
    }
    public static float RayLine(WPos rayOrigin, WDir rayDir, WPos lineOrigin, WDir line) => RayLine(rayOrigin - lineOrigin, rayDir, line);

    public static float RaySegment(WDir rayOriginOffset, WDir rayDir, WDir oa, WDir ob)
    {
        var lineDir = ob - oa;
        var t = RayLine(rayOriginOffset - oa, rayDir, lineDir);
        if (t == float.MaxValue)
            return float.MaxValue;

        // check that intersection point is inside segment
        var p = rayOriginOffset + t * rayDir;
        var u = lineDir.Dot(p - oa);
        return u >= 0 && u <= lineDir.LengthSq() ? t : float.MaxValue;
    }
    public static float RaySegments(WDir rayOriginOffset, WDir rayDir, IEnumerable<(WDir, WDir)> edges) => edges.Min(e => RaySegment(rayOriginOffset, rayDir, e.Item1, e.Item2));

    public static float RayPolygon(WDir rayOriginOffset, WDir rayDir, RelPolygonWithHoles poly)
    {
        var dist = RaySegments(rayOriginOffset, rayDir, poly.ExteriorEdges);
        foreach (var h in poly.Holes)
            dist = Math.Min(dist, RaySegments(rayOriginOffset, rayDir, poly.InteriorEdges(h)));
        return dist;
    }
    public static float RayPolygon(WDir rayOriginOffset, WDir rayDir, RelSimplifiedComplexPolygon poly) => poly.Parts.Min(part => RayPolygon(rayOriginOffset, rayDir, part));
}
