using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public class GeometryUtils
    {
        public static List<List<Vector2>> ClipPolygonToPolygon(IEnumerable<Vector2> pts, IEnumerable<Vector2> clipPoly)
        {
            // use Clipper library (Vatti algorithm)
            ClipperLib.Clipper c = new(ClipperLib.Clipper.ioStrictlySimple);
            List<ClipperLib.IntPoint> ipts = new();

            foreach (var p in clipPoly)
                ipts.Add(new(p.X * 1000, p.Y * 1000));
            c.AddPath(ipts, ClipperLib.PolyType.ptClip, true);
            ipts.Clear();
            foreach (var p in pts)
                ipts.Add(new(p.X * 1000, p.Y * 1000));
            c.AddPath(ipts, ClipperLib.PolyType.ptSubject, true);

            ClipperLib.PolyTree solution = new();
            c.Execute(ClipperLib.ClipType.ctIntersection, solution);

            List<List<Vector2>> polys = new();
            AddPolysWithHoles(polys, solution);
            return polys;
        }

        private static void AddPolysWithHoles(List<List<Vector2>> result, ClipperLib.PolyNode node)
        {
            Func<ClipperLib.IntPoint, Vector2> cvtPt = (ipt) => new Vector2(ipt.X / 1000.0f, ipt.Y / 1000.0f);
            Func<ClipperLib.IntPoint, ClipperLib.IntPoint, long> intDist = (a, b) => {
                ClipperLib.IntPoint d = new(a.X - b.X, a.Y - b.Y);
                return d.X * d.X + d.Y * d.Y;
            };
            foreach (var outer in node.Childs)
            {
                if (outer.Contour.Count == 0 || outer.IsOpen)
                    continue;

                // TODO: this is not good...
                List<(int, int)> connPoints = new();
                for (int iHole = 0; iHole < outer.ChildCount; ++iHole)
                {
                    var hole = outer.Childs[iHole];
                    if (hole.Contour.Count == 0)
                        continue;

                    int closestOuter = 0;
                    long closestDist = intDist(hole.Contour[0], outer.Contour[0]);
                    for (int iTest = 0; iTest < outer.Contour.Count; ++iTest)
                    {
                        long dist = intDist(hole.Contour[0], outer.Contour[iTest]);
                        if (dist < closestDist)
                        {
                            closestOuter = iTest;
                            closestDist = dist;
                        }
                    }

                    connPoints.Add(new(iHole, closestOuter));
                }
                connPoints.Sort((l, r) => l.Item2.CompareTo(r.Item2));

                List<Vector2> poly = new();
                int iNextConn = 0;
                for (int iOuter = 0; iOuter < outer.Contour.Count; ++iOuter)
                {
                    poly.Add(cvtPt(outer.Contour[iOuter]));
                    if (iNextConn < connPoints.Count && connPoints[iNextConn].Item2 == iOuter)
                    {
                        var hole = outer.Childs[connPoints[iNextConn++].Item1];
                        foreach (var p in hole.Contour)
                            poly.Add(cvtPt(p));
                        poly.Add(cvtPt(hole.Contour[0]));
                        poly.Add(cvtPt(outer.Contour[iOuter]));
                    }
                }
                result.Add(poly);

                foreach (var hole in outer.Childs)
                    AddPolysWithHoles(result, hole);
            }
        }

        public static bool ClipLineToNearPlane(ref SharpDX.Vector3 a, ref SharpDX.Vector3 b, SharpDX.Matrix viewProj)
        {
            var n = viewProj.Column3; // near plane
            var an = SharpDX.Vector4.Dot(new(a, 1), n);
            var bn = SharpDX.Vector4.Dot(new(b, 1), n);
            if (an <= 0 && bn <= 0)
                return false;

            if (an < 0 || bn < 0)
            {
                var ab = b - a;
                var abn = SharpDX.Vector3.Dot(ab, new(n.X, n.Y, n.Z));
                var t = -an / abn;
                if (an < 0)
                    a = a + t * ab;
                else
                    b = a + t * ab;
            }
            return true;
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

        public static float DirectionFromVec3(Vector3 direction)
        {
            return MathF.Atan2(direction.X, direction.Z);
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
