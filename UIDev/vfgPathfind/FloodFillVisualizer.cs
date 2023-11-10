using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UIDev.vfgPathfind;

public class FloodFillVisualizer
{
    public Map Map;
    public Vector3 StartPos;
    public float EndZ;
    public float ScreenPixelSize = 2.0f;
    public int CurrT;
    public List<(int x, int z, int t)> Path = new();
    public int ScrollY;

    private FloodFill _pathfind;

    public FloodFillVisualizer(Map map, Vector3 startPos, float endZ)
    {
        Map = map;
        StartPos = startPos;
        EndZ = endZ;
        _pathfind = BuildPathfind();
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

                var blocker = Map[x, y, CurrT];
                var reachable = _pathfind[x, y, CurrT];
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
        if (ImGui.Button("Reset pf"))
            ResetPathfind();
        ImGui.SameLine();
        if (ImGui.Button("Step pf"))
            StepPathfind();
        ImGui.SameLine();
        if (ImGui.Button("Run pf"))
            RunPathfind();

        ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
        ImGui.SetNextItemWidth(500);
        ImGui.SliderInt("Time step", ref CurrT, 0, _pathfind.NextT - 1);
        ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
        ImGui.SetNextItemWidth(500);
        ImGui.SliderInt("Scroll", ref ScrollY, 0, Map.Height - h);
        ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
        ImGui.SetNextItemWidth(500);
        ImGui.SliderFloat("Cell size", ref ScreenPixelSize, 1, 20);

        if (Path.Count > 0)
        {
            var fromY = Path[0].z - ScrollY;
            var from = tl + new Vector2(Path[0].x + 0.5f, fromY + 0.5f) * ScreenPixelSize;
            if (Path[0].t == CurrT && fromY >= 0 && fromY <= h)
                dl.AddCircle(from, ScreenPixelSize / 2, 0xffff0000);
            for (int i = 1; i < Path.Count; ++i)
            {
                var toY = Path[i].z - ScrollY;
                var to = tl + new Vector2(Path[i].x + 0.5f, toY + 0.5f) * ScreenPixelSize;
                if (fromY >= 0 && fromY <= h && toY >= 0 && toY <= h)
                    dl.AddLine(from, to, Path[i].t > CurrT ? 0x80800080 : 0xffff00ff, 2);
                fromY = toY;
                from = to;
                if (Path[i].t == CurrT && fromY >= 0 && fromY <= h)
                    dl.AddCircle(from, ScreenPixelSize / 2, 0xffff0000);
            }
        }

        ImGui.SetCursorPos(cursorEnd);
    }

    public void StepPathfind()
    {
        _pathfind.ExecuteStep();
        CurrT = _pathfind.NextT - 1;
    }

    public void RunPathfind()
    {
        Path.Clear();
        Path.AddRange(_pathfind.SolveUntilZ(EndZ));
        CurrT = _pathfind.NextT - 1;
    }

    public void ResetPathfind()
    {
        _pathfind = BuildPathfind();
        CurrT = _pathfind.NextT - 1;
    }

    private FloodFill BuildPathfind() => new(Map, 6, StartPos);
}
