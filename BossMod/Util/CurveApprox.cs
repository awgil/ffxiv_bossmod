namespace BossMod;

// a bunch of utilities for approximating curves with line segments
// we need them, since clipping and rendering works with polygons
public static class CurveApprox
{
    public const float ScreenError = 0.1f; // typical maximal screen-space error; tradeoff between performance and fidelity

    public static int CalculateCircleSegments(float radius, Angle angularLength, float maxError)
    {
        // select max angle such that tesselation error is smaller than desired
        // error = R * (1 - cos(phi/2)) => cos(phi/2) = 1 - error/R
        float tessAngle = 2 * MathF.Acos(1 - MathF.Min(maxError / radius, 1));
        int tessNumSegments = (int)MathF.Ceiling(angularLength.Rad / tessAngle);
        tessNumSegments = (tessNumSegments + 1) & ~1; // round up to even for symmetry
        return Math.Clamp(tessNumSegments, 4, 512);
    }

    // return polygon points approximating full circle; implicitly closed path - last point is not included
    // winding: points are in CCW order
    public static IEnumerable<WDir> Circle(float radius, float maxError)
    {
        int numSegments = CalculateCircleSegments(radius, (2 * MathF.PI).Radians(), maxError);
        var angle = (2 * MathF.PI / numSegments).Radians();
        for (int i = 0; i < numSegments; ++i) // note: do not include last point
            yield return PolarToCartesian(radius, i * angle);
    }
    public static IEnumerable<WPos> Circle(WPos center, float radius, float maxError) => Circle(radius, maxError).Select(off => center + off);

    // return polygon points approximating circle arc; both start and end points are included
    // winding: points are either in CCW order (if length is positive) or CW order (if length is negative)
    public static IEnumerable<WDir> CircleArc(float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        int numSegments = CalculateCircleSegments(radius, length.Abs(), maxError);
        var angle = length / numSegments;
        for (int i = 0; i <= numSegments; ++i)
            yield return PolarToCartesian(radius, angleStart + i * angle);
    }
    public static IEnumerable<WPos> CircleArc(WPos center, float radius, Angle angleStart, Angle angleEnd, float maxError) => CircleArc(radius, angleStart, angleEnd, maxError).Select(off => center + off);

    // return polygon points approximating circle sector; implicitly closed path - center + arc
    public static IEnumerable<WDir> CircleSector(float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        yield return default;
        foreach (var v in CircleArc(radius, angleStart, angleEnd, maxError))
            yield return v;
    }
    public static IEnumerable<WPos> CircleSector(WPos center, float radius, Angle angleStart, Angle angleEnd, float maxError) => CircleSector(radius, angleStart, angleEnd, maxError).Select(off => center + off);

    public static IEnumerable<WDir> Ellipse(float axis1, float axis2, float maxError)
    {
        int numSegments = CalculateCircleSegments((axis1 + axis2) / 2f, (2 * MathF.PI).Radians(), maxError);
        var angle = (2 * MathF.PI / numSegments).Radians();
        for (int i = 0; i < numSegments; ++i)
        {
            var t = i * angle;
            yield return new WDir(axis1 * t.Cos(), axis2 * t.Sin());
        }
    }
    public static IEnumerable<WPos> Ellipse(WPos center, float axis1, float axis2, float maxError) => Ellipse(axis1, axis2, maxError).Select(off => center + off);

    // return polygon points approximating full donut; implicitly closed path - outer arc + inner arc
    public static IEnumerable<WDir> Donut(float innerRadius, float outerRadius, float maxError)
    {
        foreach (var v in Circle(outerRadius, maxError))
            yield return v;
        yield return PolarToCartesian(outerRadius, 0.0f.Radians());
        yield return PolarToCartesian(innerRadius, 0.0f.Radians());
        foreach (var v in Circle(innerRadius, maxError).Reverse())
            yield return v;
    }
    public static IEnumerable<WPos> Donut(WPos center, float innerRadius, float outerRadius, float maxError) => Donut(innerRadius, outerRadius, maxError).Select(off => center + off);

    // return polygon points approximating donut sector; implicitly closed path - outer arc + inner arc
    public static IEnumerable<WDir> DonutSector(float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError)
    {
        foreach (var v in CircleArc(outerRadius, angleStart, angleEnd, maxError))
            yield return v;
        foreach (var v in CircleArc(innerRadius, angleEnd, angleStart, maxError))
            yield return v;
    }
    public static IEnumerable<WPos> DonutSector(WPos center, float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError) => DonutSector(innerRadius, outerRadius, angleStart, angleEnd, maxError).Select(off => center + off);

    // return polygon points for rectangle - it's not really a curve, but whatever...
    public static IEnumerable<WDir> Rect(WDir dx, WDir dz)
    {
        yield return dx - dz;
        yield return dx + dz;
        yield return -dx + dz;
        yield return -dx - dz;
    }
    public static IEnumerable<WDir> Rect(WDir dirZ, float halfWidth, float halfHeight) => Rect(dirZ.OrthoL() * halfWidth, dirZ * halfHeight);
    public static IEnumerable<WDir> Rect(WDir center, WDir dx, WDir dz) => Rect(dx, dz).Select(off => center + off);
    public static IEnumerable<WDir> Rect(WDir center, WDir dirZ, float halfWidth, float halfHeight) => Rect(center, dirZ.OrthoL() * halfWidth, dirZ * halfHeight);
    public static IEnumerable<WPos> Rect(WPos center, WDir dx, WDir dz) => Rect(dx, dz).Select(off => center + off);
    public static IEnumerable<WPos> Rect(WPos center, WDir dirZ, float halfWidth, float halfHeight) => Rect(center, dirZ.OrthoL() * halfWidth, dirZ * halfHeight);

    [Flags]
    public enum Corners : byte
    {
        None = 0,
        NE = 1,
        NW = 2,
        SW = 4,
        SE = 8,

        All = NW | NE | SE | SW
    }

    public static IEnumerable<WDir> TruncatedRect(WDir dx, WDir dz, float size, Corners corners)
    {
        if (corners.HasFlag(Corners.NE))
        {
            yield return dx - dz - new WDir(size, 0);
            yield return dx - dz + new WDir(0, size);
        }
        else
            yield return dx - dz;

        if (corners.HasFlag(Corners.SE))
        {
            yield return dx + dz - new WDir(0, size);
            yield return dx + dz - new WDir(size, 0);
        }
        else
            yield return dx + dz;

        if (corners.HasFlag(Corners.SW))
        {
            yield return -dx + dz + new WDir(size, 0);
            yield return -dx + dz - new WDir(0, size);
        }
        else
            yield return -dx + dz;

        if (corners.HasFlag(Corners.NW))
        {
            yield return -dx - dz + new WDir(0, size);
            yield return -dx - dz + new WDir(size, 0);
        }
        else
            yield return -dx - dz;
    }

    public static IEnumerable<WDir> TruncatedRect(WDir dirZ, float halfWidth, float halfHeight, float size, Corners corners) => TruncatedRect(dirZ.OrthoL() * halfWidth, dirZ * halfHeight, size, corners);
    public static IEnumerable<WDir> TruncatedRect(WDir center, WDir dx, WDir dz, float size, Corners corners) => TruncatedRect(dx, dz, size, corners).Select(off => center + off);

    // for angles, we use standard FF convention: 0 is 'south'/down/(0, -r), and then increases clockwise
    private static WDir PolarToCartesian(float r, Angle phi) => r * phi.ToDirection();
}
