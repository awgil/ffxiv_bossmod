using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    // column showing a history of some events (actions, casts, statuses, etc.)
    // entry is attached to a node (this is important if timings are adjusted for any reason)
    public class GenericHistoryColumn : Timeline.Column
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
            public List<string> TooltipExtra = new();

            public float TimeSincePhaseStart() => (AttachNode.Predecessor?.Time ?? 0) + Delay;
            public float TimeSinceGlobalStart(StateMachineTree tree) => tree.Phases[AttachNode.PhaseID].StartTime + TimeSincePhaseStart();

            public Entry(Type type, StateMachineTree.Node attachNode, float delay, float duration, string name, uint color)
            {
                EntryType = type;
                AttachNode = attachNode;
                Delay = delay;
                Duration = duration;
                Name = name;
                Color = color;
            }
        }

        public List<Entry> Entries = new();
        public StateMachineTree Tree { get; private init; }
        public List<int> PhaseBranches { get; private init; }

        private float _trackHalfWidth = 5;
        private float _eventRadius = 4;

        private uint _colBackground = 0x40404040;

        public GenericHistoryColumn(Timeline timeline, StateMachineTree tree, List<int> phaseBranches)
            : base(timeline)
        {
            Tree = tree;
            PhaseBranches = phaseBranches;
            Width = 10;
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

            foreach (var e in Entries.Where(IsEntryVisible))
            {
                var eStart = e.TimeSinceGlobalStart(Tree);
                var yStart = Timeline.TimeToScreenCoord(eStart);
                var yEnd = Timeline.TimeToScreenCoord(eStart + e.Duration);
                switch (e.EntryType)
                {
                    case Entry.Type.Dot:
                        drawlist.AddCircleFilled(new((trackMin.X + trackMax.X) * 0.5f, yStart), _eventRadius, e.Color);
                        break;
                    case Entry.Type.Line:
                        drawlist.AddLine(new(trackMin.X, yStart), new(trackMax.X, yStart), e.Color);
                        break;
                    case Entry.Type.Range:
                        drawlist.AddRectFilled(new(trackMin.X, yStart), new(trackMax.X, yEnd), e.Color);
                        break;
                }
            }
        }

        protected void DrawHover()
        {
            var mousePos = ImGui.GetMousePos();
            var trackMin = Timeline.ColumnCoordsToScreenCoords(Width / 2 - _trackHalfWidth, Timeline.MinVisibleTime);
            var trackMax = Timeline.ColumnCoordsToScreenCoords(Width / 2 + _trackHalfWidth, Timeline.MaxVisibleTime);
            if (!PointInRect(mousePos, trackMin, trackMax))
                return;

            foreach (var e in Entries.Where(IsEntryVisible))
            {
                var tStart = e.TimeSinceGlobalStart(Tree);
                var yMin = Timeline.TimeToScreenCoord(tStart);
                var yMax = Timeline.TimeToScreenCoord(tStart + e.Duration);
                if (mousePos.Y >= (yMin - 4) && mousePos.Y <= (yMax + 4))
                {
                    Timeline.AddTooltip(EntryTooltip(e));
                    HighlightEntry(e);
                }
            }
        }

        protected void HighlightEntry(Entry e)
        {
            var tStart = e.TimeSinceGlobalStart(Tree);
            Timeline.HighlightTime(tStart);
            Timeline.HighlightTime(tStart + e.Duration);
        }

        private static bool PointInRect(Vector2 point, Vector2 min, Vector2 max) => point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y;

        private List<string> EntryTooltip(Entry e)
        {
            List<string> res = new();
            res.Add($"{e.TimeSinceGlobalStart(Tree):f1}: {e.Name}");
            res.AddRange(e.TooltipExtra);
            return res;
        }
    }
}
