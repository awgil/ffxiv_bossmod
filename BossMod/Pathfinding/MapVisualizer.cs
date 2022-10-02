using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod.Pathfinding
{
    public class MapVisualizer
    {
        public Map Map;
        public float ScreenPixelSize = 10;
        public List<(WPos center, float ir, float or, Angle dir, Angle halfWidth)> Sectors = new();
        public List<(WPos origin, float lenF, float lenB, float halfWidth, Angle dir)> Rects = new();

        public MapVisualizer(Map map)
        {
            Map = map;
        }

        public void Draw()
        {
            var tl = ImGui.GetCursorScreenPos();
            var br = tl + new Vector2(Map.Width, Map.Height) * ScreenPixelSize;
            var tr = new Vector2(br.X, tl.Y);
            var bl = new Vector2(tl.X, br.Y);
            ImGui.Dummy(br - tl);
            var dl = ImGui.GetWindowDrawList();

            // blocked squares / goal
            for (int y = 0; y < Map.Height; ++y)
            {
                for (int x = 0; x < Map.Width; ++x)
                {
                    var corner = tl + new Vector2(x, y) * ScreenPixelSize;
                    var cornerEnd = corner + new Vector2(ScreenPixelSize, ScreenPixelSize);

                    var pix = Map[x, y];
                    if (pix.MaxG < float.MaxValue)
                    {
                        var alpha = 1 - (pix.MaxG > 0 ? pix.MaxG / Map.MaxG : 0);
                        uint c = 128 + (uint)(alpha * 127);
                        c = c | (c << 8) | 0xff000000;
                        dl.AddRectFilled(corner, cornerEnd, c);
                    }

                    if (ImGui.IsMouseHoveringRect(corner, cornerEnd))
                    {
                        ImGui.SameLine();
                        if (pix.MaxG < float.MaxValue)
                        {
                            ImGui.TextUnformatted($"Pixel at {x},{y}: blocked, g={pix.MaxG:f3}");
                        }
                        else if (pix.Priority > 0)
                        {
                            ImGui.TextUnformatted($"Pixel at {x},{y}: goal, prio={pix.Priority}");
                        }
                        else
                        {
                            ImGui.TextUnformatted($"Pixel at {x},{y}: normal");
                        }
                    }
                }
            }

            // border
            dl.AddLine(tl, tr, 0xffffffff, 2);
            dl.AddLine(tr, br, 0xffffffff, 2);
            dl.AddLine(br, bl, 0xffffffff, 2);
            dl.AddLine(bl, tl, 0xffffffff, 2);

            // grid
            for (int x = 1; x < Map.Width; ++x)
            {
                var off = new Vector2(x * ScreenPixelSize, 0);
                dl.AddLine(tl + off, bl + off, 0xffffffff, 1);
            }
            for (int y = 1; y < Map.Height; ++y)
            {
                var off = new Vector2(0, y * ScreenPixelSize);
                dl.AddLine(tl + off, tr + off, 0xffffffff, 1);
            }

            // shapes
            foreach (var c in Sectors)
            {
                DrawSector(dl, tl, c.center, c.ir, c.or, c.dir, c.halfWidth);
            }
            foreach (var r in Rects)
            {
                var direction = r.dir.ToDirection();
                var side = r.halfWidth * direction.OrthoR();
                var front = r.origin + r.lenF * direction;
                var back = r.origin - r.lenB * direction;
                dl.AddQuad(tl + Map.WorldToGridFrac(front + side) * ScreenPixelSize, tl + Map.WorldToGridFrac(front - side) * ScreenPixelSize, tl + Map.WorldToGridFrac(back - side) * ScreenPixelSize, tl + Map.WorldToGridFrac(back + side) * ScreenPixelSize, 0xff0000ff);
            }
        }

        private void DrawSector(ImDrawListPtr dl, Vector2 tl, WPos center, float ir, float or, Angle dir, Angle halfWidth)
        {
            if (halfWidth.Rad <= 0 || or <= 0 || ir >= or)
                return;

            var sCenter = tl + Map.WorldToGridFrac(center) * ScreenPixelSize;
            if (halfWidth.Rad >= MathF.PI)
            {
                dl.AddCircle(sCenter, or / Map.Resolution * ScreenPixelSize, 0xff0000ff);
                if (ir > 0)
                    dl.AddCircle(sCenter, ir / Map.Resolution * ScreenPixelSize, 0xff0000ff);
            }
            else
            {
                float sDir = MathF.PI / 2 - dir.Rad;
                dl.PathArcTo(sCenter, ir / Map.Resolution * ScreenPixelSize, sDir + halfWidth.Rad, sDir - halfWidth.Rad);
                dl.PathArcTo(sCenter, or / Map.Resolution * ScreenPixelSize, sDir - halfWidth.Rad, sDir + halfWidth.Rad);
                dl.PathStroke(0xff0000ff, ImDrawFlags.Closed, 1);
            }
        }
    }
}
