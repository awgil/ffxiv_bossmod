namespace BossMod;

// a bunch of utilities for approximating curves with line segments
// we need them, since clipping and rendering works with polygons
[SkipLocalsInit]
public static class CurveApprox
{
    public const float ScreenError = 0.05f;
    // for angles, we use standard FF convention: 0 is 'south'/down/(0, -r), and then increases clockwise

    public static int CalculateCircleSegments(float radius, Angle angularLength, float maxError)
    {
        // select max angle such that tesselation error is smaller than desired
        // error = R * (1 - cos(phi/2)) => cos(phi/2) = 1 - error/R
        var tessAngle = 2 * MathF.Acos(1 - Math.Min(maxError / radius, 1));
        var tessNumSegments = (int)MathF.Ceiling(angularLength.Rad / tessAngle);
        tessNumSegments = (tessNumSegments + 1) & ~1;
        return Math.Clamp(tessNumSegments, 4, 512);
    }

    // return polygon points approximating full circle; implicitly closed path - last point is not included
    // winding: points are in CCW order
    public static List<WDir> Circle(float Radius, float maxError)
    {
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, 360f.Degrees(), maxError);
        var angleIncrement = (Angle.DoublePI / numSegments).Radians();
        var points = new List<WDir>(numSegments);

        for (var i = 0; i < numSegments; ++i) // note: do not include last point
        {
            points.Add(radius * (i * angleIncrement).ToDirection());
        }
        return points;
    }

    public static WDir[] Circle(WDir centerOffset, float Radius, float maxError)
    {
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, 360f.Degrees(), maxError);
        var angleIncrement = (Angle.DoublePI / numSegments).Radians();
        var points = new WDir[numSegments];
        var centerO = centerOffset;
        for (var i = 0; i < numSegments; ++i) // note: do not include last point
        {
            points[i] = radius * (i * angleIncrement).ToDirection() + centerO;
        }
        return points;
    }

    public static List<WDir> CircleL(WDir centerOffset, float Radius, float maxError)
    {
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, 360f.Degrees(), maxError);
        var angleIncrement = (Angle.DoublePI / numSegments).Radians();
        var points = new List<WDir>(numSegments);
        var centerO = centerOffset;
        for (var i = 0; i < numSegments; ++i) // note: do not include last point
        {
            points.Add(radius * (i * angleIncrement).ToDirection() + centerO);
        }
        return points;
    }

    public static WDir[] CircleArc(float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var _radius = radius;
        var numSegments = CalculateCircleSegments(_radius, length.Abs(), maxError);
        var angleIncrement = length / numSegments;
        var points = new WDir[numSegments + 1];
        for (var i = 0; i <= numSegments; ++i)
        {
            var angle = angleStart + i * angleIncrement;
            points[i] = PolarToCartesian(radius, angle);
        }
        return points;
    }

    // return polygon points approximating circle sector; implicitly closed path - center + arc
    public static WDir[] CircleSector(WDir centerOffset, float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var _radius = radius;
        var numSegments = CalculateCircleSegments(_radius, length.Abs(), maxError);
        var angleIncrement = length / numSegments;
        var points = new WDir[numSegments + 2];
        var centerO = centerOffset;

        for (var i = 0; i <= numSegments; ++i)
        {
            points[i + 1] = _radius * (angleStart + i * angleIncrement).ToDirection() + centerO;
        }

        points[0] = centerO;
        return points;
    }

    public static List<WDir> CircleSectorL(WDir centerOffset, float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var _radius = radius;
        var numSegments = CalculateCircleSegments(_radius, length.Abs(), maxError);
        var angleIncrement = length / numSegments;
        var points = new List<WDir>(numSegments + 2);
        var centerO = centerOffset;

        points.Add(centerO);
        for (var i = 0; i <= numSegments; ++i)
        {
            points.Add(_radius * (angleStart + i * angleIncrement).ToDirection() + centerO);
        }

        return points;
    }

    // return polygon points approximating full donut; implicitly closed path - outer arc + inner arc
    public static WDir[] Donut(WDir centerOffset, float innerRadius, float outerRadius, float maxError)
    {
        var radiusO = outerRadius;
        var radiusI = innerRadius;
        var a360 = 360f.Degrees();
        var numSegmentsO = CalculateCircleSegments(radiusO, a360, maxError);
        var numSegmentsI = CalculateCircleSegments(radiusI, a360, maxError);
        var angleIncrementO = (Angle.DoublePI / numSegmentsO).Radians();
        var points = new WDir[numSegmentsO + numSegmentsI + 2];
        var centerO = centerOffset;

        for (var i = 0; i < numSegmentsO; ++i) // note: do not include last point
        {
            points[i] = radiusO * (i * angleIncrementO).ToDirection() + centerO;
        }

        var v1 = new WDir(0f, 1f);
        points[numSegmentsO] = radiusO * v1 + centerO;
        points[numSegmentsO + 1] = radiusI * v1 + centerO;

        var index = numSegmentsO + 2;
        var innerAdj = numSegmentsI - 1;
        var angleIncrementI = (Angle.DoublePI / numSegmentsI).Radians();
        for (var i = innerAdj; i >= 0; --i)
        {
            points[index++] = radiusI * (i * angleIncrementI).ToDirection() + centerO;
        }

        return points;
    }

    public static List<WDir> DonutL(WDir centerOffset, float innerRadius, float outerRadius, float maxError)
    {
        var radiusO = outerRadius;
        var radiusI = innerRadius;
        var a360 = 360f.Degrees();
        var numSegmentsO = CalculateCircleSegments(radiusO, a360, maxError);
        var numSegmentsI = CalculateCircleSegments(radiusI, a360, maxError);
        var angleIncrementO = (Angle.DoublePI / numSegmentsO).Radians();
        var points = new List<WDir>(numSegmentsO + numSegmentsI + 2);
        var centerO = centerOffset;

        for (var i = 0; i < numSegmentsO; ++i) // note: do not include last point
        {
            points.Add(radiusO * (i * angleIncrementO).ToDirection() + centerO);
        }

        var v1 = new WDir(0f, 1f);
        points.Add(radiusO * v1 + centerOffset);
        points.Add(radiusI * v1 + centerOffset);

        var innerAdj = numSegmentsI - 1;
        var angleIncrementI = (Angle.DoublePI / numSegmentsI).Radians();
        for (var i = innerAdj; i >= 0; --i)
        {
            points.Add(radiusI * (i * angleIncrementI).ToDirection() + centerO);
        }

        return points;
    }

    // return polygon points approximating donut sector; implicitly closed path - outer arc + inner arc
    public static WDir[] DonutSector(WDir centerOffset, float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var radiusO = outerRadius;
        var radiusI = innerRadius;
        var lenAbs = length.Abs();
        var numSegmentsO = CalculateCircleSegments(radiusO, lenAbs, maxError);
        var numSegmentsI = CalculateCircleSegments(radiusI, lenAbs, maxError);
        var angleIncrementO = length / numSegmentsO;
        var angleIncrementI = length / numSegmentsI;
        var points = new WDir[numSegmentsO + numSegmentsI + 2];
        var centerO = centerOffset;

        for (var i = 0; i <= numSegmentsO; ++i)
        {
            points[i] = radiusO * (angleStart + i * angleIncrementO).ToDirection() + centerO;
        }

        var adj = numSegmentsO + 1;
        for (var i = 0; i <= numSegmentsI; ++i)
        {
            points[adj + i] = radiusI * (angleEnd - i * angleIncrementI).ToDirection() + centerO;
        }
        return points;
    }

    public static List<WDir> DonutSectorL(WDir centerOffset, float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var radiusO = outerRadius;
        var radiusI = innerRadius;
        var lenAbs = length.Abs();
        var numSegmentsO = CalculateCircleSegments(radiusO, lenAbs, maxError);
        var numSegmentsI = CalculateCircleSegments(radiusI, lenAbs, maxError);
        var angleIncrementO = length / numSegmentsO;
        var angleIncrementI = length / numSegmentsI;
        var points = new List<WDir>(numSegmentsO + numSegmentsI + 2);
        var centerO = centerOffset;

        for (var i = 0; i <= numSegmentsO; ++i)
        {
            points.Add(radiusO * (angleStart + i * angleIncrementO).ToDirection() + centerO);
        }

        for (var i = 0; i <= numSegmentsI; ++i)
        {
            points.Add(radiusI * (angleEnd - i * angleIncrementI).ToDirection() + centerO);
        }
        return points;
    }

    private static WDir PolarToCartesian(float r, Angle phi) => r * phi.ToDirection();

    public static WDir[] Capsule(WDir centerOffset, WDir dir, float length, float radius, float maxError)
    {
        dir = dir.Normalized();

        var p0 = default(WDir);
        var p1 = length * dir;
        var offset = centerOffset;
        var angleDir = Angle.FromDirection(dir);
        var a90 = 90f.Degrees();

        // Start at the +OrthoL side, go around the forward end to -OrthoL.
        var p1AngleStart = angleDir + a90;
        var lengthP1 = angleDir - a90 - p1AngleStart;
        var _radius = radius;
        var numSegments = CalculateCircleSegments(_radius, lengthP1.Abs(), maxError);
        var angleIncrement = lengthP1 / numSegments;
        var numSegmentAdj = numSegments + 1;
        var points = new WDir[numSegmentAdj * 2];
        for (var i = 0; i <= numSegments; ++i)
        {
            var angle = p1AngleStart + i * angleIncrement;
            points[i] = p1 + radius * angle.ToDirection() + offset;
        }

        // Start at the -OrthoL side, go around the rear end to +OrthoL.
        var p2AngleStart = angleDir - a90;

        for (var i = 0; i <= numSegments; ++i)
        {
            var angle = p2AngleStart + i * angleIncrement;
            points[i + numSegmentAdj] = p0 + radius * angle.ToDirection() + offset;
        }

        return points;
    }

    public static List<WDir> CapsuleL(WDir centerOffset, WDir dir, float length, float radius, float maxError)
    {
        dir = dir.Normalized();

        var p0 = default(WDir);
        var p1 = length * dir;
        var offset = centerOffset;
        var angleDir = Angle.FromDirection(dir);
        var a90 = 90f.Degrees();

        // Start at the +OrthoL side, go around the forward end to -OrthoL.
        var p1AngleStart = angleDir + a90;
        var lengthP1 = angleDir - a90 - p1AngleStart;
        var _radius = radius;
        var numSegments = CalculateCircleSegments(_radius, lengthP1.Abs(), maxError);
        var angleIncrement = lengthP1 / numSegments;
        var points = new List<WDir>((numSegments + 1) * 2);
        for (var i = 0; i <= numSegments; ++i)
        {
            var angle = p1AngleStart + i * angleIncrement;
            points.Add(p1 + radius * angle.ToDirection() + offset);
        }

        // Start at the -OrthoL side, go around the rear end to +OrthoL.
        var p2AngleStart = angleDir - a90;

        for (var i = 0; i <= numSegments; ++i)
        {
            var angle = p2AngleStart + i * angleIncrement;
            points.Add(p0 + radius * angle.ToDirection() + offset);
        }

        return points;
    }

    public static WDir[] ArcCapsule(WDir toOrbitCenter, Angle angularLength, float radius, float maxError)
    {
        var C = toOrbitCenter;
        var R = C.Length();

        var outerR = R + radius;
        var innerR = R - radius;

        var theta0 = Angle.FromDirection(-C); // orbitCenter -> start
        var theta1 = theta0 + angularLength;
        var a90 = 90f.Degrees();

        var s = Math.Sign(angularLength.Rad);
        if (s == 0)
        {
            s = 1;
        }

        // segment counts
        var lenAbs = angularLength.Abs();
        var nOut = CalculateCircleSegments(outerR, lenAbs, maxError);
        var n = CalculateCircleSegments(innerR, lenAbs, maxError);
        var nCap = CalculateCircleSegments(radius, 180f.Degrees(), maxError);

        // total vertices (we keep joint duplicates)
        var total = nOut + nCap + n + nCap + 4;
        var pts = new WDir[total];

        // outer
        var idx = 0;
        idx = WriteArc(pts, idx, C, outerR, theta0, theta1, nOut);

        // end cap
        var p1 = C + PolarToCartesian(R, theta1);
        var t1 = theta1 + (s > 0 ? a90 : -a90);
        idx = WriteArc(pts, idx, p1, radius, t1 - a90, t1 + a90, nCap);

        // inner
        idx = WriteArc(pts, idx, C, innerR, theta1, theta0, n);

        // start cap
        var t0 = theta0 + (s < 0 ? a90 : -a90);
        WriteArc(pts, idx, default, radius, t0 - a90, t0 + a90, nCap);

        return pts;

        static int WriteArc(WDir[] dst, int startIndex, WDir center, float radius, Angle a0, Angle a1, int segments)
        {
            var inc = (a1 - a0) / segments;
            var k = startIndex;
            for (var i = 0; i <= segments; ++i)
            {
                var a = a0 + i * inc;
                dst[k++] = center + PolarToCartesian(radius, a);
            }
            return k;
        }
    }
}
