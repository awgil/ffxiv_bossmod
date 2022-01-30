using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // note on coordinate systems:
    // - world coordinates - X points West to East, Z points North to South - so SE is corner with both maximal coords, NW is corner with both minimal coords
    //                       rotation 0 corresponds to South, and increases counterclockwise (so East is +pi/2, North is pi, West is -pi/2)
    // - camera azimuth 0 correpsonds to camera looking North and increases counterclockwise
    // - screen coordinates - X points left to right, Y points top to bottom
    public class MiniArena
    {
        public bool IsCircle = false;
        public Vector3 WorldCenter = new(100, 0, 100);
        public float WorldHalfSize = 20;
        public float ScreenHalfSize = 150;
        public float ScreenMarginSize = 20;

        // these are set at the beginning of each draw
        public Vector2 ScreenCenter { get; private set; } = new();
        private float _cameraAzimuth = 0;
        private float _cameraSinAzimuth = 0;
        private float _cameraCosAzimuth = 1;
        private List<Vector2> _clipPolygon = new();

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
        public uint ColorAOE = 0x80008080;
        public uint ColorDanger = 0xff00ffff;
        public uint ColorSafe = 0xff00ff00;
        public uint ColorPC = 0xff00ff00;
        public uint ColorEnemy = 0xff0000ff;
        public uint ColorPlayerInteresting = 0xffc0c0c0;
        public uint ColorPlayerGeneric = 0xff808080;

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
            ScreenCenter = cursor + centerOffset;

            if (IsCircle)
            {
                _clipPolygon = GeometryUtils.BuildTesselatedCircle(ScreenCenter, ScreenHalfSize);
                _clipPolygon.Reverse();
            }
            else
            {
                _clipPolygon.Clear();
                _clipPolygon.Add(WorldPositionToScreenPosition(WorldNW));
                _clipPolygon.Add(WorldPositionToScreenPosition(WorldNE));
                _clipPolygon.Add(WorldPositionToScreenPosition(WorldSE));
                _clipPolygon.Add(WorldPositionToScreenPosition(WorldSW));
            }

            ImGui.GetWindowDrawList().PushClipRect(cursor, cursor + fullSize);
        }

        // if you are 100% sure your primitive does not need clipping, you can use drawlist api directly
        // this helper allows converting world-space coords to screen-space ones
        public Vector2 WorldPositionToScreenPosition(Vector3 world)
        {
            return ScreenCenter + WorldOffsetToScreenOffset(world - WorldCenter);
            //var viewPos = SharpDX.Vector3.Transform(new SharpDX.Vector3(worldOffset.X, 0, worldOffset.Z), CameraView);
            //return ScreenHalfSize * new Vector2(viewPos.X / viewPos.Z, viewPos.Y / viewPos.Z);
            //return ScreenHalfSize * new Vector2(viewPos.X, viewPos.Y) / WorldHalfSize;
        }

        // this is useful for drawing on margins (TODO better api)
        public Vector2 RotatedCoords(Vector2 coords)
        {
            var x = coords.X * _cameraCosAzimuth - coords.Y * _cameraSinAzimuth;
            var y = coords.Y * _cameraCosAzimuth + coords.X * _cameraSinAzimuth;
            return new(x, y);
        }

        private Vector2 WorldOffsetToScreenOffset(Vector3 worldOffset)
        {
            return ScreenHalfSize *  RotatedCoords(new(worldOffset.X, worldOffset.Z)) / WorldHalfSize;
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
        public void PathArcTo(Vector3 center, float radius, float amin, float amax)
        {
            ImGui.GetWindowDrawList().PathArcTo(WorldPositionToScreenPosition(center), radius / WorldHalfSize * ScreenHalfSize, amin - MathF.PI / 2 + _cameraAzimuth, amax - MathF.PI / 2 + _cameraAzimuth);
        }

        public void PathStroke(bool closed, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().PathStroke(color, closed ? ImDrawFlags.Closed : ImDrawFlags.None, thickness);
        }

        public void PathFillConvex(uint color)
        {
            ImGui.GetWindowDrawList().PathFillConvex(color);
        }

        // draw zones - these are filled primitives clipped to various borders
        public void ZoneCone(Vector3 center, float innerRadius, float outerRadius, float angleStart, float angleEnd, uint color)
        {
            // TODO: think of a better way to do that (analytical clipping?)
            if (innerRadius >= outerRadius || innerRadius < 0)
                return;
            if (angleStart == angleEnd)
                return;
            if (angleStart > angleEnd)
            {
                var tmp = angleEnd;
                angleEnd = angleStart;
                angleStart = tmp;
            }
            var angleLength = MathF.Min(angleEnd - angleStart, 2 * MathF.PI);

            if (innerRadius == 0 && angleLength >= 2 * MathF.PI)
            {
                ZoneCircle(center, outerRadius, color);
                return;
            }

            // convert angles to screen coords and normalize
            angleStart -= _cameraAzimuth + MathF.PI / 2;
            angleStart %= 2 * MathF.PI;
            if (angleStart < -MathF.PI)
                angleStart += 2 * MathF.PI;
            else if (angleStart > MathF.PI)
                angleStart -= 2 * MathF.PI;
            // at this point, angleStart is in [-pi, pi) and angleLength is in (0, 2pi)

            var centerScreen = WorldPositionToScreenPosition(center);
            float innerRadiusScreen = innerRadius / WorldHalfSize * ScreenHalfSize;
            float outerRadiusScreen = outerRadius / WorldHalfSize * ScreenHalfSize;

            int innerSegments = innerRadiusScreen > 0 ? GeometryUtils.CalculateTesselationSegments(innerRadiusScreen, angleLength) : 0;
            int outerSegments = GeometryUtils.CalculateTesselationSegments(outerRadiusScreen, angleLength);

            List<Vector2> tesselated = new();
            if (innerSegments == 0)
            {
                tesselated.Add(centerScreen);
            }
            else
            {
                for (int i = 0; i <= innerSegments; ++i)
                {
                    tesselated.Add(GeometryUtils.PolarToCartesian(centerScreen, innerRadiusScreen, angleStart + (float)i / (float)innerSegments * angleLength));
                }
            }
            for (int i = outerSegments; i >= 0; --i)
            {
                tesselated.Add(GeometryUtils.PolarToCartesian(centerScreen, outerRadiusScreen, angleStart + (float)i / (float)outerSegments * angleLength));
            }

            ClipAndFillConcave(tesselated, color);
        }

        public void ZoneCircle(Vector3 center, float radius, uint color)
        {
            var poly = GeometryUtils.BuildTesselatedCircle(WorldPositionToScreenPosition(center), radius / WorldHalfSize * ScreenHalfSize);
            ClipAndFillConvex(poly, color);
        }

        public void ZoneTri(Vector3 a, Vector3 b, Vector3 c, uint color)
        {
            var tri = new Vector2[] { WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), WorldPositionToScreenPosition(c) };
            ClipAndFillConvex(tri, color);
        }

        public void ZoneIsoscelesTri(Vector3 apex, Vector3 height, Vector3 halfBase, uint color)
        {
            ZoneTri(apex, apex + height + halfBase, apex + height - halfBase, color);
        }

        public void ZoneIsoscelesTri(Vector3 apex, float direction, float halfAngle, float height, uint color)
        {
            Vector3 dir = GeometryUtils.DirectionToVec3(direction);
            Vector3 normal = new(-dir.Z, 0, dir.X);
            ZoneIsoscelesTri(apex, height * dir, height * MathF.Tan(halfAngle) * normal, color);
        }

        public void ZoneQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, uint color)
        {
            var quad = new Vector2[] { WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), WorldPositionToScreenPosition(c), WorldPositionToScreenPosition(d) };
            ClipAndFillConvex(quad, color);
        }

        public void ZoneQuad(Vector3 origin, Vector3 direction, float lenFront, float lenBack, float halfWidth, uint color)
        {
            Vector3 side = halfWidth * new Vector3(-direction.Z, 0, direction.X);
            Vector3 front = origin + lenFront * direction;
            Vector3 back = origin - lenBack * direction;
            ZoneQuad(front + side, front - side, back - side, back + side, color);
        }

        public void ZoneQuad(Vector3 origin, float direction, float lenFront, float lenBack, float halfWidth, uint color)
        {
            ZoneQuad(origin, GeometryUtils.DirectionToVec3(direction), lenFront, lenBack, halfWidth, color);
        }

        public void ZoneRect(Vector3 tl, Vector3 br, uint color)
        {
            ZoneQuad(tl, new Vector3(br.X, 0, tl.Z), br, new Vector3(tl.X, 0, br.Z), color);
        }

        // high level utilities
        // draw arena border
        public void Border()
        {
            if (IsCircle)
                AddCircle(WorldCenter, WorldHalfSize, ColorBorder, 2);
            else
                AddQuad(WorldNW, WorldNE, WorldSE, WorldSW, ColorBorder, 2);
        }

        // draw actor representation
        public void Actor(Vector3 position, float rotation, uint color)
        {
            if (InBounds(position))
            {
                var dir = GeometryUtils.DirectionToVec3(rotation);
                var normal = new Vector3(-dir.Z, 0, dir.X);
                AddTriangleFilled(position + 0.7f * dir, position - 0.35f * dir + 0.433f * normal, position - 0.35f * dir - 0.433f * normal, color);
            }
        }

        public void Actor(WorldState.Actor? actor, uint color)
        {
            if (actor != null)
                Actor(actor.Position, actor.Rotation, color);
        }

        public void End()
        {
            ImGui.GetWindowDrawList().PopClipRect();
        }

        public bool InBounds(Vector3 position)
        {
            var offset = position - WorldCenter;
            if (IsCircle)
            {
                return GeometryUtils.PointInCircle(offset, WorldHalfSize);
            }
            else
            {
                return Math.Abs(offset.X) <= WorldHalfSize && Math.Abs(offset.Z) <= WorldHalfSize;
            }
        }

        private void ClipAndFillConvex(IEnumerable<Vector2> poly, uint color)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var restoreFlags = drawlist.Flags;
            drawlist.Flags &= ~ImDrawListFlags.AntiAliasedFill;

            var clipped = GeometryUtils.ClipPolygonToPolygon(poly, _clipPolygon);
            foreach (var p in clipped)
                drawlist.PathLineToMergeDuplicate(p);
            drawlist.PathFillConvex(color);

            drawlist.Flags = restoreFlags;
        }

        private void ClipAndFillConcave(IEnumerable<Vector2> poly, uint color)
        {
            var clipped = GeometryUtils.ClipPolygonToPolygon(poly, _clipPolygon);
            if (clipped.Count < 3)
                return;

            // simple ear-clipping algorithm, TODO consider optimizing
            Func<int, (Vector2, Vector2, Vector2)> getTri = (int index) =>
            {
                var prev = clipped[index == 0 ? clipped.Count - 1 : index - 1];
                var curr = clipped[index];
                var next = clipped[index == clipped.Count - 1 ? 0 : index + 1];
                return (prev, curr, next);
            };
            Func<int, bool> isVertexConvex = (int index) =>
            {
                (var prev, var curr, var next) = getTri(index);
                var pc = curr - prev;
                var cn = next - curr;
                var normal = new Vector2(-pc.Y, pc.X);
                return Vector2.Dot(cn, normal) > 0;
            };

            List<bool> vertexIsConvex = new();
            int lastConcave = -1;
            for (int i = 0; i < clipped.Count; ++i)
            {
                bool isConvex = isVertexConvex(i);
                vertexIsConvex.Add(isConvex);
                if (!isConvex)
                    lastConcave = i;
            }

            if (lastConcave < 0)
            {
                // this is a convex polygon, use simple algorithm
                var dl = ImGui.GetWindowDrawList();
                foreach (var p in clipped)
                    dl.PathLineTo(p);
                dl.PathFillConvex(color);
                return;
            }

            Func<int, bool> isVertexEar = (int index) =>
            {
                if (!vertexIsConvex[index])
                    return false; // concave vertex can't be an ear

                (var prev, var curr, var next) = getTri(index);
                var e1 = curr - prev;
                var e2 = next - curr;
                var e3 = prev - next;
                var n1 = new Vector2(-e1.Y, e1.X);
                var n2 = new Vector2(-e2.Y, e2.X);
                var n3 = new Vector2(-e3.Y, e3.X);
                Func<int, bool> concaveVertexInTriangle = (int i) =>
                {
                    if (vertexIsConvex[i])
                        return false; // don't care about convex vertices here
                    var d1 = Vector2.Dot(clipped[i] - prev, n1);
                    var d2 = Vector2.Dot(clipped[i] - curr, n2);
                    var d3 = Vector2.Dot(clipped[i] - next, n3);
                    return d1 >= 0 && d2 >= 0 && d3 >= 0;
                };

                for (int i = index + 2, iEnd = index == 0 ? clipped.Count - 1 : clipped.Count; i < iEnd; ++i)
                    if (concaveVertexInTriangle(i))
                        return false;
                for (int i = (index == clipped.Count - 1) ? 1 : 0; i < index - 1; ++i)
                    if (concaveVertexInTriangle(i))
                        return false;
                return true;
            };

            var drawlist = ImGui.GetWindowDrawList();
            var restoreFlags = drawlist.Flags;
            drawlist.Flags &= ~ImDrawListFlags.AntiAliasedFill;
            int searchStart = lastConcave != clipped.Count - 1 ? lastConcave + 1 : 0; // optimization: avoid restarting search from the beginning after clipping each ear
            while (clipped.Count >= 3)
            {
                // find and clip next ear - skip next concave vertices
                int earIndex = searchStart;
                for (; earIndex < clipped.Count; ++earIndex)
                    if (isVertexEar(earIndex))
                        break;
                if (earIndex == clipped.Count)
                {
                    for (earIndex = 0; earIndex < searchStart; ++earIndex)
                        if (isVertexEar(earIndex))
                            break;
                    // we are guaranteed to find an ear, there's a theorem about that
                    // if we fail, that's probably because we have only degenerate stuff left and floating-point imprecision affects us
                    if (earIndex == searchStart)
                        break;
                }

                (var prev, var curr, var next) = getTri(earIndex);
                drawlist.AddTriangleFilled(prev, curr, next, color);

                clipped.RemoveAt(earIndex);
                vertexIsConvex.RemoveAt(earIndex);

                // recalculate convexity for neighbouring edges
                int neighbour = earIndex != 0 ? earIndex - 1 : clipped.Count - 1;
                vertexIsConvex[neighbour] = isVertexConvex(neighbour);
                neighbour = earIndex != clipped.Count ? earIndex : 0;
                vertexIsConvex[neighbour] = isVertexConvex(neighbour);

                searchStart = earIndex != clipped.Count ? earIndex : 0;
            }
            drawlist.Flags = restoreFlags;
        }
    }
}
