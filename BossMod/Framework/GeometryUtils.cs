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
        public static bool ClipLineToRect(ref Vector3 a, ref Vector3 b, Vector3 min, Vector3 max)
        {
            // Liang-Barsky algorithm:
            // consider point on line: P = A + t * (B-A); it lies in clip square if Min[i] <= P[i] <= Max[i] for both coords (X and Z)
            // we can rewrite these inequalities as follows:
            // P[i] >= Min[i] ==> A[i] + t * AB[i] >= Min[i] ==> t * AB[i] >= Min[i] - A[i] ==> t * (-AB[i]) <= (A[i] - Min[i])
            // P[i] <= Max[i] ==> A[i] + t * AB[i] <= Max[i] ==> t * AB[i] <= Max[i] - A[i] ==> t * (+AB[i]) <= (Max[i] - A[i])
            // AB[i] == 0 means that line is parallel to non-i'th axis; in such case this equation is not satisfied for any t if either of the right-hand sides is < 0, or satisfied for any t otherwise
            var ab = b - a;
            var q1 = a - min;
            var q2 = max - a;
            if (ab.X == 0 && (q1.X < 0 || q2.X < 0))
                return false;
            if (ab.Z == 0 && (q1.Z < 0 || q2.Z < 0))
                return false;

            // AB[i] < 0 ==> t <= q1[i] / (-AB[i]) *and* t >= q2[i] / (+AB[i])
            // AB[i] > 0 ==> t >= q1[i] / (-AB[i]) *and* t <= q2[i] / (+AB[i])
            float tmin = 0;
            float tmax = 1;
            if (ab.X < 0)
            {
                tmax = MathF.Min(tmax, q1.X / -ab.X);
                tmin = MathF.Max(tmin, q2.X / +ab.X);
            }
            else if (ab.X > 0)
            {
                tmin = MathF.Max(tmin, q1.X / -ab.X);
                tmax = MathF.Min(tmax, q2.X / +ab.X);
            }
            if (ab.Z < 0)
            {
                tmax = MathF.Min(tmax, q1.Z / -ab.Z);
                tmin = MathF.Max(tmin, q2.Z / +ab.Z);
            }
            else if (ab.Z > 0)
            {
                tmin = MathF.Max(tmin, q1.Z / -ab.Z);
                tmax = MathF.Min(tmax, q2.Z / +ab.Z);
            }

            if (tmax < tmin)
                return false; // there is no such t that satisfies all inequalities, line is fully outside

            b = a + tmax * ab;
            a = a + tmin * ab;
            return true;
        }

        public static bool ClipLineToCircle(ref Vector3 a, ref Vector3 b, Vector3 center, float radius)
        {
            var oa = a - center;
            var ab = b - a;

            // consider point on line: P = A + t * AB; it intersects with square if (P-O)^2 = R^2 ==> (OA + t * AB)^2 = R^2 => t^2*AB^2 + 2t(OA.AB) + (OA^2 - R^2) = 0
            float abab = ab.X * ab.X + ab.Z * ab.Z;
            float oaab = oa.X * ab.X + oa.Z * ab.Z;
            float oaoa = oa.X * oa.X + oa.Z * oa.Z;
            float d4 = oaab * oaab - (oaoa - radius * radius) * abab; // == d / 4
            if (d4 <= 0)
                return false; // line is fully outside circle

            // t[1, 2] = tc +/- td
            float td = MathF.Sqrt(d4) / abab; // > 0, so -td < +td
            float tc = -oaab / abab;
            float t1 = tc - td;
            float t2 = tc + td;
            if (t1 > 1 || t2 < 0)
                return false; // line is fully outside circle (but would intersect, if extended to infinity)

            b = a + MathF.Min(1, t2) * ab;
            a = a + MathF.Max(0, t1) * ab;
            return true;
        }

        public static List<Vector2> ClipPolygonToPolygon(IEnumerable<Vector2> pts, IEnumerable<Vector2> clipPoly)
        {
            // Sutherland-Hodgman algorithm: clip polygon by each edge
            var it = clipPoly.GetEnumerator();
            if (!it.MoveNext())
                return new(); // empty clip poly

            var first = it.Current;
            var prev = first;
            IEnumerable<Vector2> input = pts;
            while (it.MoveNext())
            {
                var res = ClipPolygonToEdge(input, prev, it.Current);
                input = res;
                prev = it.Current;
            }
            return ClipPolygonToEdge(input, prev, first);
        }

        private static List<Vector2> ClipPolygonToEdge(IEnumerable<Vector2> pts, Vector2 vertexStart, Vector2 vertexEnd)
        {
            // single iteration of Sutherland-Hodgman algorithm's outer loop
            List<Vector2> res = new();
            var it = pts.GetEnumerator();
            if (!it.MoveNext())
                return res; // empty polygon

            var dir = vertexEnd - vertexStart;
            var normal = new Vector2(-dir.Y, dir.X); // not normalized, but that's ok

            var first = it.Current;
            var prev = first;
            while (it.MoveNext())
            {
                ClipPolygonVertexPairToEdge(res, prev, it.Current, vertexStart, normal);
                prev = it.Current;
            }
            ClipPolygonVertexPairToEdge(res, prev, first, vertexStart, normal);
            return res;
        }

        private static void ClipPolygonVertexPairToEdge(List<Vector2> res, Vector2 prev, Vector2 curr, Vector2 vertex, Vector2 normal)
        {
            // single iteration of Sutherland-Hodgman algorithm's inner loop
            var ea = prev - vertex;
            var eb = curr - vertex;
            float ean = Vector2.Dot(ea, normal);
            float ebn = Vector2.Dot(eb, normal);
            // intersection point P = A + t * AB is such that (P-E).n = 0 ==> EA.n + t * AB.n = 0
            // AB.n == 0 means that AB is parallel to the edge; for such edges we'll never calculate intersection points, since both A and B will be either inside or outside
            // otherwise t = -EA.n/AB.n, and P = A + t * AB
            Func<Vector2> intersection = () =>
            {
                var ab = curr - prev;
                float abn = Vector2.Dot(ab, normal);
                float t = -ean / abn;
                return prev + t * ab;
            };
            if (ebn >= 0)
            {
                // curr is 'inside' edge
                if (ean < 0)
                {
                    // but prev is not
                    res.Add(intersection());
                }
                res.Add(curr);
            }
            else if (ean >= 0)
            {
                res.Add(intersection());
            }
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
            float tessAngle = MathF.Sqrt(0.5f / radius); //MathF.Acos(1 - MathF.Min(0.3f / radius, 1));
            int tessNumSegments = (int)MathF.Ceiling(angularLength / tessAngle);
            tessNumSegments = ((tessNumSegments + 1) / 2) * 2; // round up to even for symmetry
            return Math.Clamp(tessNumSegments, 4, 512);
        }

        public static Vector2 PolarToCartesian(Vector2 center, float r, float phi)
        {
            return center + r * new Vector2(MathF.Cos(phi), MathF.Sin(phi));
        }
    }
}
