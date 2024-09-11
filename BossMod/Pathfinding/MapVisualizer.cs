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
    private float _lastExecTime;

    public MapVisualizer(Map map, WPos startPos, WPos goalPos, float goalRadius)
    {
        Map = map;
        StartPos = startPos;
        GoalPos = goalPos;
        GoalRadius = goalRadius;
        _pathfind = BuildPathfind();
        ExecTimed(() => _pathfind.Execute());
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

                var pixMaxG = Map.PixelMaxG[nodeIndex];
                var pixPriority = Map.PixelPriority[nodeIndex];
                if (pixMaxG < float.MaxValue)
                {
                    var alpha = 1 - (pixMaxG > 0 ? pixMaxG / Map.MaxG : 0);
                    uint c = 128 + (uint)(alpha * 127);
                    c = c | (c << 8) | 0xff000000;
                    dl.AddRectFilled(corner, cornerEnd, c);
                }
                else if (pixPriority > 0)
                {
                    var alpha = Map.MaxPriority > 0 ? pixPriority / Map.MaxPriority : 1;
                    uint c = 128 + (uint)(alpha * 127);
                    c = (c << 8) | 0xff000000;
                    dl.AddRectFilled(corner, cornerEnd, c);
                }
                else if (pixPriority < 0)
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

        DrawPath(dl, tl, hoverNode >= 0 ? hoverNode : _pathfind.BestIndex());

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
            var pixMaxG = Map.PixelMaxG[hoverNode];
            var pixPriority = Map.PixelPriority[hoverNode];
            if (pixMaxG < float.MaxValue)
            {
                ImGui.TextUnformatted($"Pixel at {x}x{y} ({wpos}): blocked, g={pixMaxG:f3}");
            }
            else if (pixPriority != 0)
            {
                ImGui.TextUnformatted($"Pixel at {x}x{y} ({wpos}): goal, prio={pixPriority}");
            }
            else
            {
                ImGui.TextUnformatted($"Pixel at {x}x{y} ({wpos}): normal");
            }

            ref var pfNode = ref _pathfind.NodeByIndex(hoverNode);
            if (pfNode.OpenHeapIndex != 0)
            {
                var (parentX, parentY) = Map.IndexToGrid(pfNode.ParentIndex);
                ImGui.TextUnformatted($"PF: g={pfNode.GScore:f3}, h={pfNode.HScore:f3}, g+h={pfNode.FScore:f3}, score={pfNode.Score}, parent={parentX}x{parentY}, index={pfNode.OpenHeapIndex}, leeway={pfNode.PathLeeway}, off={pfNode.EnterOffset}");

                //if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                //{
                //    var res = _pathfind.IsLeftBetter(ref _pathfind.NodeByIndex(hoverNode), ref Map.Pixels[hoverNode], ref _pathfind.NodeByIndex(_pathfind.BestIndex), ref Map.Pixels[_pathfind.BestIndex]);
                //}

                ref var pfParent = ref _pathfind.NodeByIndex(pfNode.ParentIndex);
                var (grandParentX, grandParentY) = Map.IndexToGrid(pfParent.ParentIndex);
                var losLeeway = _pathfind.LineOfSight(grandParentX, grandParentY, _pathfind.NodeByIndex(pfParent.ParentIndex).EnterOffset, x, y, out var grandParentOffset, out var grandParentDist, out var grandParentMinG);
                ImGui.TextUnformatted($"PF: grandparent={grandParentX}x{grandParentY}, dist={grandParentDist}, losLeeway={losLeeway}, off={grandParentOffset}, minG={grandParentMinG}");
            }
        }

        // pathfinding
        if (ImGui.Button("Reset pf"))
            ExecTimed(() => _pathfind = BuildPathfind());
        ImGui.SameLine();
        if (ImGui.Button("Step pf"))
            ExecTimed(() => _pathfind.ExecuteStep());
        ImGui.SameLine();
        if (ImGui.Button("Run pf"))
            ExecTimed(() => _pathfind.Execute());
        ImGui.SameLine();
        if (ImGui.Button("Step back") && _pathfind.NumSteps > 0)
            ExecTimed(() =>
            {
                var s = _pathfind.NumSteps - 1;
                _pathfind = BuildPathfind();
                while (_pathfind.NumSteps < s && _pathfind.ExecuteStep())
                    ;
            });
        ImGui.SameLine();
        if (ImGui.Button("Run until reopen"))
            ExecTimed(() =>
            {
                var startR = _pathfind.NumReopens;
                while (_pathfind.ExecuteStep() && _pathfind.NumReopens == startR)
                    ;
            });
        ImGui.SameLine();
        if (ImGui.Button("Step x100"))
            ExecTimed(() =>
            {
                var cntr = 0;
                while (_pathfind.ExecuteStep() && ++cntr < 100)
                    ;
            });
        ImGui.SameLine();
        ImGui.TextUnformatted($"Last op: {_lastExecTime:f3}s, num steps: {_pathfind.NumSteps}, num reopens: {_pathfind.NumReopens}");

        var pfRes = _pathfind.BestIndex();
        if (pfRes >= 0)
        {
            ref var node = ref _pathfind.NodeByIndex(pfRes);
            ImGui.TextUnformatted($"Path length: {node.GScore:f3} to {_pathfind.CellCenter(pfRes)}, leeway={node.PathLeeway}");
        }

        using (var n = ImRaii.TreeNode("Waypoints"))
            if (n)
                DrawWaypoints(hoverNode >= 0 ? hoverNode : _pathfind.BestIndex());
    }

    private void ExecTimed(Action action)
    {
        var now = DateTime.Now;
        action();
        _lastExecTime = (float)(DateTime.Now - now).TotalSeconds;
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

    private void DrawPath(ImDrawListPtr dl, Vector2 tl, int startingIndex)
    {
        if (startingIndex < 0)
            return;

        ref var startingNode = ref _pathfind.NodeByIndex(startingIndex);
        if (startingNode.OpenHeapIndex == 0)
            return;

        var color = 0xffffff00;
        var nextIndex = startingNode.ParentIndex;
        var (x1, y1) = Map.IndexToGrid(startingIndex);
        var (x2, y2) = Map.IndexToGrid(startingNode.ParentIndex);
        while (x1 != x2 || y1 != y2)
        {
            var off1 = _pathfind.NodeByIndex(startingIndex).EnterOffset;
            var off2 = _pathfind.NodeByIndex(nextIndex).EnterOffset;
            dl.AddLine(tl + new Vector2(x1 + 0.5f + off1.X, y1 + 0.5f + off1.Y) * ScreenPixelSize, tl + new Vector2(x2 + 0.5f + off2.X, y2 + 0.5f + off2.Y) * ScreenPixelSize, color, 2);
            color = 0xffff00ff;
            startingIndex = nextIndex;
            nextIndex = _pathfind.NodeByIndex(startingIndex).ParentIndex;
            (x1, y1) = (x2, y2);
            (x2, y2) = Map.IndexToGrid(nextIndex);
        }
    }

    private void DrawWaypoints(int startingIndex)
    {
        if (startingIndex < 0)
            return;

        ref var startingNode = ref _pathfind.NodeByIndex(startingIndex);
        if (startingNode.OpenHeapIndex == 0)
            return;

        var nextIndex = startingNode.ParentIndex;
        var (x1, y1) = Map.IndexToGrid(startingIndex);
        var (x2, y2) = Map.IndexToGrid(nextIndex);
        while (x1 != x2 || y1 != y2)
        {
            ref var node = ref _pathfind.NodeByIndex(startingIndex);
            var off1 = node.EnterOffset;
            using var n = ImRaii.TreeNode($"Waypoint: {x1}x{y1} ({Map.GridToWorld(x1, y1, off1.X + 0.5f, off1.Y + 0.5f)}), minG={node.PathMinG}, leeway={node.PathLeeway}", ImGuiTreeNodeFlags.Leaf);
            x1 = x2;
            y1 = y2;
            startingIndex = nextIndex;
            nextIndex = node.ParentIndex;
            (x1, y1) = (x2, y2);
            (x2, y2) = Map.IndexToGrid(nextIndex);
        }
    }

    private ThetaStar BuildPathfind()
    {
        var res = new ThetaStar();
        res.Start(Map, StartPos, GoalPos, GoalRadius, 1.0f / 6);
        return res;
    }
}
