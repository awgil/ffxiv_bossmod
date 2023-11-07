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
    //public List<(WPos center, float ir, float or, Angle dir, Angle halfWidth)> Sectors = new();
    //public List<(WPos origin, float lenF, float lenB, float halfWidth, Angle dir)> Rects = new();
    //public List<(WPos origin, WPos dest)> Lines = new();

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
        var goalZ = Map.WorldToGrid(new(0, 0, EndZ)).z;
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
                else if (y == goalZ)
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

                //ref var pfNode = ref _pathfind.NodeByIndex(nodeIndex);
                //if (pfNode.OpenHeapIndex != 0)
                //{
                //    dl.AddCircle((corner + cornerEnd) / 2, 3, pfNode.OpenHeapIndex < 0 ? 0xff0000ff : 0xffff0080, 0, pfNode.OpenHeapIndex == 1 ? 2 : 1);
                //}

                //if (ImGui.IsMouseHoveringRect(corner, cornerEnd))
                //{
                //    ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
                //    if (pix.MaxG < float.MaxValue)
                //    {
                //        ImGui.TextUnformatted($"Pixel at {x}x{y}: blocked, g={pix.MaxG:f3}");
                //    }
                //    else if (pix.Priority != 0)
                //    {
                //        ImGui.TextUnformatted($"Pixel at {x}x{y}: goal, prio={pix.Priority}");
                //    }
                //    else
                //    {
                //        ImGui.TextUnformatted($"Pixel at {x}x{y}: normal");
                //    }

                //    if (pfNode.OpenHeapIndex != 0)
                //    {
                //        ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
                //        ImGui.TextUnformatted($"PF: g={pfNode.GScore:f3}, h={pfNode.HScore:f3}, g+h={pfNode.GScore + pfNode.HScore:f3}, parent={pfNode.ParentX}x{pfNode.ParentY}, index={pfNode.OpenHeapIndex}, leeway={pfNode.PathLeeway:f3}");

                //        pfPathNode = nodeIndex;
                //    }
                //}
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

        //var pfRes = _pathfind.CurrentResult();
        //if (pfRes >= 0)
        //{
        //    ImGui.SetCursorPosX(cursorEnd.X + Map.Width * ScreenPixelSize + 10);
        //    ImGui.TextUnformatted($"Path length: {_pathfind.NodeByIndex(pfRes).GScore:f3}");
        //}

        //if (pfPathNode == -1)
        //    pfPathNode = _pathfind.CurrentResult();
        //if (pfPathNode >= 0)
        //    DrawPath(dl, tl, pfPathNode);

        // shapes
        //foreach (var c in Sectors)
        //{
        //    DrawSector(dl, tl, c.center, c.ir, c.or, c.dir, c.halfWidth);
        //}
        //foreach (var r in Rects)
        //{
        //    var direction = r.dir.ToDirection();
        //    var side = r.halfWidth * direction.OrthoR();
        //    var front = r.origin + r.lenF * direction;
        //    var back = r.origin - r.lenB * direction;
        //    dl.AddQuad(tl + Map.WorldToGridFrac(front + side) * ScreenPixelSize, tl + Map.WorldToGridFrac(front - side) * ScreenPixelSize, tl + Map.WorldToGridFrac(back - side) * ScreenPixelSize, tl + Map.WorldToGridFrac(back + side) * ScreenPixelSize, 0xff0000ff);
        //}
        //foreach (var l in Lines)
        //{
        //    dl.AddLine(tl + Map.WorldToGridFrac(l.origin) * ScreenPixelSize, tl + Map.WorldToGridFrac(l.dest) * ScreenPixelSize, 0xff0000ff);
        //}

        ImGui.SetCursorPos(cursorEnd);
    }

    public void StepPathfind()
    {
        _pathfind.ExecuteStep();
        CurrT = _pathfind.NextT - 1;
    }

    public void RunPathfind()
    {
        var gz = Map.WorldToGrid(new(0, 0, EndZ)).z;
        while (_pathfind.ExecuteStep() is var res && res.t >= 0)
        {
            if (res.minZ <= gz)
            {
                var minX = Enumerable.Range(0, Map.Width).First(x => _pathfind[x, res.minZ, res.t]);
                BuildSolution(minX, res.minZ, res.t);
                break;
            }
        }
        CurrT = _pathfind.NextT - 1;
    }

    public void ResetPathfind()
    {
        _pathfind = BuildPathfind();
        CurrT = _pathfind.NextT - 1;
    }

    //private void DrawSector(ImDrawListPtr dl, Vector2 tl, WPos center, float ir, float or, Angle dir, Angle halfWidth)
    //{
    //    if (halfWidth.Rad <= 0 || or <= 0 || ir >= or)
    //        return;

    //    var sCenter = tl + Map.WorldToGridFrac(center) * ScreenPixelSize;
    //    if (halfWidth.Rad >= MathF.PI)
    //    {
    //        dl.AddCircle(sCenter, or / Map.Resolution * ScreenPixelSize, 0xff0000ff);
    //        if (ir > 0)
    //            dl.AddCircle(sCenter, ir / Map.Resolution * ScreenPixelSize, 0xff0000ff);
    //    }
    //    else
    //    {
    //        float sDir = MathF.PI / 2 - dir.Rad;
    //        dl.PathArcTo(sCenter, ir / Map.Resolution * ScreenPixelSize, sDir + halfWidth.Rad, sDir - halfWidth.Rad);
    //        dl.PathArcTo(sCenter, or / Map.Resolution * ScreenPixelSize, sDir - halfWidth.Rad, sDir + halfWidth.Rad);
    //        dl.PathStroke(0xff0000ff, ImDrawFlags.Closed, 1);
    //    }
    //}

    //private void DrawPath(ImDrawListPtr dl, Vector2 tl, int startingIndex)
    //{
    //    if (startingIndex < 0)
    //        return;

    //    int from = startingIndex;
    //    int x1 = startingIndex % Map.Width;
    //    int y1 = startingIndex / Map.Width;
    //    int x2 = _pathfind.NodeByIndex(from).ParentX;
    //    int y2 = _pathfind.NodeByIndex(from).ParentY;
    //    while (x1 != x2 || y1 != y2)
    //    {
    //        dl.AddLine(tl + new Vector2(x1 + 0.5f, y1 + 0.5f) * ScreenPixelSize, tl + new Vector2(x2 + 0.5f, y2 + 0.5f) * ScreenPixelSize, 0xffff00ff, 2);
    //        x1 = x2;
    //        y1 = y2;
    //        from = y1 * Map.Width + x1;
    //        x2 = _pathfind.NodeByIndex(from).ParentX;
    //        y2 = _pathfind.NodeByIndex(from).ParentY;
    //    }
    //}

    private FloodFill BuildPathfind() => new(Map, 6, StartPos);

    private void BuildSolution(int x, int z, int t)
    {
        Path.Clear();
        while (true)
        {
            Path.Add((x, z, t));
            var move = _pathfind.FindMove(x, z, t);
            if (move == null)
                break;
            x = move.Value.x;
            z = move.Value.z;
            --t;
        }
    }
}
