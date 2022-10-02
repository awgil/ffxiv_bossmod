using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Pathfinding
{
    public class MapVisualizer
    {
        public Map Map;
        public float ScreenPixelSize = 10;
        public List<(WPos center, float radius)> Circles = new();

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

            // blocked squares
            for (int y = 0; y < Map.Height; ++y)
            {
                for (int x = 0; x < Map.Width; ++x)
                {
                    var g = Map[x, y].MaxG;
                    if (g < float.MaxValue)
                    {
                        var alpha = 1 - (g > 0 ? g / Map.MaxG : 0);
                        uint c = 128 + (uint)(alpha * 127);
                        c = c | (c << 8) | (c << 24);
                        var corner = tl + new Vector2(x, y) * ScreenPixelSize;
                        dl.AddRectFilled(corner, corner + new Vector2(ScreenPixelSize, ScreenPixelSize), c);
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
            foreach (var c in Circles)
            {
                dl.AddCircle(tl + Map.WorldToGridFrac(c.center) * ScreenPixelSize, c.radius / Map.Resolution * ScreenPixelSize, 0xff0000ff);
            }
        }
    }
}
