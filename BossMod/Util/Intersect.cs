﻿namespace BossMod;

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

    // circle-shape intersections; they return true if shapes intersect or touch, false otherwise
    // these are used e.g. for player-initiated aoes
    public static bool CircleCircle(WDir circleOffset, float circleRadius, float radius)
    {
        var rsum = circleRadius + radius;
        return circleOffset.LengthSq() <= rsum * rsum;
    }
    public static bool CircleCircle(WPos circleCenter, float circleRadius, WPos center, float radius) => CircleCircle(circleCenter - center, circleRadius, radius);

    public static bool CircleCone(WDir circleOffset, float circleRadius, float coneRadius, WDir coneDir, Angle halfAngle)
    {
        var lsq = circleOffset.LengthSq();
        var rsq = circleRadius * circleRadius;
        if (lsq <= rsq)
            return true; // circle contains cone origin
        var rsum = circleRadius + coneRadius;
        if (lsq > rsum * rsum)
            return false; // circle can't intersect the cone, no matter the half-angle
        if (halfAngle.Rad >= MathF.PI)
            return true; // it's actually a circle-circle intersection

        var correctSide = circleOffset.Dot(coneDir) > 0;
        var normal = coneDir.OrthoL();
        var sin = halfAngle.Sin();
        var distFromAxis = circleOffset.Dot(normal);
        var originInCone = (halfAngle.Rad - MathF.PI * 0.5f) switch
        {
            < 0 => correctSide && distFromAxis * distFromAxis <= lsq * sin * sin,
            > 0 => correctSide || distFromAxis * distFromAxis >= lsq * sin * sin,
            _ => correctSide,
        };
        if (originInCone)
            return true; // circle origin is within cone sides

        // ensure normal points to the half-plane that contains circle origin
        if (distFromAxis < 0)
            normal = -normal;

        // see whether circle intersects side
        var side = coneDir * halfAngle.Cos() + normal * sin;
        var distFromSide = Math.Abs(circleOffset.Cross(side));
        if (distFromSide > circleRadius)
            return false; // too far
        var distAlongSide = circleOffset.Dot(side);
        if (distAlongSide < 0)
            return false; // behind origin; note that we don't need to test intersection with origin
        if (distAlongSide <= coneRadius)
            return true; // circle-side intersection

        // finally, we need to check far corner
        var corner = side * coneRadius;
        return (circleOffset - corner).LengthSq() <= rsq;
    }
    public static bool CircleCone(WPos circleCenter, float circleRadius, WPos coneCenter, float coneRadius, WDir coneDir, Angle halfAngle) => CircleCone(circleCenter - coneCenter, circleRadius, coneRadius, coneDir, halfAngle);
}
