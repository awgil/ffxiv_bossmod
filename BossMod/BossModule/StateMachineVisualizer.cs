using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    public class StateMachineVisualizer
    {
        class Node
        {
            public float Time;
            public int BranchID;
            public StateMachine.State State;
            public Node? Predecessor;
            public List<Node> Successors = new();

            public Node(float t, int branchID, StateMachine.State state, Node? pred)
            {
                Time = t;
                BranchID = branchID;
                State = state;
                Predecessor = pred;
            }
        }

        public float PixelsPerSecond = 10;
        public float PixelsPerBranch = 250;
        public float PixelsPerCooldownTrack = 100;
        public float TickFrequency = 5;
        public bool DrawUnnamedNodes = true;
        public bool DrawTankbusterNodesOnly = false;
        public bool DrawRaidwideNodesOnly = false;
        private float _fullDuration = 0;
        private Node? _startNode = null;
        private int _numBranches = 0;

        private CooldownPlan _curPlan = new(CooldownPlan.SupportedClass.WAR); // TODO: this should be externalized...
        private Dictionary<uint, Node>? _selectedBranchNodes;
        private List<int> _selectedCooldownTracks = new();
        private (int, int, bool)? _editedCooldownTrack; // null if not editing, otherwise track+use+whether we're editing window-end

        private float _vMargin = 10;
        private float _timeAxisWidth = 35;
        private float _circleRadius = 5;

        public StateMachineVisualizer(StateMachine.State? initial)
        {
            // TODO: remove!!
            _curPlan.PlanAbilities[0].Add(new(0, 1, 5));

            // build layout
            if (initial != null)
                (_startNode, _fullDuration, _numBranches) = LayoutNodeAndSuccessors(0, 0, initial, null);
        }

        public void Draw(StateMachine? sm)
        {
            if (_selectedBranchNodes != null)
            {
                if (ImGui.Button("Back"))
                {
                    _selectedBranchNodes = null;
                    _selectedCooldownTracks.Clear();
                }
                ImGui.SameLine();
            }

            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.Checkbox("Draw unnamed nodes", ref DrawUnnamedNodes);
                ImGui.Checkbox("Draw tankbuster nodes only", ref DrawTankbusterNodesOnly);
                ImGui.Checkbox("Draw raidwide nodes only", ref DrawRaidwideNodesOnly);
            }

            if (sm != null)
            {
                if (ImGui.Button(sm.Paused ? "Resume" : "Pause"))
                    sm.Paused = !sm.Paused;
                ImGui.SameLine();
                if (ImGui.Button("Reset"))
                    sm.ActiveState = null;
            }

            DrawTimeline(sm);

            if (_selectedBranchNodes == null)
            {
                var margin = ImGui.GetCursorPosX() + _timeAxisWidth + 20;
                for (int i = 0; i < _numBranches; ++i)
                {
                    ImGui.SetCursorPosX(margin + i * PixelsPerBranch);
                    if (ImGui.Button($"Tank CD##{i}"))
                        EnterPlanningMode(i, CooldownPlan.AbilityCategory.SelfMitigation);
                    ImGui.SameLine();
                    if (ImGui.Button($"Raid CD##{i}"))
                        EnterPlanningMode(i, CooldownPlan.AbilityCategory.RaidMitigation);
                    ImGui.SameLine();
                    ImGui.Button($"Sim##{i}");
                    ImGui.SameLine();
                }
                ImGui.NewLine();
            }
        }

        private (Node, float, int) LayoutNodeAndSuccessors(float t, int branchID, StateMachine.State state, Node? pred)
        {
            var node = new Node(t + state.Duration, branchID, state, pred);
            float succDuration = 0;

            // first layout default state, if any
            if (state.Next != null)
            {
                (var succ, var dur, var nextBranch) = LayoutNodeAndSuccessors(t + state.Duration, branchID, state.Next, node);
                node.Successors.Add(succ);
                succDuration = dur;
                branchID = nextBranch;
            }

            // now layout extra successors, if any
            if (state.PotentialSuccessors != null)
            {
                foreach (var s in state.PotentialSuccessors)
                {
                    if (state.Next == s)
                        continue; // this is already processed

                    (var succ, var dur, var nextBranch) = LayoutNodeAndSuccessors(t + state.Duration, branchID, s, node);
                    node.Successors.Add(succ);
                    succDuration = Math.Max(succDuration, dur);
                    branchID = nextBranch;
                }
            }

            if (state.Next == null && state.PotentialSuccessors == null)
            {
                branchID++; // leaf
            }

            return (node, state.Duration + succDuration, branchID);
        }

        private void DrawTimeline(StateMachine? sm)
        {
            float wTimeline = (_selectedBranchNodes == null ? _numBranches : 1) * PixelsPerBranch;
            float wTracks = _selectedCooldownTracks.Count * PixelsPerCooldownTrack;

            var cursor = ImGui.GetCursorScreenPos();
            float h = _fullDuration * PixelsPerSecond + 2 * _vMargin;
            float w = _timeAxisWidth + wTimeline + wTracks;
            ImGui.InvisibleButton("canvas", new(w, h));

            cursor.Y += _vMargin;
            DrawTimeAxis(cursor);
            cursor.X += _timeAxisWidth;
            var xAxis = cursor.X;

            List<Node> hoverNodes = new();
            if (_startNode != null)
                DrawNodeAndSuccessors(_startNode, cursor, new(10, 0), hoverNodes, true, false, false, false, sm);
            cursor.X += wTimeline;

            foreach (int id in _selectedCooldownTracks)
            {
                DrawTrack(cursor, id, xAxis);
                cursor.X += PixelsPerCooldownTrack;
            }

            if (ImGui.IsItemHovered() && ImGui.GetIO().MouseWheel != 0 && ImGui.GetIO().KeyShift)
            {
                PixelsPerSecond *= MathF.Pow(1.05f, ImGui.GetIO().MouseWheel);
                TickFrequency = 5;
                while (TickFrequency < 60 && PixelsPerSecond * TickFrequency < 30)
                    TickFrequency *= 2;
                while (TickFrequency > 1 && PixelsPerSecond * TickFrequency > 55)
                    TickFrequency = MathF.Floor(TickFrequency * 0.5f);
                while (TickFrequency > 0.1f && PixelsPerSecond * TickFrequency > 55)
                    TickFrequency = MathF.Floor(TickFrequency * 5) * 0.1f;
            }

            if (hoverNodes.Count > 0)
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
                ImGui.EndTooltip();
            }
        }

        private void DrawTimeAxis(Vector2 screenTL)
        {
            var timeAxisStart = screenTL + new Vector2(_timeAxisWidth, 0);
            var drawlist = ImGui.GetWindowDrawList();
            drawlist.AddLine(timeAxisStart, timeAxisStart + new Vector2(0, _fullDuration * PixelsPerSecond), 0xffffffff);
            for (float t = 0; t <= _fullDuration; t += TickFrequency)
            {
                string tickText = $"{t:f1}";
                var tickTextSize = ImGui.CalcTextSize(tickText);

                var p = timeAxisStart + new Vector2(0, t * PixelsPerSecond);
                drawlist.AddLine(p, p - new Vector2(3, 0), 0xffffffff);
                drawlist.AddText(p - new Vector2(tickTextSize.X + 5, tickTextSize.Y / 2), 0xffffffff, tickText);
            }
        }

        private void DrawNodeAndSuccessors(Node node, Vector2 screenTL, Vector2 predPos, List<Node> hoverNodes, bool inGroup, bool bossIsCasting, bool isDowntime, bool isPositioning, StateMachine? sm)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var nodePos = new Vector2(10 + (_selectedBranchNodes == null ? node.BranchID : 0) * PixelsPerBranch, node.Time * PixelsPerSecond);
            var connection = nodePos - predPos;
            var nodeScreenPos = screenTL + nodePos;
            var predScreenPos = screenTL + predPos;

            Vector2? currentTimeScreenPos = null;
            if (node.State == sm?.ActiveState)
            {
                currentTimeScreenPos = predScreenPos + connection * Math.Clamp(sm.TimeSinceTransition / node.State.Duration, 0, 1);
            }

            // draw connection from predecessor
            var connLen = connection.Length();
            var lenOffset = _circleRadius + 1;
            if (connLen > 2 * lenOffset)
            {
                var connDir = connection / connLen;
                var connScreenBeg = predScreenPos + lenOffset * connDir;
                var connScreenEnd = nodeScreenPos - lenOffset * connDir;
                drawlist.AddLine(connScreenBeg, connScreenEnd, inGroup ? 0xffffffff : 0xff404040);

                var connNormal = new Vector2(connDir.Y, -connDir.X);
                if (bossIsCasting)
                    drawlist.AddLine(connScreenBeg + 3 * connNormal, connScreenEnd + 3 * connNormal, 0xff00ff00);
                if (isDowntime)
                    drawlist.AddLine(connScreenBeg + 6 * connNormal, connScreenEnd + 6 * connNormal, 0xffff0000);
                if (isPositioning)
                    drawlist.AddLine(connScreenBeg - 3 * connNormal, connScreenEnd - 3 * connNormal, 0xff0000ff);

                if (currentTimeScreenPos != null)
                {
                    drawlist.AddCircleFilled(currentTimeScreenPos.Value, 3, 0xffffffff);
                }
            }

            // draw node itself
            bool showNode = true;
            showNode &= DrawUnnamedNodes || node.State.Name.Length > 0;
            showNode &= !DrawTankbusterNodesOnly || node.State.EndHint.HasFlag(StateMachine.StateHint.Tankbuster);
            showNode &= !DrawRaidwideNodesOnly || node.State.EndHint.HasFlag(StateMachine.StateHint.Raidwide);
            if (showNode)
            {
                var nodeColor = node.State.EndHint.HasFlag(StateMachine.StateHint.Raidwide)
                    ? (node.State.EndHint.HasFlag(StateMachine.StateHint.Tankbuster) ? 0xffff00ff : 0xffff0000)
                    : (node.State.EndHint.HasFlag(StateMachine.StateHint.Tankbuster) ? 0xff0000ff : 0xffffffff);
                drawlist.AddCircleFilled(nodeScreenPos, _circleRadius, nodeColor);
                drawlist.AddText(nodeScreenPos + new Vector2(7, -10), 0xffffffff, $"{node.State.ID:X} '{node.State.Name}'");

                if (node.State == sm?.ActiveState)
                {
                    drawlist.AddCircle(nodeScreenPos, _circleRadius + 3, nodeColor);
                }

                if (ImGui.IsMouseHoveringRect(nodeScreenPos - new Vector2(_circleRadius), nodeScreenPos + new Vector2(_circleRadius)))
                {
                    hoverNodes.Add(node);

                    if (sm != null && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        sm.ActiveState = node.State;
                    }
                }
            }

            if (currentTimeScreenPos != null)
            {
                // draw timeline mark
                var markPos = new Vector2(screenTL.X, currentTimeScreenPos.Value.Y);
                drawlist.AddTriangleFilled(markPos, markPos - new Vector2(4, 2), markPos - new Vector2(4, -2), 0xffffffff);
            }

            inGroup = node.State.EndHint.HasFlag(StateMachine.StateHint.GroupWithNext);
            bossIsCasting = (bossIsCasting || node.State.EndHint.HasFlag(StateMachine.StateHint.BossCastStart)) && !node.State.EndHint.HasFlag(StateMachine.StateHint.BossCastEnd);
            isDowntime = (isDowntime || node.State.EndHint.HasFlag(StateMachine.StateHint.DowntimeStart)) && !node.State.EndHint.HasFlag(StateMachine.StateHint.DowntimeEnd);
            isPositioning = (isPositioning || node.State.EndHint.HasFlag(StateMachine.StateHint.PositioningStart)) && !node.State.EndHint.HasFlag(StateMachine.StateHint.PositioningEnd);
            foreach (var succ in node.Successors.Where(succ => _selectedBranchNodes == null || _selectedBranchNodes.ContainsKey(succ.State.ID)))
            {
                DrawNodeAndSuccessors(succ, screenTL, nodePos, hoverNodes, inGroup, bossIsCasting, isDowntime, isPositioning, sm);
            }
        }

        private void DrawTrack(Vector2 screenTL, int trackID, float axisScreenX)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var info = CooldownPlan.SupportedClasses[(int)_curPlan.PlanClass].Abilities[trackID];
            var entries = _curPlan.PlanAbilities[trackID];

            var xCenter = screenTL.X + PixelsPerCooldownTrack / 2;

            var name = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(info.Action.ID)?.Name.ToString() ?? "(unknown)";
            var nameSize = ImGui.CalcTextSize(name);
            drawlist.AddText(new(xCenter - nameSize.X / 2, screenTL.Y - _vMargin), 0xffffffff, name);

            var xMin = xCenter - 5;
            var xMax = xCenter + 5;
            drawlist.AddRectFilled(new(xMin, screenTL.Y), new(xMax, screenTL.Y + _fullDuration * PixelsPerBranch), 0x40404040);

            var lClickPos = ImGui.GetIO().MouseClickedPos[0];
            var rClickPos = ImGui.GetIO().MouseClickedPos[1];
            bool dragInProgress = ImGui.IsMouseDragging(ImGuiMouseButton.Left);
            bool selectEntryToEdit = dragInProgress && _editedCooldownTrack == null && ImGui.IsItemHovered() && lClickPos.X >= xMin && lClickPos.X <= xMax;
            bool deleteEntry = ImGui.IsItemClicked(ImGuiMouseButton.Right) && rClickPos.X >= xMin && rClickPos.X <= xMax;
            (int, bool)? newEntryToEdit = null;
            int? entryToDelete = null;

            for (int i = 0; i < entries.Count; ++i)
            {
                var e = entries[i];
                var state = _selectedBranchNodes?.GetValueOrDefault(e.StateID);
                if (state == null)
                    continue; // this use is attached to inactive branch

                var yWindowStart = screenTL.Y + ((state.Predecessor?.Time ?? 0) + e.TimeSinceActivation) * PixelsPerSecond;
                var yWindowEnd = yWindowStart + e.WindowLength * PixelsPerSecond;
                var yEffectEnd = yWindowStart + info.Duration * PixelsPerSecond;
                var yCooldownEnd = yWindowEnd + info.Cooldown * PixelsPerSecond;

                drawlist.AddRectFilled(new(xMin, yWindowStart), new(xMax, yWindowEnd), 0x800000ff);
                if (yEffectEnd > yWindowEnd)
                    drawlist.AddRectFilled(new(xMin, yWindowEnd), new(xMax, yEffectEnd), 0x8000ffff);
                drawlist.AddRectFilled(new(xMin, MathF.Max(yEffectEnd, yWindowEnd)), new(xMax, yCooldownEnd), 0x8000ff00);

                if (selectEntryToEdit && lClickPos.Y >= (yWindowStart - 2) && lClickPos.Y <= (yCooldownEnd + 2))
                    newEntryToEdit = (i, MathF.Abs(lClickPos.Y - yWindowEnd) < 5);
                else if (deleteEntry && rClickPos.Y >= (yWindowStart - 2) && rClickPos.Y <= (yCooldownEnd + 2))
                    entryToDelete = i;
            }

            if (selectEntryToEdit)
            {
                if (newEntryToEdit == null)
                {
                    // create new entry
                    var t = (lClickPos.Y - screenTL.Y) / PixelsPerSecond;
                    var nextState = _selectedBranchNodes?.Values.Where(n => n.Time >= t).MinBy(n => n.Time);
                    entries.Add(new(nextState?.State.ID ?? 0, t - (nextState?.Predecessor?.Time ?? 0), 0));
                    _editedCooldownTrack = (trackID, entries.Count - 1, true);
                }
                else
                {
                    _editedCooldownTrack = (trackID, newEntryToEdit.Value.Item1, newEntryToEdit.Value.Item2);
                }
            }

            if (_editedCooldownTrack != null && _editedCooldownTrack.Value.Item1 == trackID)
            {
                var e = entries[_editedCooldownTrack.Value.Item2];
                if (dragInProgress)
                {
                    var state = _selectedBranchNodes?.GetValueOrDefault(e.StateID);
                    var t = (state?.Predecessor?.Time ?? 0) + e.TimeSinceActivation;
                    if (_editedCooldownTrack.Value.Item3)
                        t += MathF.Max(0, e.WindowLength);
                    var y = screenTL.Y + t * PixelsPerSecond;
                    drawlist.AddLine(new(axisScreenX, y), new(xMin, y), 0xffffffff);

                    var dt = ImGui.GetIO().MouseDelta.Y / PixelsPerSecond;
                    if (_editedCooldownTrack.Value.Item3)
                        e.WindowLength += dt;
                    else
                        e.TimeSinceActivation += dt;
                }
                else
                {
                    // finish edit
                    if (_editedCooldownTrack.Value.Item3)
                    {
                        e.WindowLength = MathF.Max(0, e.WindowLength);
                    }
                    else
                    {
                        var state = _selectedBranchNodes?.GetValueOrDefault(e.StateID);
                        var t = (state?.Predecessor?.Time ?? 0) + e.TimeSinceActivation;
                        t = Math.Clamp(t, 0, _fullDuration);
                        state = _selectedBranchNodes?.Values.Where(n => n.Time >= t).MinBy(n => n.Time);
                        e.StateID = state?.State.ID ?? 0;
                        e.TimeSinceActivation = t - (state?.Predecessor?.Time ?? 0);
                    }
                    _editedCooldownTrack = null;
                }
            }

            if (entryToDelete != null)
            {
                if (_editedCooldownTrack != null && _editedCooldownTrack.Value.Item1 == trackID && _editedCooldownTrack.Value.Item2 == entryToDelete.Value)
                    _editedCooldownTrack = null;
                entries.RemoveAt(entryToDelete.Value);
            }
        }

        private void EnterPlanningMode(int branch, CooldownPlan.AbilityCategory category)
        {
            _selectedBranchNodes = new();
            var n = _startNode;
            if (n != null)
            {
                _selectedBranchNodes[n.State.ID] = n;
                while (n.Successors.Count > 0)
                {
                    int index = n.Successors.FindIndex(n => n.BranchID > branch);
                    if (index == -1)
                        index = n.Successors.Count;
                    --index;
                    n = n.Successors[index];
                    _selectedBranchNodes[n.State.ID] = n;
                }
            }

            var supportedAbilities = CooldownPlan.SupportedClasses[(int)_curPlan.PlanClass].Abilities;
            for (int i = 0; i < supportedAbilities.Count; ++i)
                if (supportedAbilities[i].Category == category)
                    _selectedCooldownTracks.Add(i);
        }
    }
}
