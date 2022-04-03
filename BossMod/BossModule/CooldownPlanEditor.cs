using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public class CooldownPlanEditor
    {
        private class TrackElement
        {
            public ActionID Action;
            public StateMachineTree.Node? AttachNode; // used only for determining per-branch visibility
            public float WindowStart;
            public float WindowLength;

            public TrackElement(ActionID action, StateMachineTree.Node? attachNode, float windowStart, float windowLength)
            {
                Action = action;
                AttachNode = attachNode;
                WindowStart = windowStart;
                WindowLength = windowLength;
            }
        }

        private class EditState
        {
            public TrackElement? Element;
            public bool EditingEnd;

            public EditState(TrackElement element, bool editingEnd)
            {
                Element = element;
                EditingEnd = editingEnd;
            }
        }

        private CooldownPlan _curPlan;
        private Action _onModified;
        private StateMachineTree _tree;
        private Timeline _timeline = new();
        private int _selectedBranch = 0;
        private string _modifiedName;
        private Dictionary<ActionID, List<TrackElement>> _modifiedTracks = new(); // modified version
        private List<ActionID> _visibleTracks = new();
        private bool _modified = false;
        private EditState? _edit = null;

        private float _nodeHOffset = 10;
        private float _tracksHOffset = 250;
        private float _trackHBetween = 100;
        private float _trackHalfWidth = 5;
        private float _circleRadius = 5;

        public CooldownPlanEditor(CooldownPlan plan, StateMachine.State? initial, Action onModified)
        {
            _curPlan = plan;
            _onModified = onModified;
            _tree = new(initial);
            _modifiedName = _curPlan.Name;
            foreach (var aid in CooldownPlan.SupportedClasses[plan.Class].Abilities.Keys)
            {
                var mt = _modifiedTracks[aid] = new();
                foreach (var e in plan.PlanAbilities[aid.Raw])
                {
                    var state = _tree.Nodes.GetValueOrDefault(e.StateID);
                    if (state != null)
                        mt.Add(new(aid, state, (state.Predecessor?.Time ?? 0) + e.TimeSinceActivation, e.WindowLength));
                }
                _visibleTracks.Add(aid);
            }
        }

        public void Draw()
        {
            if (ImGui.Button("<") && _selectedBranch > 0)
                --_selectedBranch;
            ImGui.SameLine();
            ImGui.Text($"Current branch: {_selectedBranch}/{_tree.NumBranches - 1}");
            ImGui.SameLine();
            if (ImGui.Button(">") && _selectedBranch < _tree.NumBranches - 1)
                ++_selectedBranch;
            ImGui.SameLine();
            if (ImGui.Button(_modified ? "Save" : "No changes") && _modified)
                Save();
            ImGui.SameLine();
            if (ImGui.InputText("Name", ref _modifiedName, 255))
                _modified = true;

            for (int i = 0; i < _visibleTracks.Count; ++i)
            {
                var name = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(_visibleTracks[i].ID)?.Name.ToString() ?? "(unknown)";
                _timeline.DrawHeader(TrackCenterX(i), name, 0xffffffff);
            }

            List<StateMachineTree.Node> hoverNodes = new();
            List<TrackElement> hoverTracks = new();

            _timeline.Begin(_tracksHOffset + _trackHBetween * _visibleTracks.Count, 5, _tree.MaxTime);
            foreach (var n in _tree.BranchNodes(_selectedBranch))
                DrawNode(n, hoverNodes);
            for (int i = 0; i < _visibleTracks.Count; ++i)
                DrawTrack(i, hoverTracks);
            _timeline.End();

            if (hoverNodes.Count > 0 || hoverTracks.Count > 0)
            {
                ImGui.BeginTooltip();
                bool first = true;
                foreach (var n in hoverNodes)
                {
                    if (!first)
                        ImGui.Separator();
                    first = false;
                    ImGui.Text($"State: {n.State.ID:X} '{n.State.Name}'");
                    ImGui.Text($"Comment: {n.State.Comment}");
                    ImGui.Text($"Time: {n.Time:f1} ({n.State.Duration:f1} from prev)");
                    ImGui.Text($"Flags: {n.State.EndHint}");
                }
                foreach (var t in hoverTracks)
                {
                    if (!first)
                        ImGui.Separator();
                    first = false;
                    ImGui.Text($"Action: {t.Action}");
                    ImGui.Text($"Press at: {t.WindowStart:f1}s");
                    ImGui.Text($"Window: {t.WindowLength:f1}s");
                    if (t != _edit?.Element)
                        ImGui.Text($"Attached: {(t.AttachNode?.Time ?? 0) - t.WindowStart:f1}s before {t.AttachNode?.State.ID:X} '{t.AttachNode?.State.Name}' ({t.AttachNode?.State.Comment})");
                }
                ImGui.EndTooltip();
            }
        }

        private Vector2 NodeScreenPos(StateMachineTree.Node? node)
        {
            return _timeline.ToScreenCoords(_nodeHOffset, node?.Time ?? 0);
        }

        private float TrackCenterX(int trackIndex)
        {
            return _tracksHOffset + trackIndex * _trackHBetween + _trackHBetween / 2;
        }

        private bool PointInRect(Vector2 point, Vector2 min, Vector2 max)
        {
            return point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y;
        }

        private void DrawNode(StateMachineTree.Node node, List<StateMachineTree.Node> hoverNodes)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var nodeScreenPos = NodeScreenPos(node);
            var predScreenPos = NodeScreenPos(node.Predecessor);
            var connection = nodeScreenPos - predScreenPos;

            // draw connection from predecessor
            var connLen = connection.Length();
            var lenOffset = _circleRadius + 1;
            if (connLen > 2 * lenOffset)
            {
                var connDir = connection / connLen;
                var connScreenBeg = predScreenPos + lenOffset * connDir;
                var connScreenEnd = nodeScreenPos - lenOffset * connDir;
                drawlist.AddLine(connScreenBeg, connScreenEnd, node.InGroup ? 0xffffffff : 0xff404040);

                var connNormal = new Vector2(connDir.Y, -connDir.X);
                if (node.BossIsCasting)
                    drawlist.AddLine(connScreenBeg + 3 * connNormal, connScreenEnd + 3 * connNormal, 0xff00ff00);
                if (node.IsDowntime)
                    drawlist.AddLine(connScreenBeg + 6 * connNormal, connScreenEnd + 6 * connNormal, 0xffff0000);
                if (node.IsPositioning)
                    drawlist.AddLine(connScreenBeg - 3 * connNormal, connScreenEnd - 3 * connNormal, 0xff0000ff);
            }

            // draw node itself
            bool showNode = true;
            //showNode &= DrawUnnamedNodes || node.State.Name.Length > 0;
            //showNode &= !DrawTankbusterNodesOnly || node.State.EndHint.HasFlag(StateMachine.StateHint.Tankbuster);
            //showNode &= !DrawRaidwideNodesOnly || node.State.EndHint.HasFlag(StateMachine.StateHint.Raidwide);
            if (showNode)
            {
                var nodeColor = node.State.EndHint.HasFlag(StateMachine.StateHint.Raidwide)
                    ? (node.State.EndHint.HasFlag(StateMachine.StateHint.Tankbuster) ? 0xffff00ff : 0xffff0000)
                    : (node.State.EndHint.HasFlag(StateMachine.StateHint.Tankbuster) ? 0xff0000ff : 0xffffffff);
                drawlist.AddCircleFilled(nodeScreenPos, _circleRadius, nodeColor);
                drawlist.AddText(nodeScreenPos + new Vector2(7, -10), 0xffffffff, $"{node.State.ID:X} '{node.State.Name}'");

                if (ImGui.IsMouseHoveringRect(nodeScreenPos - new Vector2(_circleRadius), nodeScreenPos + new Vector2(_circleRadius)))
                {
                    hoverNodes.Add(node);
                }
            }
        }

        private void DrawTrack(int trackIndex, List<TrackElement> hoverTracks)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var action = _visibleTracks[trackIndex];
            var info = CooldownPlan.SupportedClasses[_curPlan.Class].Abilities[action];
            var entries = _modifiedTracks[action];

            var xCenter = TrackCenterX(trackIndex);
            var trackMin = _timeline.ScreenClientTL + new Vector2(xCenter - _trackHalfWidth, 0);
            var trackMax = _timeline.ScreenClientTL + new Vector2(xCenter + _trackHalfWidth, _timeline.Height);
            drawlist.AddRectFilled(trackMin, trackMax, 0x40404040);

            var mousePos = ImGui.GetMousePos();
            var lclickPos = ImGui.GetIO().MouseClickedPos[0];
            var rclickPos = ImGui.GetIO().MouseClickedPos[1];
            var dragInProgress = ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left) && PointInRect(lclickPos, trackMin, trackMax);
            var deleteEntry = ImGui.IsItemClicked(ImGuiMouseButton.Right) && PointInRect(rclickPos, trackMin, trackMax);
            bool trackHovered = PointInRect(mousePos, trackMin, trackMax);
            bool selectEntryToEdit = dragInProgress && _edit == null;
            TrackElement? toDelete = null;

            foreach (var e in entries.Where(e => _tree.BranchNodes(_selectedBranch).Contains(e.AttachNode)))
            {
                var yWindowStart = _timeline.ToScreenCoords(0, e.WindowStart).Y;
                var yWindowEnd = _timeline.ToScreenCoords(0, e.WindowStart + e.WindowLength).Y;
                var yEffectEnd = _timeline.ToScreenCoords(0, e.WindowStart + info.Duration).Y;
                var yCooldownEnd = _timeline.ToScreenCoords(0, e.WindowStart + e.WindowLength + info.Cooldown).Y;

                drawlist.AddRectFilled(new(trackMin.X, yWindowStart), new(trackMax.X, yWindowEnd), 0x800000ff);
                if (yEffectEnd > yWindowEnd)
                    drawlist.AddRectFilled(new(trackMin.X, yWindowEnd), new(trackMax.X, yEffectEnd), 0x8000ffff);
                drawlist.AddRectFilled(new(trackMin.X, MathF.Max(yEffectEnd, yWindowEnd)), new(trackMax.X, yCooldownEnd), 0x8000ff00);

                if (selectEntryToEdit && lclickPos.Y >= (yWindowStart - 2) && lclickPos.Y <= (yCooldownEnd + 2))
                    _edit = new(e, MathF.Abs(lclickPos.Y - yWindowEnd) < 5);
                else if (deleteEntry && rclickPos.Y >= (yWindowStart - 2) && rclickPos.Y <= (yCooldownEnd + 2))
                    toDelete = e;
                else if (trackHovered && mousePos.Y >= (yWindowStart - 2) && mousePos.Y <= (yCooldownEnd + 2))
                {
                    hoverTracks.Add(e);
                    if (!dragInProgress && _edit == null)
                        HighlightElement(xCenter, e);
                }
            }

            if (selectEntryToEdit && _edit == null)
            {
                // create new entry
                var t = _timeline.TimeFromScreen(lclickPos);
                var newElem = new TrackElement(action, _tree.TimeToBranchNode(_selectedBranch, t), t, 0);
                entries.Add(newElem);
                _edit = new(newElem, true);
            }

            if (_edit?.Element?.Action == action)
            {
                if (dragInProgress)
                {
                    HighlightElement(xCenter, _edit.Element);

                    var dt = ImGui.GetIO().MouseDelta.Y / _timeline.PixelsPerSecond;
                    if (_edit.EditingEnd)
                        _edit.Element.WindowLength += dt;
                    else
                        _edit.Element.WindowStart += dt;
                }
                else
                {
                    // finish edit
                    _edit.Element.WindowStart = Math.Clamp(_edit.Element.WindowStart, 0, _tree.MaxTime);
                    _edit.Element.AttachNode = _tree.TimeToBranchNode(_selectedBranch, _edit.Element.WindowStart);
                    _edit.Element.WindowLength = Math.Clamp(_edit.Element.WindowLength, 0, _tree.MaxTime - _edit.Element.WindowStart);
                    _edit = null;
                    _modified = true;
                }
            }

            if (toDelete != null)
            {
                entries.Remove(toDelete);
                _modified = true;
            }
        }

        private void HighlightElement(float xCenter, TrackElement elem)
        {
            var end = elem.WindowStart + elem.WindowLength;
            var dl = ImGui.GetWindowDrawList();
            dl.AddLine(_timeline.ToScreenCoords(xCenter, elem.WindowStart), _timeline.ToScreenCoords(0, elem.WindowStart), 0xffffffff);
            dl.AddLine(_timeline.ToScreenCoords(xCenter, end), _timeline.ToScreenCoords(0, end), 0xffffffff);
        }

        private void Save()
        {
            _curPlan.Name = _modifiedName;
            _curPlan.PlanAbilities = new();
            foreach (var (k, entries) in _modifiedTracks)
            {
                var list = _curPlan.PlanAbilities[k.Raw] = new();
                foreach (var e in entries.Where(e => e.AttachNode != null))
                {
                    var pred = e.AttachNode?.Predecessor?.Time ?? 0;
                    list.Add(new(e.AttachNode!.State.ID, e.WindowStart - pred, e.WindowLength));
                }
            }
            _onModified();
            _modified = false;
        }
    }
}
