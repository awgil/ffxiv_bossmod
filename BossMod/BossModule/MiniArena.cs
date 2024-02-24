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
        public BossModuleConfig Config { get; init; }
        public ArenaBounds Bounds;

        public float ScreenHalfSize => 150 * Config.ArenaScale;
        public float ScreenMarginSize => 20 * Config.ArenaScale;

        // these are set at the beginning of each draw
        public Vector2 ScreenCenter { get; private set; } = new();
        private float _cameraAzimuth;
        private float _cameraSinAzimuth = 0;
        private float _cameraCosAzimuth = 1;

        public MiniArena(BossModuleConfig config, ArenaBounds bounds)
        {
            Config = config;
            Bounds = bounds;
        }

        // prepare for drawing - set up internal state, clip rect etc.
        public void Begin(float cameraAzimuthRadians)
        {
            var centerOffset = new Vector2(ScreenMarginSize + (Config.AddSlackForRotations ? 1.5f : 1.0f) * ScreenHalfSize);
            var fullSize = 2 * centerOffset;
            var cursor = ImGui.GetCursorScreenPos();
            ImGui.Dummy(fullSize);

            Bounds.ScreenHalfSize = ScreenHalfSize;
            ScreenCenter = cursor + centerOffset;
            _cameraAzimuth = cameraAzimuthRadians;
            _cameraSinAzimuth = MathF.Sin(cameraAzimuthRadians);
            _cameraCosAzimuth = MathF.Cos(cameraAzimuthRadians);
            //var camWorld = SharpDX.Matrix.RotationYawPitchRoll(azimuth, altitude, 0);
            //camWorld.Row4 = camWorld.Row3 * WorldHalfSize;
            //camWorld.M44 = 1;
            //CameraView = SharpDX.Matrix.Invert(camWorld);

            var wmin = ImGui.GetWindowPos();
            var wmax = wmin + ImGui.GetWindowSize();
            ImGui.GetWindowDrawList().PushClipRect(Vector2.Max(cursor, wmin), Vector2.Min(cursor + fullSize, wmax));
            if (Config.OpaqueArenaBackground)
            {
                foreach (var p in Bounds.ClipPoly)
                    PathLineTo(p);
                PathFillConvex(ArenaColor.Background);
            }
        }

        // if you are 100% sure your primitive does not need clipping, you can use drawlist api directly
        // this helper allows converting world-space coords to screen-space ones
        public Vector2 WorldPositionToScreenPosition(WPos p)
        {
            return ScreenCenter + WorldOffsetToScreenOffset(p - Bounds.Center);
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

        private Vector2 WorldOffsetToScreenOffset(WDir worldOffset)
        {
            return ScreenHalfSize * RotatedCoords(worldOffset.ToVec2()) / Bounds.HalfSize;
        }

        // unclipped primitive rendering that accept world-space positions; thin convenience wrappers around drawlist api
        public void AddLine(WPos a, WPos b, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().AddLine(WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), color, thickness);
        }

        public void AddTriangle(WPos p1, WPos p2, WPos p3, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().AddTriangle(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color, thickness);
        }

        public void AddTriangleFilled(WPos p1, WPos p2, WPos p3, uint color)
        {
            ImGui.GetWindowDrawList().AddTriangleFilled(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color);
        }

        public void AddQuad(WPos p1, WPos p2, WPos p3, WPos p4, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().AddQuad(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), WorldPositionToScreenPosition(p4), color, thickness);
        }

        public void AddQuadFilled(WPos p1, WPos p2, WPos p3, WPos p4, uint color)
        {
            ImGui.GetWindowDrawList().AddQuadFilled(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), WorldPositionToScreenPosition(p4), color);
        }

        public void AddCircle(WPos center, float radius, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().AddCircle(WorldPositionToScreenPosition(center), radius / Bounds.HalfSize * ScreenHalfSize, color, 0, thickness);
        }

        public void AddCircleFilled(WPos center, float radius, uint color)
        {
            ImGui.GetWindowDrawList().AddCircleFilled(WorldPositionToScreenPosition(center), radius / Bounds.HalfSize * ScreenHalfSize, color);
        }

        public void AddCone(WPos center, float radius, Angle centerDirection, Angle halfAngle, uint color, float thickness = 1)
        {
            var sCenter = WorldPositionToScreenPosition(center);
            float sDir = MathF.PI / 2 - centerDirection.Rad + _cameraAzimuth;
            var drawlist = ImGui.GetWindowDrawList();
            drawlist.PathLineTo(sCenter);
            drawlist.PathArcTo(sCenter, radius / Bounds.HalfSize * ScreenHalfSize, sDir - halfAngle.Rad, sDir + halfAngle.Rad);
            drawlist.PathStroke(color, ImDrawFlags.Closed, thickness);
        }

        public void AddDonutCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color, float thickness = 1)
        {
            var sCenter = WorldPositionToScreenPosition(center);
            float sDir = MathF.PI / 2 - centerDirection.Rad + _cameraAzimuth;
            var drawlist = ImGui.GetWindowDrawList();
            drawlist.PathArcTo(sCenter, innerRadius / Bounds.HalfSize * ScreenHalfSize, sDir + halfAngle.Rad, sDir - halfAngle.Rad);
            drawlist.PathArcTo(sCenter, outerRadius / Bounds.HalfSize * ScreenHalfSize, sDir - halfAngle.Rad, sDir + halfAngle.Rad);
            drawlist.PathStroke(color, ImDrawFlags.Closed, thickness);
        }

        public void AddPolygon(IEnumerable<WPos> vertices, uint color, float thickness = 1)
        {
            foreach (var p in vertices)
                PathLineTo(p);
            PathStroke(true, color, thickness);
        }

        // path api: add new point to path; this adds new edge from last added point, or defines first vertex if path is empty
        public void PathLineTo(WPos p)
        {
            ImGui.GetWindowDrawList().PathLineToMergeDuplicate(WorldPositionToScreenPosition(p));
        }

        // adds a bunch of points corresponding to arc - if path is non empty, this adds an edge from last point to first arc point
        public void PathArcTo(WPos center, float radius, float amin, float amax)
        {
            ImGui.GetWindowDrawList().PathArcTo(WorldPositionToScreenPosition(center), radius / Bounds.HalfSize * ScreenHalfSize, MathF.PI / 2 - amin + _cameraAzimuth, MathF.PI / 2 - amax + _cameraAzimuth);
        }

        public void PathStroke(bool closed, uint color, float thickness = 1)
        {
            ImGui.GetWindowDrawList().PathStroke(color, closed ? ImDrawFlags.Closed : ImDrawFlags.None, thickness);
        }

        public void PathFillConvex(uint color)
        {
            ImGui.GetWindowDrawList().PathFillConvex(color);
        }

        // draw clipped & triangulated zone
        public void Zone(List<(WPos, WPos, WPos)> triangulation, uint color)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var restoreFlags = drawlist.Flags;
            drawlist.Flags &= ~ImDrawListFlags.AntiAliasedFill;
            foreach (var (p1, p2, p3) in triangulation)
                drawlist.AddTriangleFilled(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color);
            drawlist.Flags = restoreFlags;
        }

        // draw zones - these are filled primitives clipped to various borders
        public void ZoneCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color) => Zone(Bounds.ClipAndTriangulateCone(center, innerRadius, outerRadius, centerDirection, halfAngle), color);
        public void ZoneCircle(WPos center, float radius, uint color) => Zone(Bounds.ClipAndTriangulateCircle(center, radius), color);
        public void ZoneDonut(WPos center, float innerRadius, float outerRadius, uint color) => Zone(Bounds.ClipAndTriangulateDonut(center, innerRadius, outerRadius), color);
        public void ZoneTri(WPos a, WPos b, WPos c, uint color) => Zone(Bounds.ClipAndTriangulateTri(a, b, c), color);
        public void ZoneIsoscelesTri(WPos apex, WDir height, WDir halfBase, uint color) => Zone(Bounds.ClipAndTriangulateIsoscelesTri(apex, height, halfBase), color);
        public void ZoneIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height, uint color) => Zone(Bounds.ClipAndTriangulateIsoscelesTri(apex, direction, halfAngle, height), color);
        public void ZoneRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color) => Zone(Bounds.ClipAndTriangulateRect(origin, direction, lenFront, lenBack, halfWidth), color);
        public void ZoneRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color) => Zone(Bounds.ClipAndTriangulateRect(origin, direction, lenFront, lenBack, halfWidth), color);
        public void ZoneRect(WPos start, WPos end, float halfWidth, uint color) => Zone(Bounds.ClipAndTriangulateRect(start, end, halfWidth), color);

        public void TextScreen(Vector2 center, string text, uint color, float fontSize = 17)
        {
            var size = ImGui.CalcTextSize(text) * Config.ArenaScale;
            ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), fontSize * Config.ArenaScale, center - size / 2, color, text);
        }

        public void TextWorld(WPos center, string text, uint color, float fontSize = 17)
        {
            TextScreen(WorldPositionToScreenPosition(center), text, color, fontSize);
        }

        // high level utilities
        // draw arena border
        public void Border(uint color)
        {
            foreach (var p in Bounds.ClipPoly)
                PathLineTo(p);
            PathStroke(true, color, 2);
        }

        public void CardinalNames()
        {
            var offCenter = ScreenHalfSize + ScreenMarginSize / 2;
            var offS = RotatedCoords(new(0, offCenter));
            var offE = RotatedCoords(new(offCenter, 0));
            TextScreen(ScreenCenter - offS, "N", ArenaColor.Border);
            TextScreen(ScreenCenter + offS, "S", ArenaColor.Border);
            TextScreen(ScreenCenter + offE, "E", ArenaColor.Border);
            TextScreen(ScreenCenter - offE, "W", ArenaColor.Border);
        }

        // draw actor representation
        public void Actor(WPos position, Angle rotation, uint color)
        {
            var dir = rotation.ToDirection();
            var normal = dir.OrthoR();
            if (Bounds.Contains(position))
            {
                if (Config.ShowOutlinesAndShadows)
                    AddTriangle(position + 0.7f * dir, position - 0.35f * dir + 0.433f * normal, position - 0.35f * dir - 0.433f * normal, 0xFF000000, 2);
                AddTriangleFilled(position + 0.7f * dir, position - 0.35f * dir + 0.433f * normal, position - 0.35f * dir - 0.433f * normal, color);
            }
            else
            {
                position = Bounds.ClampToBounds(position);
                AddTriangle(position + 0.7f * dir, position - 0.35f * dir + 0.433f * normal, position - 0.35f * dir - 0.433f * normal, color);
            }
        }

        public void Actor(Actor? actor, uint color, bool allowDeadAndUntargetable = false)
        {
            if (actor != null && !actor.IsDestroyed && (allowDeadAndUntargetable || actor.IsTargetable && !actor.IsDead))
                Actor(actor.Position, actor.Rotation, color);
        }

        public void Actors(IEnumerable<Actor> actors, uint color, bool allowDeadAndUntargetable = false)
        {
            foreach (var a in actors)
                Actor(a, color, allowDeadAndUntargetable);
        }

        public void End()
        {
            ImGui.GetWindowDrawList().PopClipRect();
        }
    }
}
