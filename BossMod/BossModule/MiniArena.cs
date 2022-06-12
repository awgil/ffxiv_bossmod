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
        public WPos WorldCenter = new(100, 100);
        public float WorldHalfSize = 20;
        public float ScreenHalfSize => 150 * Config.ArenaScale;
        public float ScreenMarginSize => 20 * Config.ArenaScale;

        // these are set at the beginning of each draw
        public Vector2 ScreenCenter { get; private set; } = new();
        private float _cameraAzimuth;
        private float _cameraSinAzimuth = 0;
        private float _cameraCosAzimuth = 1;
        private Clip2D _clipper = new();

        private float WorldApproxError => CurveApprox.ScreenError / ScreenHalfSize * WorldHalfSize;

        // useful points
        public WPos WorldN  => WorldCenter + new WDir(             0, -WorldHalfSize);
        public WPos WorldNE => WorldCenter + new WDir(+WorldHalfSize, -WorldHalfSize);
        public WPos WorldE  => WorldCenter + new WDir(+WorldHalfSize, 0);
        public WPos WorldSE => WorldCenter + new WDir(+WorldHalfSize, +WorldHalfSize);
        public WPos WorldS  => WorldCenter + new WDir(             0, +WorldHalfSize);
        public WPos WorldSW => WorldCenter + new WDir(-WorldHalfSize, +WorldHalfSize);
        public WPos WorldW  => WorldCenter + new WDir(-WorldHalfSize, 0);
        public WPos WorldNW => WorldCenter + new WDir(-WorldHalfSize, -WorldHalfSize);

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
                _clipper.SetClipPoly(CurveApprox.Circle(WorldCenter.ToVec2(), WorldHalfSize, WorldApproxError));
            }
            else
            {
                _clipper.SetClipPoly(new[] { WorldNW.ToVec2(), WorldNE.ToVec2(), WorldSE.ToVec2(), WorldSW.ToVec2() });
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
        public Vector2 WorldPositionToScreenPosition(WPos worldXZ)
        {
            return ScreenCenter + WorldOffsetToScreenOffset(worldXZ - WorldCenter);
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
            return ScreenHalfSize *  RotatedCoords(worldOffset.ToVec2()) / WorldHalfSize;
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
            ImGui.GetWindowDrawList().AddCircle(WorldPositionToScreenPosition(center), radius / WorldHalfSize * ScreenHalfSize, color, 0, thickness);
        }

        public void AddCircleFilled(WPos center, float radius, uint color)
        {
            ImGui.GetWindowDrawList().AddCircleFilled(WorldPositionToScreenPosition(center), radius / WorldHalfSize * ScreenHalfSize, color);
        }

        public void AddCone(WPos center, float radius, Angle centerDirection, Angle halfAngle, uint color, float thickness = 1)
        {
            var sCenter = WorldPositionToScreenPosition(center);
            float sDir = centerDirection.Rad - MathF.PI / 2 + _cameraAzimuth;
            var drawlist = ImGui.GetWindowDrawList();
            drawlist.PathLineTo(sCenter);
            drawlist.PathArcTo(sCenter, radius / WorldHalfSize * ScreenHalfSize, sDir - halfAngle.Rad, sDir + halfAngle.Rad);
            drawlist.PathStroke(color, ImDrawFlags.Closed, thickness);
        }

        // path api: add new point to path; this adds new edge from last added point, or defines first vertex if path is empty
        public void PathLineTo(WPos p)
        {
            ImGui.GetWindowDrawList().PathLineToMergeDuplicate(WorldPositionToScreenPosition(p));
        }

        // adds a bunch of points corresponding to arc - if path is non empty, this adds an edge from last point to first arc point
        public void PathArcTo(WPos center, float radius, float amin, float amax)
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
        public void ZoneCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color)
        {
            // TODO: think of a better way to do that (analytical clipping?)
            if (innerRadius >= outerRadius || innerRadius < 0 || halfAngle.Rad <= 0)
                return;

            bool fullCircle = halfAngle.Rad >= MathF.PI;
            bool donut = innerRadius > 0;
            var points = (donut, fullCircle) switch
            {
                (false, false) => CurveApprox.CircleSector(center.ToVec2(), outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, WorldApproxError),
                (false, true) => CurveApprox.Circle(center.ToVec2(), outerRadius, WorldApproxError),
                (true, false) => CurveApprox.DonutSector(center.ToVec2(), innerRadius, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, WorldApproxError),
                (true, true) => CurveApprox.DonutSector(center.ToVec2(), innerRadius, outerRadius, Angle.Radians(0), Angle.Radians(2 * MathF.PI), WorldApproxError),
            };
            ClipAndFill(points, color);
        }

        public void ZoneCircle(WPos center, float radius, uint color)
        {
            ClipAndFill(CurveApprox.Circle(center.ToVec2(), radius, WorldApproxError), color);
        }

        public void ZoneDonut(WPos center, float innerRadius, float outerRadius, uint color)
        {
            if (innerRadius >= outerRadius || innerRadius < 0)
                return;
            ClipAndFill(CurveApprox.DonutSector(center.ToVec2(), innerRadius, outerRadius, Angle.Radians(0), Angle.Radians(2 * MathF.PI), WorldApproxError), color);
        }

        public void ZoneTri(WPos a, WPos b, WPos c, uint color)
        {
            ClipAndFill(new[] { a.ToVec2(), b.ToVec2(), c.ToVec2() }, color);
        }

        public void ZoneIsoscelesTri(WPos apex, WDir height, WDir halfBase, uint color)
        {
            ZoneTri(apex, apex + height + halfBase, apex + height - halfBase, color);
        }

        public void ZoneIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height, uint color)
        {
            var dir = direction.ToDirection();
            var normal = dir.OrthoL();
            ZoneIsoscelesTri(apex, height * dir, height * halfAngle.Tan() * normal, color);
        }

        public void ZoneQuad(WPos a, WPos b, WPos c, WPos d, uint color)
        {
            ClipAndFill(new[] { a.ToVec2(), b.ToVec2(), c.ToVec2(), d.ToVec2() }, color);
        }

        public void ZoneQuad(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color)
        {
            var side = halfWidth * direction.OrthoR();
            var front = origin + lenFront * direction;
            var back = origin - lenBack * direction;
            ZoneQuad(front + side, front - side, back - side, back + side, color);
        }

        public void ZoneQuad(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color)
        {
            ZoneQuad(origin, direction.ToDirection(), lenFront, lenBack, halfWidth, color);
        }

        public void ZoneQuad(WPos start, WPos end, float halfWidth, uint color)
        {
            var dir = (end - start).Normalized();
            var side = halfWidth * dir.OrthoR();
            ZoneQuad(start + side, start - side, end - side, end + side, color);
        }

        public void ZoneRect(WPos tl, WPos br, uint color)
        {
            ZoneQuad(tl, new(br.X, tl.Z), br, new(tl.X, br.Z), color);
        }

        public void TextScreen(Vector2 center, string text, uint color, float fontSize = 17)
        {
            var size = ImGui.CalcTextSize(text);
            ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), fontSize, center - size / 2, color, text);
        }

        public void TextWorld(WPos center, string text, uint color, float fontSize = 17)
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
        public void Actor(WPos position, Angle rotation, uint color)
        {
            if (InBounds(position))
            {
                var dir = rotation.ToDirection();
                var normal = dir.OrthoR();
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

        public bool InBounds(WPos position)
        {
            if (IsCircle)
            {
                return position.InCircle(WorldCenter, WorldHalfSize);
            }
            else
            {
                var offset = position - WorldCenter;
                return Math.Abs(offset.X) <= WorldHalfSize && Math.Abs(offset.Z) <= WorldHalfSize;
            }
        }

        public WPos ClampToBounds(WPos position)
        {
            var offset = position - WorldCenter;
            if (IsCircle)
            {
                if (offset.LengthSq() > WorldHalfSize * WorldHalfSize)
                    offset *= WorldHalfSize / offset.Length();
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
                drawlist.AddTriangleFilled(WorldPositionToScreenPosition(new(tri[i])), WorldPositionToScreenPosition(new(tri[i + 1])), WorldPositionToScreenPosition(new(tri[i + 2])), color);

            drawlist.Flags = restoreFlags;
        }
    }
}
