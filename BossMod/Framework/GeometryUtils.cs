using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    class GeometryUtils
    {
        public static List<Vector2> ClipPolygonToPolygon(IEnumerable<Vector2> pts, IEnumerable<Vector2> clipPoly)
        {
            // use Clipper library (Vatti algorithm)
            ClipperLib.Clipper c = new(ClipperLib.Clipper.ioPreserveCollinear);
            List<ClipperLib.IntPoint> ipts = new();

            foreach (var p in clipPoly)
                ipts.Add(new(p.X * 1000, p.Y * 1000));
            c.AddPath(ipts, ClipperLib.PolyType.ptClip, true);
            ipts.Clear();
            foreach (var p in pts)
                ipts.Add(new(p.X * 1000, p.Y * 1000));
            c.AddPath(ipts, ClipperLib.PolyType.ptSubject, true);

            List<List<ClipperLib.IntPoint>> solution = new();
            c.Execute(ClipperLib.ClipType.ctIntersection, solution);

            // TODO: improve this...
            Func<ClipperLib.IntPoint, Vector2> cvtPt = (ipt) => new Vector2(ipt.X / 1000.0f, ipt.Y / 1000.0f);
            List<Vector2> res = new();
            if (solution.Count > 0)
                foreach (var p in solution[0])
                    res.Add(cvtPt(p));
            if (solution.Count > 1)
            {
                var testPoint = cvtPt(solution[0][0]);
                res.Add(testPoint);

                int closest = 0;
                float closestDist = (cvtPt(solution[1][0]) - testPoint).LengthSquared();
                for (int i = 1; i < solution[1].Count; ++i)
                {
                    float dist = (cvtPt(solution[1][i]) - testPoint).LengthSquared();
                    if (dist < closestDist)
                    {
                        closest = i;
                        closestDist = dist;
                    }
                }

                for (int i = closest; i < solution[1].Count; ++i)
                    res.Add(cvtPt(solution[1][i]));
                for (int i = 0; i <= closest; ++i)
                    res.Add(cvtPt(solution[1][i]));
            }
            return res;
        }

        // note: startAngle assumed to be in [-pi, pi), length in (0, pi]
        public static List<Vector2> BuildTesselatedConvexCone(Vector2 center, float radius, float startAngle, float length)
        {
            List<Vector2> tesselated = new();
            if (length < MathF.PI)
                tesselated.Add(center);
            int tessNumSegments = CalculateTesselationSegments(radius, length);
            for (int i = 0; i <= tessNumSegments; ++i) // note: include last point
                tesselated.Add(PolarToCartesian(center, radius, startAngle + (float)i / (float)tessNumSegments * length));
            return tesselated;
        }

        public static List<Vector2> BuildTesselatedCircle(Vector2 center, float radius)
        {
            List<Vector2> tesselated = new();
            int tessNumSegments = CalculateTesselationSegments(radius, 2 * MathF.PI);
            for (int i = 0; i < tessNumSegments; ++i) // note: do not include last point
                tesselated.Add(PolarToCartesian(center, radius, (float)i / (float)tessNumSegments * 2 * MathF.PI));
            return tesselated;
        }

        public static int CalculateTesselationSegments(float radius, float angularLength)
        {
            // select max angle such that tesselation error is smaller than desired
            // error = R * (1 - cos(phi/2)) => cos(phi/2) = 1 - error/R ~= 1 - (phi/2)^2 / 2 => phi ~= sqrt(8*error/R)
            float tessAngle = MathF.Sqrt(2f / radius); //MathF.Acos(1 - MathF.Min(0.3f / radius, 1));
            int tessNumSegments = (int)MathF.Ceiling(angularLength / tessAngle);
            tessNumSegments = ((tessNumSegments + 1) / 2) * 2; // round up to even for symmetry
            return Math.Clamp(tessNumSegments, 4, 512);
        }

        // phi=0 corresponds to (r, 0), then phi increases counterclockwise - so phi=pi/2 corresponds to (0, -r)
        public static Vector2 PolarToCartesian(Vector2 center, float r, float phi)
        {
            return center + r * new Vector2(MathF.Cos(phi), -MathF.Sin(phi));
        }

        public static Vector3 DirectionToVec3(float direction)
        {
            return new(MathF.Sin(direction), 0, MathF.Cos(direction));
        }

        // zone checking
        public static bool PointInRect(Vector3 offsetFromOrigin, Vector3 direction, float lenFront, float lenBack, float halfWidth)
        {
            var normal = new Vector3(-direction.Z, 0, direction.X);
            var dotDir = Vector3.Dot(offsetFromOrigin, direction);
            var dotNormal = Vector3.Dot(offsetFromOrigin, normal);
            return dotDir >= -lenBack && dotDir <= lenFront && MathF.Abs(dotNormal) <= halfWidth;
        }

        public static bool PointInRect(Vector3 offsetFromOrigin, float direction, float lenFront, float lenBack, float halfWidth)
        {
            return PointInRect(offsetFromOrigin, DirectionToVec3(direction), lenFront, lenBack, halfWidth);
        }

        public static bool PointInCircle(Vector3 offsetFromOrigin, float radius)
        {
            return offsetFromOrigin.LengthSquared() <= radius * radius;
        }

        public static bool PointInCone(Vector3 offsetFromOrigin, Vector3 direction, float halfAngle)
        {
            return Vector3.Dot(Vector3.Normalize(offsetFromOrigin), direction) >= MathF.Cos(halfAngle);
        }

        public static bool PointInCone(Vector3 offsetFromOrigin, float direction, float halfAngle)
        {
            return PointInCone(offsetFromOrigin, DirectionToVec3(direction), halfAngle);
        }
    }
}
