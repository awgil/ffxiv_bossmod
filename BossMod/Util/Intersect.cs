namespace BossMod;

// ray-shape intersection functions return parameter along ray dir of intersection point; if intersection does not happen, they return float.MaxValue
// rayDir is assumed to be normalized
// WDir rayOriginOffset overload for symmetrical shapes uses offset from shape center for ray origin
[SkipLocalsInit]
public static class Intersect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayCircle(in WDir rayOriginOffset, in WDir rayDir, float circleRadius)
    {
        // (rayOriginOffset + t * rayDir) ^ 2 = R^2 => t^2 + 2 * t * rayOriginOffset dot rayDir + rayOriginOffset^2 - R^2 = 0
        var halfB = rayOriginOffset.Dot(rayDir);
        var halfDSq = halfB * halfB - rayOriginOffset.LengthSq() + circleRadius * circleRadius;
        if (halfDSq < 0f)
        {
            return float.MaxValue; // never intersects
        }

        var t = -halfB + MathF.Sqrt(halfDSq);
        return t >= 0f ? t : float.MaxValue;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayCircle(in WPos rayOrigin, in WDir rayDir, in WPos circleCenter, float circleRadius) => RayCircle(rayOrigin - circleCenter, rayDir, circleRadius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RayCircle(in WDir rayOriginOffset, in WDir rayDir, float circleRadius, float maxDist)
    {
        var t = (-rayOriginOffset).Dot(rayDir);
        var tClamped = Math.Max(0f, Math.Min(maxDist, t));

        var closest = rayOriginOffset + rayDir * tClamped;
        return closest.LengthSq() <= circleRadius * circleRadius;
    }

    // halfWidth is along X, halfHeight is along Z
    public static float RayAABB(in WDir rayOriginOffset, in WDir rayDir, float halfWidth, float halfHeight)
    {
        // see https://tavianator.com/2022/ray_box_boundary.html
        // rayOriginOffset.i + t.i * rayDir.i = +- halfSize.i => t.i = (+-halfSize.i - rayOriginOffset.i) / rayDir.i
        var invX = 1.0f / rayDir.X; // could be +- inf
        var invZ = 1.0f / rayDir.Z;
        var tmin = -float.Epsilon;
        var tmax = float.MaxValue;

        // if rayDir.i == 0, inv.i == +- inf
        // then if ray origin is outside box, ti1 = ti2 = +-inf (infinities of same sign)
        // if it's inside box, ti1 = -ti2 = +-inf (infinities of different sign)
        // if it's exactly on bound, either one of the ti is infinity, other is NaN
        var offsetX = rayOriginOffset.X;
        var offsetZ = rayOriginOffset.Z;
        var tx1 = (-halfWidth - offsetX) * invX;
        var tx2 = (+halfWidth - offsetX) * invX;
        var tz1 = (-halfHeight - offsetZ) * invZ;
        var tz2 = (+halfHeight - offsetZ) * invZ;

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
        return tmin > tmax ? float.MaxValue : tmin >= 0f ? tmin : tmax >= 0f ? tmax : float.MaxValue;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayAABB(in WPos rayOrigin, in WDir rayDir, in WPos boxCenter, float halfWidth, float halfHeight) => RayAABB(rayOrigin - boxCenter, rayDir, halfWidth, halfHeight);

    // if rotation is 0, half-width is along X and half-height is along Z
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayRect(in WDir rayOriginOffset, in WDir rayDir, in WDir rectRotation, float halfWidth, float halfHeight)
    {
        var rectX = rectRotation.OrthoL();
        return RayAABB(new(rayOriginOffset.Dot(rectX), rayOriginOffset.Dot(rectRotation)), new(rayDir.Dot(rectX), rayDir.Dot(rectRotation)), halfWidth, halfHeight);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayRect(in WPos rayOrigin, in WDir rayDir, in WPos rectCenter, in WDir rectRotation, float halfWidth, float halfHeight) => RayRect(rayOrigin - rectCenter, rayDir, rectRotation, halfWidth, halfHeight);

    // infinite line intersection; 'center of symmetry' is any point on line
    // note that 'line' doesn't have to be normalized
    public static float RayLine(in WDir rayOriginOffset, in WDir rayDir, in WDir line)
    {
        // rayOriginOffset + t * rayDir = u * line
        // mul by n = line.ortho: rayOriginOffset dot n + t * rayDir dot n = 0 => t = -(rayOriginOffset dot n) / (rayDir dot n)
        var n = line.OrthoL(); // don't bother with normalization here
        var ddn = rayDir.Dot(n);
        var odn = rayOriginOffset.Dot(n);
        if (ddn == 0)
        {
            return odn == 0 ? 0 : float.MaxValue; // ray parallel to line
        }

        var t = -odn / ddn;
        return t >= 0 ? t : float.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayLine(in WPos rayOrigin, in WDir rayDir, in WPos lineOrigin, in WDir line) => RayLine(rayOrigin - lineOrigin, rayDir, line);

    public static float RaySegment(in WDir rayOriginOffset, in WDir rayDir, in WDir oa, in WDir ob)
    {
        var lineDir = ob - oa;
        var t = RayLine(rayOriginOffset - oa, rayDir, lineDir);
        if (t == float.MaxValue)
        {
            return float.MaxValue;
        }

        // check that intersection point is inside segment
        var p = rayOriginOffset + t * rayDir;
        var u = lineDir.Dot(p - oa);
        return u >= 0f && u <= lineDir.LengthSq() ? t : float.MaxValue;
    }

    public static float RaySegment(in WPos rayOrigin, in WDir rayDir, in WPos vertexA, in WPos vertexB)
    {
        var lineDir = vertexB - vertexA;
        var t = RayLine(rayOrigin - vertexA, rayDir, lineDir);
        if (t == float.MaxValue)
        {
            return float.MaxValue;
        }

        var p = rayOrigin + t * rayDir;
        var u = lineDir.Dot(p - vertexA);
        return u >= 0f && u <= lineDir.LengthSq() ? t : float.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayPolygon(in WDir rayOriginOffset, in WDir rayDir, RelSimplifiedComplexPolygon poly)
        => poly.Raycast(rayOriginOffset, rayDir);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayPolygon(in WPos rayOrigin, in WDir rayDir, in WPos polyCenter, RelSimplifiedComplexPolygon poly)
        => RayPolygon(rayOrigin - polyCenter, rayDir, poly);

    // circle-shape intersections; they return true if shapes intersect or touch, false otherwise
    // these are used e.g. for player-initiated aoes
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleCircle(in WDir circleOffset, float circleRadius, float radius)
    {
        var rsum = circleRadius + radius;
        return circleOffset.LengthSq() <= rsum * rsum;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleCircle(in WPos circleCenter, float circleRadius, in WPos center, float radius) => CircleCircle(circleCenter - center, circleRadius, radius);

    public static bool CircleCone(in WDir circleOffset, float circleRadius, float coneRadius, in WDir coneDir, Angle halfAngle)
    {
        var lsq = circleOffset.LengthSq();
        var rsq = circleRadius * circleRadius;
        if (lsq <= rsq)
        {
            return true; // circle contains cone origin
        }

        var rsum = circleRadius + coneRadius;
        if (lsq > rsum * rsum)
        {
            return false; // circle can't intersect the cone, no matter the half-angle
        }

        if (halfAngle.Rad >= MathF.PI)
        {
            return true; // it's actually a circle-circle intersection
        }

        var correctSide = circleOffset.Dot(coneDir) > 0;
        var normal = coneDir.OrthoL();
        var sin = halfAngle.Sin();
        var distFromAxis = circleOffset.Dot(normal);
        var originInCone = (halfAngle.Rad - Angle.HalfPi) switch
        {
            < 0 => correctSide && distFromAxis * distFromAxis <= lsq * sin * sin,
            > 0 => correctSide || distFromAxis * distFromAxis >= lsq * sin * sin,
            _ => correctSide,
        };
        if (originInCone)
        {
            return true; // circle origin is within cone sides
        }

        // ensure normal points to the half-plane that contains circle origin
        if (distFromAxis < 0)
        {
            normal = -normal;
        }

        // see whether circle intersects side
        var side = coneDir * halfAngle.Cos() + normal * sin;
        var distFromSide = Math.Abs(circleOffset.Cross(side));
        if (distFromSide > circleRadius)
        {
            return false; // too far
        }

        var distAlongSide = circleOffset.Dot(side);
        if (distAlongSide < 0)
        {
            return false; // behind origin; note that we don't need to test intersection with origin
        }

        if (distAlongSide <= coneRadius)
        {
            return true; // circle-side intersection
        }

        // finally, we need to check far corner
        var corner = side * coneRadius;
        return (circleOffset - corner).LengthSq() <= rsq;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleCone(in WPos circleCenter, float circleRadius, in WPos coneCenter, float coneRadius, in WDir coneDir, Angle halfAngle) => CircleCone(circleCenter - coneCenter, circleRadius, coneRadius, coneDir, halfAngle);

    public static bool CircleAARect(WDir circleOffset, float circleRadius, float halfExtentX, float halfExtentZ)
    {
        circleOffset = circleOffset.Abs(); // symmetrical along X/Z, consider only positive quadrant
        var cornerOffset = circleOffset - new WDir(halfExtentX, halfExtentZ); // relative to corner
        if (cornerOffset.X > circleRadius || cornerOffset.Z > circleRadius)
        {
            return false; // circle is too far from one of the edges, so can't intersect
        }

        if (cornerOffset.X <= 0 || cornerOffset.Z <= 0)
        {
            return true; // circle center is inside/on the edge, or close enough to one of the edges to intersect
        }

        return cornerOffset.LengthSq() <= circleRadius * circleRadius; // check whether circle touches the corner
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleAARect(in WPos circleCenter, float circleRadius, in WPos rectCenter, float halfExtentX, float halfExtentZ) => CircleAARect(circleCenter - rectCenter, circleRadius, halfExtentX, halfExtentZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleRect(in WDir circleOffset, float circleRadius, in WDir rectZDir, float halfExtentX, float halfExtentZ) => CircleAARect(circleOffset.Rotate(rectZDir.MirrorX()), circleRadius, halfExtentX, halfExtentZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleRect(in WPos circleCenter, float circleRadius, in WPos rectCenter, in WDir rectZDir, float halfExtentX, float halfExtentZ) => CircleRect(circleCenter - rectCenter, circleRadius, rectZDir, halfExtentX, halfExtentZ);

    // check if AABB Rect edge intersects with a circle
    public static bool CircleAARectEdge(WDir circleOffset, float circleRadius, float halfExtentX, float halfExtentZ)
    {
        circleOffset = circleOffset.Abs();

        var cornerOffset = circleOffset - new WDir(halfExtentX, halfExtentZ);

        // center is inside rectangle
        if (cornerOffset.X <= 0 && cornerOffset.Z <= 0)
        {
            var distToXEdge = -cornerOffset.X;
            var distToZEdge = -cornerOffset.Z;
            var distToNearestEdge = MathF.Min(distToXEdge, distToZEdge);

            return circleRadius >= distToNearestEdge;
        }

        // too far outside to touch
        if (cornerOffset.X > circleRadius || cornerOffset.Z > circleRadius)
            return false;

        // outside along only one axis
        if (cornerOffset.X <= 0 || cornerOffset.Z <= 0)
            return true;

        // outside near a corner
        return cornerOffset.LengthSq() <= circleRadius * circleRadius;
    }

    public static bool CircleDonutSector(in WDir circleOffset, float circleRadius, float innerRadius, float outerRadius, WDir sectorDir, Angle halfAngle)
    {
        var distSq = circleOffset.LengthSq();
        var maxR = outerRadius + circleRadius;
        var minR = Math.Max(0, innerRadius - circleRadius);

        if (distSq > maxR * maxR || distSq < minR * minR)
        {
            return false;
        }

        if (halfAngle.Rad >= MathF.PI)
        {
            return true;
        }

        // Ensure sectorDir is normalized
        sectorDir = sectorDir.Normalized();

        // angle to center
        var angleToCenter = Angle.Acos(Math.Clamp(circleOffset.Normalized().Dot(sectorDir), -1f, 1f));
        if (angleToCenter <= halfAngle)
        {
            return true;
        }

        // sample side arcs: left/right boundary rays of sector
        var sideDirL = sectorDir.Rotate(halfAngle);
        var sideDirR = sectorDir.Rotate(-halfAngle);

        static float DistToRay(WDir dir, WDir pt)
            // vector rejection = cross product length / length of ray dir (==1)
            => Math.Abs(pt.Cross(dir));

        // check if circle intersects side rays
        var dL = DistToRay(sideDirL, circleOffset);
        var dR = DistToRay(sideDirR, circleOffset);
        var projL = circleOffset.Dot(sideDirL);
        var projR = circleOffset.Dot(sideDirR);

        if (projL >= 0 && projL <= outerRadius && dL <= circleRadius ||
            projR >= 0 && projR <= outerRadius && dR <= circleRadius)
        {
            return true;
        }

        // check corners
        var cornerL = sideDirL * outerRadius;
        var cornerR = sideDirR * outerRadius;

        return (circleOffset - cornerL).LengthSq() <= circleRadius * circleRadius || (circleOffset - cornerR).LengthSq() <= circleRadius * circleRadius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleDonutSector(in WPos circleCenter, float circleRadius, in WPos sectorCenter, float innerRadius, float outerRadius, in WDir sectorDir, Angle halfAngle)
    => CircleDonutSector(circleCenter - sectorCenter, circleRadius, innerRadius, outerRadius, sectorDir, halfAngle);

    public static int RayCircleAnglesDeg(WPos centerC, float radius, WPos rayOriginO, WDir rayDirD, out float degEnter, out float degExit)
    {
        degEnter = degExit = default;

        // shift to circle space: F = O - C
        var F = rayOriginO - centerC;

        // Quadratic coefficients for |F + tD|^2 = r^2
        var A = rayDirD.Dot(rayDirD);
        var B = 2f * F.Dot(rayDirD);
        var C = F.Dot(F) - radius * radius;

        var disc = B * B - 4f * A * C;
        if (disc < 0f)
        {
            return 0; // no intersection
        }

        var s = MathF.Sqrt(Math.Max(0f, disc));
        var inv2A = 0.5f / A;
        var t0 = (-B - s) * inv2A; // entry (closest)
        var t1 = (-B + s) * inv2A; // exit  (farthest)

        // all forward intersections (t >= 0)
        var t0Ok = t0 >= 0f;
        var t1Ok = t1 >= 0f;

        if (!t0Ok && !t1Ok)
        {
            return 0; // both behind origin
        }

        // Compute angle for a hit: from center to hit point
        static float HitDeg(WPos C, WPos O, WDir D, float t)
        {
            var p = O + t * D;
            var v = p - C; // vector from center to hit
            var a = Angle.Atan2(v.X, v.Z).Normalized();
            return a.Deg;
        }

        if (disc == 0f)
        {
            // Tangent: one forward hit (might be at t=0 if starting on the boundary)
            var t = Math.Max(t0, t1); // both equal; ensures non-negative if one is 0
            if (t < 0f)
            {
                return default;
            }
            degEnter = HitDeg(centerC, rayOriginO, rayDirD, t);
            return 1;
        }

        // Two solutions; keep only those in front
        if (t0Ok && t1Ok)
        {
            degEnter = HitDeg(centerC, rayOriginO, rayDirD, MathF.Min(t0, t1)); // entry
            degExit = HitDeg(centerC, rayOriginO, rayDirD, MathF.Max(t0, t1)); // exit
            return 2;
        }
        else
        {
            // Ray starts inside the circle: only the exit is valid
            var t = Math.Max(t0, t1);
            degExit = HitDeg(centerC, rayOriginO, rayDirD, t);
            return 1;
        }
    }
}
