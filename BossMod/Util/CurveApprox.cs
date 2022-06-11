using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // a bunch of utilities for approximating curves with line segments
    // we need them, since clipping and rendering works with polygons
    public static class CurveApprox
    {
        public const float ScreenError = 0.3f; // typical maximal screen-space error; tradeoff between performance and fidelity

        public static int CalculateCircleSegments(float radius, float angularLength, float maxError)
        {
            // select max angle such that tesselation error is smaller than desired
            // error = R * (1 - cos(phi/2)) => cos(phi/2) = 1 - error/R
            float tessAngle = 2 * MathF.Acos(1 - MathF.Min(maxError / radius, 1));
            int tessNumSegments = (int)MathF.Ceiling(angularLength / tessAngle);
            tessNumSegments = (tessNumSegments + 1) & ~1; // round up to even for symmetry
            return Math.Clamp(tessNumSegments, 4, 512);
        }

        // return polygon points approximating full circle; implicitly closed path - last point is not included
        // winding: points are in CCW order
        public static IEnumerable<Vector2> Circle(Vector2 center, float radius, float maxError)
        {
            int numSegments = CalculateCircleSegments(radius, 2 * MathF.PI, maxError);
            float angle = 2 * MathF.PI / numSegments;
            for (int i = 0; i < numSegments; ++i) // note: do not include last point
                yield return PolarToCartesian(center, radius, i * angle);
        }

        // return polygon points approximating circle arc; both start and end points are included
        // winding: points are either in CCW order (if length is positive) or CW order (if length is negative)
        public static IEnumerable<Vector2> CircleArc(Vector2 center, float radius, float angleStart, float angleEnd, float maxError)
        {
            float length = angleEnd - angleStart;
            int numSegments = CalculateCircleSegments(radius, Math.Abs(length), maxError);
            float angle = length / numSegments;
            for (int i = 0; i <= numSegments; ++i)
                yield return PolarToCartesian(center, radius, angleStart + i * angle);
        }

        // return polygon points approximating circle sector; implicitly closed path - center + arc
        public static IEnumerable<Vector2> CircleSector(Vector2 center, float radius, float angleStart, float angleEnd, float maxError)
        {
            yield return center;
            foreach (var v in CircleArc(center, radius, angleStart, angleEnd, maxError))
                yield return v;
        }

        // return polygon points approximating donut sector; implicitly closed path - outer arc + inner arc
        public static IEnumerable<Vector2> DonutSector(Vector2 center, float innerRadius, float outerRadius, float angleStart, float angleEnd, float maxError)
        {
            foreach (var v in CircleArc(center, outerRadius, angleStart, angleEnd, maxError))
                yield return v;
            foreach (var v in CircleArc(center, innerRadius, angleEnd, angleStart, maxError))
                yield return v;
        }

        // for angles, we use standard FF convention: 0 is 'south'/down/(0, -r), and then increases clockwise
        private static Vector2 PolarToCartesian(Vector2 center, float r, float phi)
        {
            return center + r * new Vector2(MathF.Sin(phi), MathF.Cos(phi));
        }
    }
}
