namespace BossMod;

// a bunch of utilities for approximating curves with line segments
// we need them, since clipping and rendering works with polygons
[SkipLocalsInit]
public static class CurveApprox
{
    public const float ScreenError = 0.05f;

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
    public static WDir[] Circle(float Radius, float maxError)
    {
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, 360f.Degrees(), maxError);
        var angleIncrement = (Angle.DoublePI / numSegments).Radians();
        var points = new WDir[numSegments];
        for (var i = 0; i < numSegments; ++i) // note: do not include last point
        {
            points[i] = PolarToCartesian(radius, i * angleIncrement);
        }
        return points;
    }

    public static WDir[] CircleArc(float Radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, length.Abs(), maxError);
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
    public static WDir[] CircleSector(float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var arcPoints = CircleArc(radius, angleStart, angleEnd, maxError);
        var length = arcPoints.Length;
        var points = new WDir[length + 1];
        points[0] = default;
        Array.Copy(arcPoints, 0, points, 1, length);
        return points;
    }

    // return polygon points approximating full donut; implicitly closed path - outer arc + inner arc
    public static WDir[] Donut(float innerRadius, float outerRadius, float maxError)
    {
        var outerCircle = Circle(outerRadius, maxError);
        var innerCircle = Circle(innerRadius, maxError);
        var outerLength = outerCircle.Length;
        var innerLength = innerCircle.Length;
        var points = new WDir[outerLength + innerLength + 2];

        for (var i = 0; i < outerLength; ++i)
        {
            points[i] = outerCircle[i];
        }

        points[outerLength] = PolarToCartesian(outerRadius, default);
        points[outerLength + 1] = PolarToCartesian(innerRadius, default);
        var index = outerLength + 2;
        var innerAdj = innerLength - 1;
        for (var i = innerAdj; i >= 0; --i)
        {
            points[index++] = innerCircle[i];
        }

        return points;
    }

    // return polygon points approximating donut sector; implicitly closed path - outer arc + inner arc
    public static WDir[] DonutSector(float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var outerArc = CircleArc(outerRadius, angleStart, angleEnd, maxError);
        var innerArc = CircleArc(innerRadius, angleEnd, angleStart, maxError);
        var outerLength = outerArc.Length;
        var innerLength = innerArc.Length;
        var totalPoints = outerLength + innerLength;
        var points = new WDir[totalPoints];

        for (var i = 0; i < outerLength; ++i)
        {
            points[i] = outerArc[i];
        }

        for (var i = 0; i < innerLength; ++i)
        {
            points[outerLength + i] = innerArc[i];
        }

        return points;
    }

    // return polygon points for rectangle - it's not really a curve, but whatever...
    public static WDir[] Rect(WDir dx, WDir dz) => [
            dx - dz,
            dx + dz,
            -dx + dz,
            -dx - dz
        ];

    public static WDir[] Rect(WDir dirZ, float halfWidth, float halfHeight)
    {
        var dx = dirZ.OrthoL() * halfWidth;
        var dz = dirZ * halfHeight;
        return Rect(dx, dz);
    }

    // for angles, we use standard FF convention: 0 is 'south'/down/(0, -r), and then increases clockwise
    private static WDir PolarToCartesian(float r, Angle phi) => r * phi.ToDirection();

    public static List<WDir> Capsule(WDir centerOffset, WDir dir, float length, float radius, float maxError)
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
