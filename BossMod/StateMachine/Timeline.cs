using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // visualization of a state machine
    public class Timeline
    {
        class Node
        {
            public float Time;
            public int BranchID;
            public StateMachine.State State;
            public List<Node> Successors = new();

            public Node(float t, int branchID, StateMachine.State state)
            {
                Time = t;
                BranchID = branchID;
                State = state;
            }
        }

        public float PixelsPerSecond = 10;
        public float PixelsPerBranch = 250;
        public float TickFrequency = 5;
        public bool DrawUnnamedNodes = true;
        public bool DrawTankbusterNodesOnly = false;
        public bool DrawRaidwideNodesOnly = false;
        private float _fullDuration;
        private Node _startNode;
        private int _numBranches;

        private float _vMargin = 10;
        private float _timeAxisWidth = 35;
        private float _circleRadius = 5;

        public Timeline(StateMachine.State initial)
        {
            // build layout
            (_startNode, _fullDuration, _numBranches) = LayoutNodeAndSuccessors(0, 0, initial);
        }

        public void Draw(StateMachine.State? activeState, float timeSinceActivation)
        {
            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.SliderFloat("Pixels per second", ref PixelsPerSecond, 1, 50, "%.0f", ImGuiSliderFlags.Logarithmic);
                ImGui.SliderFloat("Tick frequency", ref TickFrequency, 1, 30);
                ImGui.Checkbox("Draw unnamed nodes", ref DrawUnnamedNodes);
                ImGui.Checkbox("Draw tankbuster nodes only", ref DrawTankbusterNodesOnly);
                ImGui.Checkbox("Draw raidwide nodes only", ref DrawRaidwideNodesOnly);
            }

            var cursor = ImGui.GetCursorScreenPos();
            float h = _fullDuration * PixelsPerSecond + 2 * _vMargin;
            float w = _numBranches * PixelsPerBranch + _timeAxisWidth;
            ImGui.Dummy(new(w, h));

            var timeAxisStart = cursor + new Vector2(_timeAxisWidth, _vMargin);
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

            List<Node> hoverNodes = new();
            DrawNodeAndSuccessors(_startNode, cursor, timeAxisStart + new Vector2(10, 0), hoverNodes, true, false, false, false, activeState, timeSinceActivation);
            if (hoverNodes.Count > 0)
            {
                ImGui.BeginTooltip();
                bool first = true;
                foreach (var n in hoverNodes)
                {
                    if (!first)
                        ImGui.Separator();
                    first = false;
                    ImGui.Text($"State: '{n.State.Name}'");
                    ImGui.Text($"Time: {n.Time:f1} ({n.State.Duration:f1} from prev)");
                    ImGui.Text($"Flags: {n.State.EndHint}");
                }
                ImGui.EndTooltip();
            }

            var margin = ImGui.GetCursorPosX() + _timeAxisWidth + 20;
            for (int i = 0; i < _numBranches; ++i)
            {
                ImGui.SetCursorPosX(margin + i * PixelsPerBranch);
                ImGui.Button($"Tank CD");
                ImGui.SameLine();
                ImGui.Button($"Raid CD");
                ImGui.SameLine();
            }
            ImGui.NewLine();
        }

        private (Node, float, int) LayoutNodeAndSuccessors(float t, int branchID, StateMachine.State state)
        {
            var node = new Node(t + state.Duration, branchID, state);
            float succDuration = 0;
            if (state.PotentialSuccessors != null)
            {
                foreach (var s in state.PotentialSuccessors)
                {
                    (var succ, var dur, var nextBranch) = LayoutNodeAndSuccessors(t + state.Duration, branchID, s);
                    node.Successors.Add(succ);
                    succDuration = Math.Max(succDuration, dur);
                    branchID = nextBranch;
                }
            }
            else if (state.Next != null)
            {
                (var succ, var dur, var nextBranch) = LayoutNodeAndSuccessors(t + state.Duration, branchID, state.Next);
                node.Successors.Add(succ);
                succDuration = dur;
                branchID = nextBranch;
            }
            else
            {
                branchID++;
            }
            return (node, state.Duration + succDuration, branchID);
        }

        private void DrawNodeAndSuccessors(Node node, Vector2 screenTL, Vector2 predPos, List<Node> hoverNodes, bool inGroup, bool bossIsCasting, bool isDowntime, bool isPositioning, StateMachine.State? activeState, float timeSinceActivation)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var nodePos = screenTL + new Vector2(_timeAxisWidth + node.BranchID * PixelsPerBranch + 10, _vMargin + node.Time * PixelsPerSecond);
            var connection = nodePos - predPos;

            Vector2? currentTimeScreenPos = null;
            if (node.State == activeState)
            {
                currentTimeScreenPos = predPos + connection * Math.Clamp(timeSinceActivation / node.State.Duration, 0, 1);
            }

            // draw connection from predecessor
            var connLen = connection.Length();
            var lenOffset = _circleRadius + 1;
            if (connLen > 2 * lenOffset)
            {
                var connDir = connection / connLen;
                var connStart = predPos + lenOffset * connDir;
                var connEnd = nodePos - lenOffset * connDir;
                drawlist.AddLine(connStart, connEnd, inGroup ? 0xffffffff : 0xff404040);

                var connNormal = new Vector2(connDir.Y, -connDir.X);
                if (bossIsCasting)
                    drawlist.AddLine(connStart + 3 * connNormal, connEnd + 3 * connNormal, 0xff00ff00);
                if (isDowntime)
                    drawlist.AddLine(connStart + 6 * connNormal, connEnd + 6 * connNormal, 0xffff0000);
                if (isPositioning)
                    drawlist.AddLine(connStart - 3 * connNormal, connEnd - 3 * connNormal, 0xff0000ff);

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
                drawlist.AddCircleFilled(nodePos, _circleRadius, nodeColor);
                drawlist.AddText(nodePos + new Vector2(7, -10), 0xffffffff, node.State.Name);

                if (node.State == activeState)
                {
                    drawlist.AddCircle(nodePos, _circleRadius + 3, nodeColor);
                }

                if (ImGui.IsMouseHoveringRect(nodePos - new Vector2(_circleRadius), nodePos + new Vector2(_circleRadius)))
                {
                    hoverNodes.Add(node);
                }
            }

            if (currentTimeScreenPos != null)
            {
                // draw timeline mark
                var markPos = new Vector2(screenTL.X + _timeAxisWidth, currentTimeScreenPos.Value.Y);
                drawlist.AddTriangleFilled(markPos, markPos - new Vector2(4, 2), markPos - new Vector2(4, -2), 0xffffffff);
            }

            inGroup = node.State.EndHint.HasFlag(StateMachine.StateHint.GroupWithNext);
            bossIsCasting = (bossIsCasting || node.State.EndHint.HasFlag(StateMachine.StateHint.BossCastStart)) && !node.State.EndHint.HasFlag(StateMachine.StateHint.BossCastEnd);
            isDowntime = (isDowntime || node.State.EndHint.HasFlag(StateMachine.StateHint.DowntimeStart)) && !node.State.EndHint.HasFlag(StateMachine.StateHint.DowntimeEnd);
            isPositioning = (isPositioning || node.State.EndHint.HasFlag(StateMachine.StateHint.PositioningStart)) && !node.State.EndHint.HasFlag(StateMachine.StateHint.PositioningEnd);
            foreach (var succ in node.Successors)
            {
                DrawNodeAndSuccessors(succ, screenTL, nodePos, hoverNodes, inGroup, bossIsCasting, isDowntime, isPositioning, activeState, timeSinceActivation);
            }
        }
    }
}
