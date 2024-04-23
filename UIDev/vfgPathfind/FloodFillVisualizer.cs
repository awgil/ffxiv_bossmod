using BossMod;
using ImGuiNET;

namespace UIDev.vfgPathfind;

public class FloodFillVisualizer
{
    public Map Map;
    public Vector3 StartPos;
    public float EndZ;
    public float ScreenPixelSize = 2.0f;
    public float CurrT;
    public float MaxT;
    public List<Vector4> Path = []; // x/y/z/t
    public int ScrollY;

    public FloodFillVisualizer(Map map, Vector3 startPos, float endZ, int maxDeltaZ)
    {
        Map = map;
        StartPos = startPos;
        EndZ = endZ;

        Path.Add(new(startPos, 0));
        var p = map.WorldToGrid(StartPos);
        var t = 0;
        var ey = map.WorldToGrid(new(0, 0, endZ)).y;
        var invSpeed = 1.0f / 6;
        while (p.y > ey)
        {
            var pathfind = new FloodFill(map, 6, p.x, p.y, t);
            var path = pathfind.SolveUntilY(Math.Max(ey, p.y - maxDeltaZ)).ToList();
            path.Reverse();
            var firstMoveIndex = path.FindIndex(v => (v.x, v.y) != p);
            if (firstMoveIndex <= 0)
                break;

            var firstStep = path[firstMoveIndex];
            while (map[firstStep.x, firstStep.y, t])
                ++t;

            var firstUnreachable = firstMoveIndex + 1 < path.Count ? path.FindIndex(firstMoveIndex + 1, v => !map.StraightLineAllowed(p.x, p.y, t, v.x, v.y, invSpeed)) : -1;
            var next = path[firstUnreachable > 0 ? firstUnreachable - 1 : path.Count - 1];
            var dx = next.x - p.x;
            var dy = next.y - p.y;
            if (dx == 0 && dy == 0)
                break;
            Path.Add(new(map.GridToWorld(next.x, next.y, 0, 0), t * map.TimeResolution));

            var dist = MathF.Sqrt(dx * dx + dy * dy) * map.SpaceResolution;
            var dt = dist * invSpeed * map.InvTimeResolution;
            p = (next.x, next.y);
            t += (int)MathF.Ceiling(dt);
        }
        CurrT = MaxT = t * map.TimeResolution;
    }

    public void Draw()
    {
        var h = Math.Min(Map.Height, (int)(ImGui.GetWindowHeight() * 0.5f / ScreenPixelSize));
        var tl = ImGui.GetCursorScreenPos();
        var br = tl + new Vector2(Map.Width, h) * ScreenPixelSize;
        var tr = new Vector2(br.X, tl.Y);
        var bl = new Vector2(tl.X, br.Y);
        //ImGui.Dummy(br - tl);
        var cursorEnd = ImGui.GetCursorPos();
        cursorEnd.Y += h * ScreenPixelSize + 10;
        var dl = ImGui.GetWindowDrawList();

        // blocked squares / goal
        int nodeIndex = 0;
        var goalY = Map.WorldToGrid(new(0, 0, EndZ)).y;
        for (int y = ScrollY, ymax = Math.Min(Map.Height, ScrollY + h); y < ymax; ++y)
        {
            for (int x = 0; x < Map.Width; ++x, ++nodeIndex)
            {
                var corner = tl + new Vector2(x, y - ScrollY) * ScreenPixelSize;
                var cornerEnd = corner + new Vector2(ScreenPixelSize, ScreenPixelSize);

                var blocker = Map[x, y, (int)(CurrT * Map.InvTimeResolution)];
                var reachable = false;// _pathfind[x, y, CurrT];
                if (reachable)
                {
                    var alpha = 0.5f; // 1 - (pix.MaxG > 0 ? pix.MaxG / Map.MaxG : 0);
                    uint c = 128 + (uint)(alpha * 127);
                    c = c | (c << 8) | 0xff000000;
                    dl.AddRectFilled(corner, cornerEnd, c);
                }
                else if (y == goalY)
                {
                    var alpha = 1;// pix.Priority / Map.MaxPriority;
                    uint c = 128 + (uint)(alpha * 127);
                    c = (c << 8) | 0xff000000;
                    dl.AddRectFilled(corner, cornerEnd, c);
                }
                else if (blocker)
                {
                    dl.AddRectFilled(corner, cornerEnd, 0x80808080);
                }
            }
        }

        // border
        dl.AddLine(tl, tr, 0xffffffff, 2);
        dl.AddLine(tr, br, 0xffffffff, 2);
        dl.AddLine(br, bl, 0xffffffff, 2);
        dl.AddLine(bl, tl, 0xffffffff, 2);

        // grid
        if (ScreenPixelSize > 4)
        {
            for (int x = 1; x < Map.Width; ++x)
            {
                var off = new Vector2(x * ScreenPixelSize, 0);
                dl.AddLine(tl + off, bl + off, 0xffffffff, 1);
            }
            for (int y = 1; y < h; ++y)
            {
                var off = new Vector2(0, y * ScreenPixelSize);
                dl.AddLine(tl + off, tr + off, 0xffffffff, 1);
            }
        }

        // pathfinding
        ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
        //if (ImGui.Button("Reset pf"))
        //    ResetPathfind();
        //ImGui.SameLine();
        //if (ImGui.Button("Step pf"))
        //    StepPathfind();
        //ImGui.SameLine();
        //if (ImGui.Button("Run pf"))
        //    RunPathfind();

        ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
        ImGui.SetNextItemWidth(500);
        ImGui.SliderFloat("Time step", ref CurrT, 0, MaxT);
        ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
        ImGui.SetNextItemWidth(500);
        ImGui.SliderInt("Scroll", ref ScrollY, 0, Map.Height - h);
        ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
        ImGui.SetNextItemWidth(500);
        ImGui.SliderFloat("Cell size", ref ScreenPixelSize, 1, 20);

        if (Path.Count > 0)
        {
            var from = Map.WorldToGrid(Path[0].XYZ());
            var fromY = from.y - ScrollY;
            var fromS = tl + new Vector2(from.x + 0.5f, fromY + 0.5f) * ScreenPixelSize;
            //if (CurrT == 0 && fromY >= 0 && fromY <= h)
            //    dl.AddCircle(fromS, ScreenPixelSize / 2, 0xffff0000);
            for (int i = 1; i < Path.Count; ++i)
            {
                var to = Map.WorldToGrid(Path[i].XYZ());
                var toY = to.y - ScrollY;
                var toS = tl + new Vector2(to.x + 0.5f, toY + 0.5f) * ScreenPixelSize;
                if (fromY >= 0 && fromY <= h && toY >= 0 && toY <= h)
                    dl.AddLine(fromS, toS, Path[i].W <= CurrT ? 0x80800080 : 0xffff00ff, 2);
                from = to;
                fromY = toY;
                fromS = toS;
                //if (CurrT == i && fromY >= 0 && fromY <= h)
                //    dl.AddCircle(fromS, ScreenPixelSize / 2, 0xffff0000);
            }
        }

        ImGui.SetCursorPos(cursorEnd);
    }
}
