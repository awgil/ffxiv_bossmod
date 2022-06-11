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

        public bool IsCircle = false;
        public Vector3 WorldCenter = new(100, 0, 100);
        public float WorldHalfSize = 20;
        public float ScreenHalfSize => 150 * Config.ArenaScale;
        public float ScreenMarginSize => 20 * Config.ArenaScale;

        // these are set at the beginning of each draw
        public Vector2 ScreenCenter { get; private set; } = new();
        private float _cameraAzimuth = 0;
        private float _cameraSinAzimuth = 0;
        private float _cameraCosAzimuth = 1;
        private Clip2D _clipper = new();

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
        public uint ColorBackground = 0xc00f0f0f;
        public uint ColorBorder = 0xffffffff;
        public uint ColorAOE = 0x80008080;
        public uint ColorSafeFromAOE = 0x80008000;
        public uint ColorDanger = 0xff00ffff;
        public uint ColorSafe = 0xff00ff00;
        public uint ColorPC = 0xff00ff00;
        public uint ColorEnemy = 0xff0000ff;
        public uint ColorObject = 0xff0080ff;
        public uint ColorPlayerInteresting = 0xffc0c0c0;
        public uint ColorPlayerGeneric = 0xff808080;
        public uint ColorVulnerable = 0xffff00ff;

        public MiniArena(BossModuleConfig config)
        {
            Config = config;
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
            ScreenCenter = cursor + centerOffset;

            if (IsCircle)
            {
                _clipper.SetClipPoly(CurveApprox.Circle(ScreenCenter, ScreenHalfSize));
            }
            else
            {
                _clipper.SetClipPoly(new[] { WorldPositionToScreenPosition(WorldNW), WorldPositionToScreenPosition(WorldNE), WorldPositionToScreenPosition(WorldSE), WorldPositionToScreenPosition(WorldSW) });
            }

            var wmin = ImGui.GetWindowPos();
            var wmax = wmin + ImGui.GetWindowSize();
            ImGui.GetWindowDrawList().PushClipRect(Vector2.Max(cursor, wmin), Vector2.Min(cursor + fullSize, wmax));
            if (Config.OpaqueArenaBackground)
            {
                if (IsCircle)
                    ZoneCircle(WorldCenter, WorldHalfSize, ColorBackground);
                else
                    ZoneQuad(WorldNW, WorldNE, WorldSE, WorldSW, ColorBackground);
            }
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

        public void AddCone(Vector3 center, float radius, float centerDirection, float halfAngle, uint color, float thickness = 1)
        {
            var sCenter = WorldPositionToScreenPosition(center);
            float sDir = centerDirection - MathF.PI / 2 + _cameraAzimuth;
            var drawlist = ImGui.GetWindowDrawList();
            drawlist.PathLineTo(sCenter);
            drawlist.PathArcTo(sCenter, radius / WorldHalfSize * ScreenHalfSize, sDir - halfAngle, sDir + halfAngle);
            drawlist.PathStroke(color, ImDrawFlags.Closed, thickness);
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
        public void ZoneCone(Vector3 center, float innerRadius, float outerRadius, float centerDirection, float halfAngle, uint color)
        {
            // TODO: think of a better way to do that (analytical clipping?)
            if (innerRadius >= outerRadius || innerRadius < 0 || halfAngle <= 0)
                return;

            var centerScreen = WorldPositionToScreenPosition(center);
            float innerRadiusScreen = innerRadius / WorldHalfSize * ScreenHalfSize;
            float outerRadiusScreen = outerRadius / WorldHalfSize * ScreenHalfSize;
            centerDirection -= _cameraAzimuth;

            bool fullCircle = halfAngle >= MathF.PI;
            bool donut = innerRadius > 0;
            var points = (donut, fullCircle) switch
            {
                (false, false) => CurveApprox.CircleSector(centerScreen, outerRadiusScreen, centerDirection - halfAngle, centerDirection + halfAngle),
                (false, true) => CurveApprox.Circle(centerScreen, outerRadiusScreen),
                (true, false) => CurveApprox.DonutSector(centerScreen, innerRadiusScreen, outerRadiusScreen, centerDirection - halfAngle, centerDirection + halfAngle),
                (true, true) => CurveApprox.DonutSector(centerScreen, innerRadiusScreen, outerRadiusScreen, 0, 2 * MathF.PI),
            };
            ClipAndFill(points, color);
        }

        public void ZoneCircle(Vector3 center, float radius, uint color)
        {
            ClipAndFill(CurveApprox.Circle(WorldPositionToScreenPosition(center), radius / WorldHalfSize * ScreenHalfSize), color);
        }

        public void ZoneDonut(Vector3 center, float innerRadius, float outerRadius, uint color)
        {
            if (innerRadius >= outerRadius || innerRadius < 0)
                return;
            ClipAndFill(CurveApprox.DonutSector(WorldPositionToScreenPosition(center), innerRadius / WorldHalfSize * ScreenHalfSize, outerRadius / WorldHalfSize * ScreenHalfSize, 0, 2 * MathF.PI), color);
        }

        public void ZoneTri(Vector3 a, Vector3 b, Vector3 c, uint color)
        {
            var tri = new[] { WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), WorldPositionToScreenPosition(c) };
            ClipAndFill(tri, color);
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
            var quad = new[] { WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), WorldPositionToScreenPosition(c), WorldPositionToScreenPosition(d) };
            ClipAndFill(quad, color);
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

        public void ZoneQuad(Vector3 start, Vector3 end, float halfWidth, uint color)
        {
            Vector3 dir = Vector3.Normalize(end - start);
            Vector3 side = halfWidth * new Vector3(-dir.Z, 0, dir.X);
            ZoneQuad(start + side, start - side, end - side, end + side, color);
        }

        public void ZoneRect(Vector3 tl, Vector3 br, uint color)
        {
            ZoneQuad(tl, new Vector3(br.X, 0, tl.Z), br, new Vector3(tl.X, 0, br.Z), color);
        }

        public void TextScreen(Vector2 center, string text, uint color, float fontSize = 17)
        {
            var size = ImGui.CalcTextSize(text);
            ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), fontSize, center - size / 2, color, text);
        }

        public void TextWorld(Vector3 center, string text, uint color, float fontSize = 17)
        {
            TextScreen(WorldPositionToScreenPosition(center), text, color, fontSize);
        }

        // high level utilities
        // draw arena border
        public void Border()
        {
            if (IsCircle)
                AddCircle(WorldCenter, WorldHalfSize, ColorBorder, 2);
            else
                AddQuad(WorldNW, WorldNE, WorldSE, WorldSW, ColorBorder, 2);

            if (Config.ShowCardinals)
            {
                var offCenter = ScreenHalfSize + 10;
                TextScreen(ScreenCenter + RotatedCoords(new(0, -offCenter)), "N", ColorBorder);
                TextScreen(ScreenCenter + RotatedCoords(new(0,  offCenter)), "S", ColorBorder);
                TextScreen(ScreenCenter + RotatedCoords(new( offCenter, 0)), "E", ColorBorder);
                TextScreen(ScreenCenter + RotatedCoords(new(-offCenter, 0)), "W", ColorBorder);
            }
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

        public void Actor(Actor? actor, uint color)
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

        public Vector3 ClampToBounds(Vector3 position)
        {
            var offset = position - WorldCenter;
            if (IsCircle)
            {
                if (offset.LengthSquared() > WorldHalfSize * WorldHalfSize)
                    offset *= WorldHalfSize * offset.Length();
            }
            else
            {
                if (Math.Abs(offset.X) > WorldHalfSize)
                    offset *= WorldHalfSize / Math.Abs(offset.X);
                if (Math.Abs(offset.Z) > WorldHalfSize)
                    offset *= WorldHalfSize / Math.Abs(offset.Z);
            }
            return WorldCenter + offset;
        }

        private void ClipAndFill(IEnumerable<Vector2> poly, uint color)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var restoreFlags = drawlist.Flags;
            drawlist.Flags &= ~ImDrawListFlags.AntiAliasedFill;

            var tri = _clipper.ClipAndTriangulate(poly);
            for (int i = 0; i < tri.Count; i += 3)
                drawlist.AddTriangleFilled(tri[i], tri[i + 1], tri[i + 2], color);

            drawlist.Flags = restoreFlags;
        }
    }
}
