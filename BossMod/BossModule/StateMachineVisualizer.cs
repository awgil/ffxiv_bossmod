using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    public class StateMachineVisualizer
    {
        public float PixelsPerBranch = 250;
        public bool DrawUnnamedNodes = true;
        public bool DrawTankbusterNodesOnly = false;
        public bool DrawRaidwideNodesOnly = false;

        private StateMachineTree _tree;
        private Timeline _timeline = new();

        private float _nodeHOffset = 10;
        private float _circleRadius = 5;

        public StateMachineVisualizer(StateMachine.State? initial)
        {
            _tree = new(initial);
        }

        public void Draw(StateMachine? sm)
        {
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
        }

        private void DrawTimeline(StateMachine? sm)
        {
            float? currentTime = null;
            if (sm?.ActiveState != null)
            {
                var dt = MathF.Max(0, sm.ActiveState.Duration - sm.TimeSinceTransition);
                var activeNode = _tree.Nodes[sm.ActiveState.ID];
                currentTime = activeNode.Time - dt;
            }

            List<StateMachineTree.Node> hoverNodes = new();

            _timeline.Begin(_tree.NumBranches * PixelsPerBranch, 5, _tree.MaxTime, currentTime);
            foreach (var n in _tree.Nodes.Values)
                DrawNode(n, hoverNodes, sm);
            _timeline.End();

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

        private Vector2 NodeScreenPos(StateMachineTree.Node? node)
        {
            var (branch, time) = node != null ? (node.BranchID, node.Time) : (0, 0);
            return _timeline.ToScreenCoords(_nodeHOffset + branch * PixelsPerBranch, time);
        }

        private void DrawNode(StateMachineTree.Node node, List<StateMachineTree.Node> hoverNodes, StateMachine? sm)
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

                if (node.State == sm?.ActiveState)
                {
                    var currentTimeScreenPos = predScreenPos + connection * Math.Clamp(sm.TimeSinceTransition / node.State.Duration, 0, 1);
                    drawlist.AddCircleFilled(currentTimeScreenPos, 3, 0xffffffff);
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
        }
    }
}
