using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Pathfinding;

public class MapVisualizer
{
    public Map Map;
    public WPos StartPos;
    public WPos GoalPos;
    public float GoalRadius;
    public float ScreenPixelSize = 12;
    public List<(WPos center, float ir, float or, Angle dir, Angle halfWidth)> Sectors = [];
    public List<(WPos origin, float lenF, float lenB, float halfWidth, Angle dir)> Rects = [];
    public List<(WPos origin, WPos dest)> Lines = [];

    private ThetaStar _pathfind;

    public MapVisualizer(Map map, WPos startPos, WPos goalPos, float goalRadius)
    {
        Map = map;
        StartPos = startPos;
        GoalPos = goalPos;
        GoalRadius = goalRadius;
        _pathfind = BuildPathfind();
        RunPathfind();
    }

    public void Draw()
    {
        using var table = ImRaii.Table("table", 2);
        if (!table)
            return;

        var size = new Vector2(Map.Width, Map.Height) * ScreenPixelSize;
        ImGui.TableSetupColumn("Map", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoClip, size.X);
        ImGui.TableSetupColumn("Control");

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        var tl = ImGui.GetCursorScreenPos();
        var br = tl + size;
        var tr = new Vector2(br.X, tl.Y);
        var bl = new Vector2(tl.X, br.Y);
        var dl = ImGui.GetWindowDrawList();

        ImGui.Dummy(size);

        // blocked squares / goal
        int nodeIndex = 0;
        int hoverNode = -1;
        for (int y = 0; y < Map.Height; ++y)
        {
            for (int x = 0; x < Map.Width; ++x, ++nodeIndex)
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
                else if (pix.Priority > 0)
                {
                    var alpha = Map.MaxPriority > 0 ? pix.Priority / Map.MaxPriority : 1;
                    uint c = 128 + (uint)(alpha * 127);
                    c = (c << 8) | 0xff000000;
                    dl.AddRectFilled(corner, cornerEnd, c);
                }
                else if (pix.Priority < 0)
                {
                    dl.AddRectFilled(corner, cornerEnd, 0xff808080);
                }

                ref var pfNode = ref _pathfind.NodeByIndex(nodeIndex);
                if (pfNode.OpenHeapIndex != 0)
                {
                    dl.AddCircle((corner + cornerEnd) / 2, ScreenPixelSize * 0.5f - 2, pfNode.OpenHeapIndex < 0 ? 0xff0000ff : 0xffff0080, 0, pfNode.OpenHeapIndex == 1 ? 2 : 1);
                }

                if (ImGui.IsMouseHoveringRect(corner, cornerEnd))
                {
                    hoverNode = nodeIndex;
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

        DrawPath(dl, tl, hoverNode >= 0 ? hoverNode : _pathfind.BestIndex);

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
        foreach (var l in Lines)
        {
            dl.AddLine(tl + Map.WorldToGridFrac(l.origin) * ScreenPixelSize, tl + Map.WorldToGridFrac(l.dest) * ScreenPixelSize, 0xff0000ff);
        }

        ImGui.TableNextColumn();

        if (hoverNode >= 0)
        {
            var (x, y) = Map.IndexToGrid(hoverNode);
            var wpos = Map.GridToWorld(x, y, 0.5f, 0.5f);
            var pix = Map[x, y];
            if (pix.MaxG < float.MaxValue)
            {
                ImGui.TextUnformatted($"Pixel at {x}x{y} ({wpos}): blocked, g={pix.MaxG:f3}");
            }
            else if (pix.Priority != 0)
            {
                ImGui.TextUnformatted($"Pixel at {x}x{y} ({wpos}): goal, prio={pix.Priority}");
            }
            else
            {
                ImGui.TextUnformatted($"Pixel at {x}x{y} ({wpos}): normal");
            }

            ref var pfNode = ref _pathfind.NodeByIndex(hoverNode);
            if (pfNode.OpenHeapIndex != 0)
            {
                ImGui.TextUnformatted($"PF: g={pfNode.GScore:f3}, h={pfNode.HScore:f3}, g+h={pfNode.GScore + pfNode.HScore:f3}, parent={pfNode.ParentX}x{pfNode.ParentY}, index={pfNode.OpenHeapIndex}, leeway={pfNode.PathLeeway}, off={pfNode.EnterOffset}");

                //if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                //{
                //    var res = _pathfind.IsLeftBetter(ref _pathfind.NodeByIndex(hoverNode), ref Map.Pixels[hoverNode], ref _pathfind.NodeByIndex(_pathfind.BestIndex), ref Map.Pixels[_pathfind.BestIndex]);
                //}

                ref var pfParent = ref _pathfind.NodeByIndex(Map.GridToIndex(pfNode.ParentX, pfNode.ParentY));
                var grandParentIndex = Map.GridToIndex(pfParent.ParentX, pfParent.ParentY);
                var losLeeway = _pathfind.LineOfSight(pfParent.ParentX, pfParent.ParentY, _pathfind.NodeByIndex(grandParentIndex).EnterOffset, x, y, out var grandParentOffset, out var grandParentDist);
                ImGui.TextUnformatted($"PF: grandparent={pfParent.ParentX}x{pfParent.ParentY}, dist={grandParentDist}, losLeeway={losLeeway}, off={grandParentOffset}");
            }
        }

        // pathfinding
        if (ImGui.Button("Reset pf"))
            ResetPathfind();
        ImGui.SameLine();
        if (ImGui.Button("Step pf"))
            StepPathfind();
        ImGui.SameLine();
        if (ImGui.Button("Run pf"))
            RunPathfind();

        var pfRes = _pathfind.BestIndex;
        if (pfRes >= 0)
        {
            ref var node = ref _pathfind.NodeByIndex(pfRes);
            ImGui.TextUnformatted($"Path length: {node.GScore:f3} to {_pathfind.CellCenter(pfRes)}, leeway={node.PathLeeway}");
        }

        using (var n = ImRaii.TreeNode("Waypoints"))
            if (n)
                DrawWaypoints(hoverNode >= 0 ? hoverNode : _pathfind.BestIndex);
    }

    public void StepPathfind() => _pathfind.ExecuteStep();
    public void RunPathfind() => _pathfind.Execute();
    public void ResetPathfind() => _pathfind = BuildPathfind();

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

    private void DrawPath(ImDrawListPtr dl, Vector2 tl, int startingIndex)
    {
        if (startingIndex < 0)
            return;

        ref var startingNode = ref _pathfind.NodeByIndex(startingIndex);
        if (startingNode.OpenHeapIndex == 0)
            return;

        var color = 0xffffff00;
        var (x1, y1) = Map.IndexToGrid(startingIndex);
        int x2 = startingNode.ParentX;
        int y2 = startingNode.ParentY;
        while (x1 != x2 || y1 != y2)
        {
            var to = Map.GridToIndex(x2, y2);
            var off1 = _pathfind.NodeByIndex(startingIndex).EnterOffset;
            var off2 = _pathfind.NodeByIndex(to).EnterOffset;
            dl.AddLine(tl + new Vector2(x1 + 0.5f + off1.X, y1 + 0.5f + off1.Y) * ScreenPixelSize, tl + new Vector2(x2 + 0.5f + off2.X, y2 + 0.5f + off2.Y) * ScreenPixelSize, color, 2);
            x1 = x2;
            y1 = y2;
            startingIndex = to;
            color = 0xffff00ff;
            x2 = _pathfind.NodeByIndex(startingIndex).ParentX;
            y2 = _pathfind.NodeByIndex(startingIndex).ParentY;
        }
    }

    private void DrawWaypoints(int startingIndex)
    {
        if (startingIndex < 0)
            return;

        ref var startingNode = ref _pathfind.NodeByIndex(startingIndex);
        if (startingNode.OpenHeapIndex == 0)
            return;

        var (x1, y1) = Map.IndexToGrid(startingIndex);
        int x2 = startingNode.ParentX;
        int y2 = startingNode.ParentY;
        while (x1 != x2 || y1 != y2)
        {
            var to = Map.GridToIndex(x2, y2);
            ref var node = ref _pathfind.NodeByIndex(startingIndex);
            var off1 = node.EnterOffset;
            using var n = ImRaii.TreeNode($"Waypoint: {x1}x{y1} ({Map.GridToWorld(x1, y1, off1.X + 0.5f, off1.Y + 0.5f)}), minG={node.PathMinG}, leeway={node.PathLeeway}", ImGuiTreeNodeFlags.Leaf);
            x1 = x2;
            y1 = y2;
            startingIndex = to;
            x2 = _pathfind.NodeByIndex(startingIndex).ParentX;
            y2 = _pathfind.NodeByIndex(startingIndex).ParentY;
        }
    }

    private ThetaStar BuildPathfind()
    {
        var res = new ThetaStar();
        res.Start(Map, StartPos, GoalPos, GoalRadius, 1.0f / 6);
        return res;
    }
}
