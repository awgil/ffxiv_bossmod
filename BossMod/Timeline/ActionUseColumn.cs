using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    public class ActionUseColumn : Timeline.Column
    {
        public class Event
        {
            public StateMachineTree.Node? AttachNode; // used only for determining per-branch visibility
            public float Timestamp; // seconds since start
            public string Name = "";
            public List<string> TooltipExtra = new();
            public uint Color;
        }

        public class Entry
        {
            public StateMachineTree.Node? AttachNode; // used only for determining per-branch visibility
            public float WindowStart;
            public float WindowLength;
            public float Duration;
            public float Cooldown;
            public string Name = "";
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

        public int SelectedBranch = 0;
        public bool Editable = false;
        public Action NotifyModified = () => { };
        public float EffectDuration = 0;
        public float Cooldown = 0;
        public List<Event> Events = new();
        public List<Entry> Entries = new();
        private StateMachineTree _tree;
        private EditState? _edit = null;

        private float _trackHalfWidth = 5;
        private float _eventRadius = 4;

        private uint _colBackground = 0x40404040;
        private uint _colCooldown = 0x80808080;
        private uint _colEffect = 0x8000ff00;
        private uint _colWindow = 0x8000ffff;

        public ActionUseColumn(Timeline timeline, StateMachineTree tree)
            : base(timeline)
        {
            _tree = tree;
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

            foreach (var e in Entries.Where(e => _tree.BranchNodes(SelectedBranch).Contains(e.AttachNode)))
            {
                var yWindowStart = Timeline.TimeToScreenCoord(e.WindowStart);
                var yWindowEnd = Timeline.TimeToScreenCoord(e.WindowStart + e.WindowLength);
                var yEffectEnd = Timeline.TimeToScreenCoord(e.WindowStart + e.Duration);
                var yCooldownEnd = Timeline.TimeToScreenCoord(e.WindowStart + e.WindowLength + e.Cooldown);

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
                var t = Timeline.ScreenCoordToTime(lclickPos.Y);
                var newUse = new Entry() { AttachNode = _tree.TimeToBranchNode(SelectedBranch, t), WindowStart = t, Duration = EffectDuration, Cooldown = Cooldown, Name = Name };
                Entries.Add(newUse);
                _edit = new(newUse, true);
            }

            if (_edit != null)
            {
                if (dragInProgress)
                {
                    HighlightEntry(_edit.Element);
                    if (_edit.EditingEnd)
                        _edit.Element.WindowLength += Timeline.ScreenDeltaToTimeDelta(ImGui.GetIO().MouseDelta.Y);
                    else
                        _edit.Element.WindowStart += Timeline.ScreenDeltaToTimeDelta(ImGui.GetIO().MouseDelta.Y);
                }
                else
                {
                    // finish edit
                    _edit.Element.WindowStart = Math.Clamp(MathF.Round(_edit.Element.WindowStart, 1), 0, _tree.MaxTime);
                    _edit.Element.AttachNode = _tree.TimeToBranchNode(SelectedBranch, _edit.Element.WindowStart);
                    _edit.Element.WindowLength = Math.Clamp(MathF.Round(_edit.Element.WindowLength, 1), 0, _tree.MaxTime - _edit.Element.WindowStart);
                    _edit = null;
                    NotifyModified!();
                }
            }

            if (toDelete != null)
            {
                Entries.Remove(toDelete);
                NotifyModified!();
            }

            foreach (var e in Events.Where(e => _tree.BranchNodes(SelectedBranch).Contains(e.AttachNode)))
            {
                var screenPos = Timeline.ColumnCoordsToScreenCoords(Width / 2, e.Timestamp);
                ImGui.GetWindowDrawList().AddCircleFilled(screenPos, _eventRadius, e.Color);
                if (ImGui.IsMouseHoveringRect(screenPos - new Vector2(_eventRadius), screenPos + new Vector2(_eventRadius)))
                {
                    Timeline.AddTooltip(EventTooltip(e));
                    Timeline.HighlightTime(e.Timestamp);
                }
            }
        }

        private static bool PointInRect(Vector2 point, Vector2 min, Vector2 max) => point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y;

        private void HighlightEntry(Entry e)
        {
            Timeline.HighlightTime(e.WindowStart);
            Timeline.HighlightTime(e.WindowStart + e.WindowLength);
            Timeline.HighlightTime(e.WindowStart + e.Duration);
        }

        private List<string> EntryTooltip(Entry e)
        {
            List<string> res = new();
            res.Add($"Action: {e.Name}");
            res.Add($"Press at: {e.WindowStart:f1}s");
            res.Add($"Window: {e.WindowLength:f1}s");
            if (e != _edit?.Element && e.AttachNode != null)
                res.Add($"Attached: {e.AttachNode.Time - e.WindowStart:f1}s before {e.AttachNode.State.ID:X} '{e.AttachNode.State.Name}' ({e.AttachNode.State.Comment})");
            return res;
        }

        private List<string> EventTooltip(Event e)
        {
            List<string> res = new();
            res.Add($"{e.Timestamp:f1}: {e.Name}");
            res.AddRange(e.TooltipExtra);
            return res;
        }
    }
}
