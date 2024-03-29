using ImGuiNET;

namespace BossMod;

// column showing a history of some events (actions, casts, statuses, etc.)
// entry is attached to a node (this is important if timings are adjusted for any reason)
public class ColumnGenericHistory : Timeline.Column
{
    public class Entry
    {
        public enum Type { Dot, Line, Range }

        public Type EntryType;
        public StateMachineTree.Node AttachNode;
        public float Delay; // from node's predecessor time
        public float Duration; // used for hover tests to display tooltip
        public string Name;
        public uint Color;
        public float WidthRel;
        public List<string> TooltipExtra = new();

        public float TimeSincePhaseStart() => (AttachNode.Predecessor?.Time ?? 0) + Delay;
        public float TimeSinceGlobalStart(StateMachineTree tree) => tree.Phases[AttachNode.PhaseID].StartTime + TimeSincePhaseStart();

        public Entry(Type type, StateMachineTree.Node attachNode, float delay, float duration, string name, uint color, float widthRel = 1.0f)
        {
            EntryType = type;
            AttachNode = attachNode;
            Delay = delay;
            Duration = duration;
            Name = name;
            Color = color;
            WidthRel = widthRel;
        }
    }

    public const float DefaultWidth = 10;

    public List<Entry> Entries = new();
    public StateMachineTree Tree { get; private init; }
    public List<int> PhaseBranches { get; private init; }

    private float _trackHalfWidth = 5;
    private float _eventRadius = 4;

    private uint _colBackground = 0x40404040;

    public ColumnGenericHistory(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, string name = "")
        : base(timeline)
    {
        Tree = tree;
        PhaseBranches = phaseBranches;
        Width = DefaultWidth;
        Name = name;
    }

    public override void Draw()
    {
        DrawEntries();
        DrawHover();
    }

    protected bool IsEntryVisible(Entry e)
    {
        int branchID = Tree.Phases[e.AttachNode.PhaseID].StartingNode.BranchID + PhaseBranches[e.AttachNode.PhaseID];
        return branchID >= e.AttachNode.BranchID && branchID < e.AttachNode.BranchID + e.AttachNode.NumBranches;
    }

    protected void DrawEntries()
    {
        var drawlist = ImGui.GetWindowDrawList();

        var trackMin = Timeline.ColumnCoordsToScreenCoords(Width / 2 - _trackHalfWidth, Timeline.MinVisibleTime);
        var trackMax = Timeline.ColumnCoordsToScreenCoords(Width / 2 + _trackHalfWidth, Timeline.MaxVisibleTime);
        drawlist.AddRectFilled(trackMin, trackMax, _colBackground);

        foreach (var e in Entries.Where(e => e.EntryType == Entry.Type.Range && IsEntryVisible(e)))
        {
            var eStart = e.TimeSinceGlobalStart(Tree);
            var yStart = Timeline.TimeToScreenCoord(eStart);
            var yEnd = Timeline.TimeToScreenCoord(eStart + e.Duration);
            drawlist.AddRectFilled(new(trackMin.X, yStart), new(Utils.Lerp(trackMin.X, trackMax.X, e.WidthRel), yEnd), e.Color);
        }

        foreach (var e in Entries.Where(e => e.EntryType == Entry.Type.Line && IsEntryVisible(e)))
        {
            var y = Timeline.TimeToScreenCoord(e.TimeSinceGlobalStart(Tree));
            drawlist.AddLine(new(trackMin.X, y), new(Utils.Lerp(trackMin.X, trackMax.X, e.WidthRel), y), e.Color);
        }

        foreach (var e in Entries.Where(e => e.EntryType == Entry.Type.Dot && IsEntryVisible(e)))
        {
            var y = Timeline.TimeToScreenCoord(e.TimeSinceGlobalStart(Tree));
            drawlist.AddCircleFilled(new(Utils.Lerp(trackMin.X, trackMax.X, e.WidthRel * 0.5f), y), _eventRadius, e.Color);
        }
    }

    protected void DrawHover()
    {
        var mousePos = ImGui.GetMousePos();
        if (!ScreenPosInTrack(mousePos))
            return;

        foreach (var e in Entries.Where(e => (e.Name.Length > 0 || e.TooltipExtra.Count > 0) && ScreenPosInEntry(mousePos, e)))
        {
            Timeline.AddTooltip(EntryTooltip(e));
            HighlightEntry(e);
        }
    }

    protected void HighlightEntry(Entry e)
    {
        var tStart = e.TimeSinceGlobalStart(Tree);
        Timeline.HighlightTime(tStart);
        Timeline.HighlightTime(tStart + e.Duration);
    }

    protected bool ScreenPosInTrack(Vector2 pos)
    {
        var ox = Timeline.ScreenCoordToColumnOffset(pos.X) - Width / 2; // distance from column center
        if (Math.Abs(ox) > _trackHalfWidth)
            return false;
        if (pos.Y < Timeline.ScreenClientTL.Y || pos.Y > Timeline.ScreenClientTL.Y + Timeline.Height)
            return false;
        return true;
    }

    protected bool ScreenPosInEntry(Vector2 pos, Entry e)
    {
        if (!IsEntryVisible(e))
            return false;
        var tStart = e.TimeSinceGlobalStart(Tree);
        var yMin = Timeline.TimeToScreenCoord(tStart);
        var yMax = Timeline.TimeToScreenCoord(tStart + e.Duration);
        return pos.Y >= (yMin - 4) && pos.Y <= (yMax + 4);
    }

    private List<string> EntryTooltip(Entry e)
    {
        List<string> res = new();
        res.Add($"{e.TimeSinceGlobalStart(Tree):f3}: {e.Name}");
        res.AddRange(e.TooltipExtra);
        return res;
    }
}
