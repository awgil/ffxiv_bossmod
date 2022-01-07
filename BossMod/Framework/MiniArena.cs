using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    public class MiniArena
    {
        public Vector3 WorldCenter = new(100, 0, 100);
        public float WorldHalfSize = 20;
        public float ScreenHalfSize = 150;
        public float ScreenMarginSize = 20;

        // these are set at the beginning of each draw
        private float _cameraAzimuth = 0;
        private float _cameraSinAzimuth = 0;
        private float _cameraCosAzimuth = 1;
        private Vector2 _screenCenterPos = new();

        // useful points
        public Vector3 WorldN  => WorldCenter + new Vector3(             0, 0, -WorldHalfSize);
        public Vector3 WorldNE => WorldCenter + new Vector3(+WorldHalfSize, 0, -WorldHalfSize);
        public Vector3 WorldE  => WorldCenter + new Vector3(+WorldHalfSize, 0, 0);
        public Vector3 WorldSE => WorldCenter + new Vector3(+WorldHalfSize, 0, +WorldHalfSize);
        public Vector3 WorldS  => WorldCenter + new Vector3(             0, 0, +WorldHalfSize);
        public Vector3 WorldSW => WorldCenter + new Vector3(-WorldHalfSize, 0, +WorldHalfSize);
        public Vector3 WorldW  => WorldCenter + new Vector3(-WorldHalfSize, 0, 0);
        public Vector3 WorldNW => WorldCenter + new Vector3(-WorldHalfSize, 0, -WorldHalfSize);

        // common color constants (ABGR)
        public uint ColorBorder = 0xffffffff;
        public uint ColorBackground = 0xff000000;
        public uint ColorDanger = 0xff00ffff;
        public uint ColorSafe = 0xff00ff00;

        // clipping utilities
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

        public static List<Vector3> ClipPolygonToRect(IEnumerable<Vector3> pts, Vector3 tl, Vector3 br)
        {
            Vector3 tr = new(br.X, 0, tl.Z);
            Vector3 bl = new(tl.X, 0, br.Z);
            // Sutherland-Hodgman algorithm: clip polygon by each edge
            var pt1 = ClipPolygonToEdge(pts, tl, Vector3.UnitZ);
            var pt2 = ClipPolygonToEdge(pt1, tr, -Vector3.UnitX);
            var pt3 = ClipPolygonToEdge(pt2, br, -Vector3.UnitZ);
            var pt4 = ClipPolygonToEdge(pt3, bl, Vector3.UnitX);
            return pt4;
        }

        private static List<Vector3> ClipPolygonToEdge(IEnumerable<Vector3> pts, Vector3 vertex, Vector3 normal)
        {
            // single iteration of Sutherland-Hodgman algorithm's outer loop
            List<Vector3> res = new();
            var it = pts.GetEnumerator();
            if (!it.MoveNext())
                return res; // empty polygon

            var first = it.Current;
            var prev = first;
            while (it.MoveNext())
            {
                ClipPolygonVertexPairToEdge(res, prev, it.Current, vertex, normal);
                prev = it.Current;
            }
            ClipPolygonVertexPairToEdge(res, prev, first, vertex, normal);
            return res;
        }

        private static void ClipPolygonVertexPairToEdge(List<Vector3> res, Vector3 prev, Vector3 curr, Vector3 vertex, Vector3 normal)
        {
            // single iteration of Sutherland-Hodgman algorithm's inner loop
            var ea = prev - vertex;
            var eb = curr - vertex;
            float ean = Vector3.Dot(ea, normal);
            float ebn = Vector3.Dot(eb, normal);
            // intersection point P = A + t * AB is such that (P-E).n = 0 ==> EA.n + t * AB.n = 0
            // AB.n == 0 means that AB is parallel to the edge; for such edges we'll never calculate intersection points, since both A and B will be either inside or outside
            // otherwise t = -EA.n/AB.n, and P = A + t * AB
            Func<Vector3> intersection = () =>
            {
                var ab = curr - prev;
                float abn = Vector3.Dot(ab, normal);
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

        // prepare for drawing - set up internal state, clip rect etc.
        public void Begin(float cameraAzimuthRadians)
        {
            _cameraAzimuth = cameraAzimuthRadians;
            _cameraSinAzimuth = MathF.Sin(cameraAzimuthRadians);
            _cameraCosAzimuth = MathF.Cos(cameraAzimuthRadians);
            //var camWorld = SharpDX.Matrix.RotationYawPitchRoll(azimuth, altitude, 0);
            //camWorld.Row4 = camWorld.Row3 * WorldHalfSize;
            //camWorld.M44 = 1;
            //CameraView = SharpDX.Matrix.Invert(camWorld);

            var centerOffset = new Vector2(ScreenMarginSize + 1.5f * ScreenHalfSize);
            var fullSize = 2 * centerOffset;
            var cursor = ImGui.GetCursorScreenPos();
            ImGui.Dummy(fullSize);
            _screenCenterPos = cursor + centerOffset;
        }

        // if you are 100% sure your primitive does not need clipping, you can use drawlist api directly
        // this helper allows converting world-space coords to screen-space ones
        public Vector2 WorldPositionToScreenPosition(Vector3 world)
        {
            return _screenCenterPos + WorldOffsetToScreenOffset(world - WorldCenter);
            //var viewPos = SharpDX.Vector3.Transform(new SharpDX.Vector3(worldOffset.X, 0, worldOffset.Z), CameraView);
            //return ScreenHalfSize * new Vector2(viewPos.X / viewPos.Z, viewPos.Y / viewPos.Z);
            //return ScreenHalfSize * new Vector2(viewPos.X, viewPos.Y) / WorldHalfSize;
        }

        private Vector2 WorldOffsetToScreenOffset(Vector3 worldOffset)
        {
            var viewX = worldOffset.X * _cameraCosAzimuth - worldOffset.Z * _cameraSinAzimuth;
            var viewY = worldOffset.Z * _cameraCosAzimuth + worldOffset.X * _cameraSinAzimuth;
            return ScreenHalfSize * new Vector2((float)viewX, (float)viewY) / WorldHalfSize;
        }

        // unclipped primitive rendering that accept world-space positions; thin convenience wrappers around drawlist api
        public void AddLine(Vector3 a, Vector3 b, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().AddLine(WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), color, thickness);
        }

        public void AddTriangle(Vector3 p1, Vector3 p2, Vector3 p3, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().AddTriangle(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color, thickness);
        }

        public void AddTriangleFilled(Vector3 p1, Vector3 p2, Vector3 p3, uint color)
        {
            ImGui.GetWindowDrawList().AddTriangleFilled(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color);
        }

        public void AddQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().AddQuad(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), WorldPositionToScreenPosition(p4), color, thickness);
        }

        public void AddQuadFilled(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, uint color)
        {
            ImGui.GetWindowDrawList().AddQuadFilled(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), WorldPositionToScreenPosition(p4), color);
        }

        public void AddCircle(Vector3 center, float radius, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().AddCircle(WorldPositionToScreenPosition(center), radius / WorldHalfSize * ScreenHalfSize, color, 0, thickness);
        }

        public void AddCircleFilled(Vector3 center, float radius, uint color)
        {
            ImGui.GetWindowDrawList().AddCircleFilled(WorldPositionToScreenPosition(center), radius / WorldHalfSize * ScreenHalfSize, color);
        }

        // path api: add new point to path; this adds new edge from last added point, or defines first vertex if path is empty
        public void PathLineTo(Vector3 p)
        {
            ImGui.GetWindowDrawList().PathLineToMergeDuplicate(WorldPositionToScreenPosition(p));
        }

        // adds a bunch of points corresponding to arc - if path is non empty, this adds an edge from last point to first arc point
        // angle 0 is E here, TODO reconsider...
        public void PathArcTo(Vector3 center, float radius, float amin, float amax)
        {
            ImGui.GetWindowDrawList().PathArcTo(WorldPositionToScreenPosition(center), radius / WorldHalfSize * ScreenHalfSize, amin + _cameraAzimuth, amax + _cameraAzimuth);
        }

        public void PathStroke(bool closed, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().PathStroke(color, closed ? ImDrawFlags.Closed : ImDrawFlags.None, thickness);
        }

        public void PathFillConvex(uint color)
        {
            ImGui.GetWindowDrawList().PathFillConvex(color);
        }

        // clipped primitive rendering (TODO...)

        // high level utilities
        // draw arena border
        public void BorderSquare()
        {
            AddQuad(WorldNW, WorldNE, WorldSE, WorldSW, ColorBorder, 2);
        }

        public void BorderCircle()
        {
            AddCircle(WorldCenter, WorldHalfSize, ColorBorder, 2);
        }

        // draw actor representation (small circle)
        public void Actor(Vector3 position, uint color)
        {
            if (WorldInBounds(position))
                AddCircleFilled(position, 0.5f, color);
        }

        public void End()
        {
            //ImGui.GetWindowDrawList().PopClipRect();
        }

        private bool WorldOffsetInBounds(Vector3 offset)
        {
            return Math.Abs(offset.X) <= WorldHalfSize && Math.Abs(offset.Z) <= WorldHalfSize;
        }

        private bool WorldInBounds(Vector3 world)
        {
            return WorldOffsetInBounds(world - WorldCenter);
        }
    }
}
