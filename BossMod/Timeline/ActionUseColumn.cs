using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // TODO: legacy, remove
    public class ActionUseColumn : Timeline.Column
    {
        public class Event
        {
            public StateMachineTree.Node AttachNode;
            public float Delay; // from node's predecessor time
            public string Name;
            public List<string> TooltipExtra = new();
            public uint Color;

            public Event(StateMachineTree.Node attachNode, float delay, string name, uint color)
            {
                AttachNode = attachNode;
                Delay = delay;
                Name = name;
                Color = color;
            }

            public float TimeSincePhaseStart() => (AttachNode.Predecessor?.Time ?? 0) + Delay;
            public float TimeSinceGlobalStart(StateMachineTree tree) => tree.Phases[AttachNode.PhaseID].StartTime + TimeSincePhaseStart();
        }

        public class Entry
        {
            public StateMachineTree.Node AttachNode;
            public float WindowStartDelay; // from node's predecessor time
            public float WindowLength;
            public float Duration;
            public float Cooldown;
            public string Name;

            public Entry(StateMachineTree.Node attachNode, float windowStartDelay, float windowLength, float duration, float cooldown, string name)
            {
                AttachNode = attachNode;
                WindowStartDelay = windowStartDelay;
                WindowLength = windowLength;
                Duration = duration;
                Cooldown = cooldown;
                Name = name;
            }

            public float WindowStartSincePhaseStart() => (AttachNode.Predecessor?.Time ?? 0) + WindowStartDelay;
            public float WindowStartSinceGlobalStart(StateMachineTree tree) => tree.Phases[AttachNode.PhaseID].StartTime + WindowStartSincePhaseStart();
        }

        private class EditState
        {
            public Entry Element;
            public bool EditingEnd;

            public EditState(Entry element, bool editingEnd)
            {
                Element = element;
                EditingEnd = editingEnd;
            }
        }

        public bool Editable = false;
        public Action NotifyModified = () => { };
        public float EffectDuration = 0;
        public float Cooldown = 0;
        public List<Event> Events = new();
        public List<Entry> Entries = new();
        private StateMachineTree _tree;
        private List<int> _phaseBranches;
        private EditState? _edit = null;

        private float _trackHalfWidth = 5;
        private float _eventRadius = 4;

        private uint _colBackground = 0x40404040;
        private uint _colCooldown = 0x80808080;
        private uint _colEffect = 0x8000ff00;
        private uint _colWindow = 0x8000ffff;

        public ActionUseColumn(Timeline timeline, StateMachineTree tree, List<int> phaseBranches)
            : base(timeline)
        {
            _tree = tree;
            _phaseBranches = phaseBranches;
        }

        public override void Draw()
        {
            var drawlist = ImGui.GetWindowDrawList();

            var trackMin = Timeline.ColumnCoordsToScreenCoords(Width / 2 - _trackHalfWidth, Timeline.MinVisibleTime);
            var trackMax = Timeline.ColumnCoordsToScreenCoords(Width / 2 + _trackHalfWidth, Timeline.MaxVisibleTime);
            drawlist.AddRectFilled(trackMin, trackMax, _colBackground);

            var mousePos = ImGui.GetMousePos();
            var lclickPos = ImGui.GetIO().MouseClickedPos[0];
            var rclickPos = ImGui.GetIO().MouseClickedPos[1];
            var dragInProgress = Editable && ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left) && PointInRect(lclickPos, trackMin, trackMax);
            var deleteEntry = Editable && ImGui.IsItemClicked(ImGuiMouseButton.Right) && PointInRect(rclickPos, trackMin, trackMax);
            bool trackHovered = PointInRect(mousePos, trackMin, trackMax);
            bool selectEntryToEdit = dragInProgress && _edit == null;
            Entry? toDelete = null;

            foreach (var e in Entries)
            {
                int branchID = _tree.Phases[e.AttachNode.PhaseID].StartingNode.BranchID + _phaseBranches[e.AttachNode.PhaseID];
                if (branchID < e.AttachNode.BranchID || branchID >= e.AttachNode.BranchID + e.AttachNode.NumBranches)
                    continue; // node is invisible with current branch selection

                float windowStart = e.WindowStartSinceGlobalStart(_tree);
                var yWindowStart = Timeline.TimeToScreenCoord(windowStart);
                var yWindowEnd = Timeline.TimeToScreenCoord(windowStart + e.WindowLength);
                var yEffectEnd = Timeline.TimeToScreenCoord(windowStart + e.Duration);
                var yCooldownEnd = Timeline.TimeToScreenCoord(windowStart + e.WindowLength + e.Cooldown);

                drawlist.AddRectFilled(new(trackMin.X, yWindowStart), new(trackMax.X, yWindowEnd), _colWindow);
                if (yEffectEnd > yWindowEnd)
                    drawlist.AddRectFilled(new(trackMin.X, yWindowEnd), new(trackMax.X, yEffectEnd), _colEffect);
                drawlist.AddRectFilled(new(trackMin.X, MathF.Max(yEffectEnd, yWindowEnd)), new(trackMax.X, yCooldownEnd), _colCooldown);

                if (selectEntryToEdit && lclickPos.Y >= (yWindowStart - 2) && lclickPos.Y <= (yCooldownEnd + 2))
                {
                    _edit = new(e, MathF.Abs(lclickPos.Y - yWindowEnd) < 5);
                }
                else if (deleteEntry && rclickPos.Y >= (yWindowStart - 2) && rclickPos.Y <= (yCooldownEnd + 2))
                {
                    toDelete = e;
                }
                else if (trackHovered && mousePos.Y >= (yWindowStart - 2) && mousePos.Y <= (yCooldownEnd + 2))
                {
                    Timeline.AddTooltip(EntryTooltip(e));
                    if (!dragInProgress && _edit == null)
                        HighlightEntry(e);
                }
            }

            if (selectEntryToEdit && _edit == null)
            {
                // create new entry
                var (node, delay) = AbsoluteTimeToNodeAndDelay(Timeline.ScreenCoordToTime(lclickPos.Y));
                var newUse = new Entry(node, delay, 0, EffectDuration, Cooldown, Name);
                Entries.Add(newUse);
                _edit = new(newUse, true);
            }

            if (_edit != null)
            {
                if (dragInProgress)
                {
                    HighlightEntry(_edit.Element);
                    var dt = Timeline.ScreenDeltaToTimeDelta(ImGui.GetIO().MouseDelta.Y);
                    if (_edit.EditingEnd)
                    {
                        _edit.Element.WindowLength += dt;
                    }
                    else
                    {
                        float windowStart = _edit.Element.WindowStartSinceGlobalStart(_tree);
                        windowStart += dt;
                        (_edit.Element.AttachNode, _edit.Element.WindowStartDelay) = AbsoluteTimeToNodeAndDelay(windowStart);
                    }
                }
                else
                {
                    // finish edit
                    _edit.Element.WindowStartDelay = Math.Max(MathF.Round(_edit.Element.WindowStartDelay, 1), 0);
                    _edit.Element.WindowLength = Math.Max(MathF.Round(_edit.Element.WindowLength, 1), 0);
                    _edit = null;
                    NotifyModified!();
                }
            }

            if (toDelete != null)
            {
                Entries.Remove(toDelete);
                NotifyModified!();
            }

            foreach (var e in Events)
            {
                int branchID = _tree.Phases[e.AttachNode.PhaseID].StartingNode.BranchID + _phaseBranches[e.AttachNode.PhaseID];
                if (branchID < e.AttachNode.BranchID || branchID >= e.AttachNode.BranchID + e.AttachNode.NumBranches)
                    continue; // node is invisible with current branch selection

                var t = e.TimeSinceGlobalStart(_tree);
                var screenPos = Timeline.ColumnCoordsToScreenCoords(Width / 2, t);
                ImGui.GetWindowDrawList().AddCircleFilled(screenPos, _eventRadius, e.Color);
                if (ImGui.IsMouseHoveringRect(screenPos - new Vector2(_eventRadius), screenPos + new Vector2(_eventRadius)))
                {
                    Timeline.AddTooltip(EventTooltip(e));
                    Timeline.HighlightTime(t);
                }
            }
        }

        public (StateMachineTree.Node, float) AbsoluteTimeToNodeAndDelay(float t)
        {
            int phaseIndex = _tree.FindPhaseAtTime(t);
            var phase = _tree.Phases[phaseIndex];
            t -= phase.StartTime;
            var node = phase.TimeToBranchNode(_phaseBranches[phaseIndex], t);
            return (node, t - (node.Predecessor?.Time ?? 0));
        }

        private static bool PointInRect(Vector2 point, Vector2 min, Vector2 max) => point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y;

        private void HighlightEntry(Entry e)
        {
            var windowStart = e.WindowStartSinceGlobalStart(_tree);
            Timeline.HighlightTime(windowStart);
            Timeline.HighlightTime(windowStart + e.WindowLength);
            Timeline.HighlightTime(windowStart + e.Duration);
        }

        private List<string> EntryTooltip(Entry e)
        {
            List<string> res = new();
            res.Add($"Action: {e.Name}");
            res.Add($"Press at: {e.WindowStartSinceGlobalStart(_tree):f1}s ({e.WindowStartSincePhaseStart():f1}s since phase start, {e.WindowStartDelay:f1}s after state start)");
            if (e.AttachNode.Predecessor != null)
                res.Add($"Attached: {e.WindowStartDelay:f1}s after {e.AttachNode.Predecessor.State.ID:X} '{e.AttachNode.Predecessor.State.Name}' ({e.AttachNode.Predecessor.State.Comment})");
            else
                res.Add($"Attached: {e.WindowStartDelay:f1}s after pull");
            res.Add($"Next state: {e.AttachNode.State.Duration - e.WindowStartDelay:f1}s before {e.AttachNode.State.ID:X} '{e.AttachNode.State.Name}' ({e.AttachNode.State.Comment})");
            res.Add($"Window: {e.WindowLength:f1}s");
            return res;
        }

        private List<string> EventTooltip(Event e)
        {
            List<string> res = new();
            res.Add($"{e.TimeSinceGlobalStart(_tree):f1}: {e.Name}");
            res.AddRange(e.TooltipExtra);
            return res;
        }
    }
}
