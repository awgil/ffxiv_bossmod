using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    public abstract class StateMachineColumn : Timeline.Column
    {
        public StateMachineTree Tree;
        public float PixelsPerBranch = 250;
        public bool DrawUnnamedNodes = true;
        public bool DrawTankbusterNodesOnly = false;
        public bool DrawRaidwideNodesOnly = false;
        protected bool SingleBranchMode = false;

        private float _nodeHOffset = 10;
        private float _nodeRadius = 5;

        public StateMachineColumn(Timeline timeline, StateMachineTree tree)
            : base(timeline)
        {
            Tree = tree;
        }

        public override void Update()
        {
            Width = (SingleBranchMode ? 1 : Tree.NumBranches) * PixelsPerBranch;
        }

        protected void DrawNode(StateMachineTree.Node node, float? progress = null)
        {
            var drawlist = ImGui.GetWindowDrawList();
            var nodeScreenPos = NodeScreenPos(node);
            var predScreenPos = NodeScreenPos(node.Predecessor);
            var connection = nodeScreenPos - predScreenPos;

            // draw connection from predecessor
            var connLen = connection.Length();
            var lenOffset = _nodeRadius + 1;
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

                if (progress != null)
                {
                    var currentTimeScreenPos = predScreenPos + connection * Math.Clamp(progress.Value / node.State.Duration, 0, 1);
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
                drawlist.AddCircleFilled(nodeScreenPos, _nodeRadius, nodeColor);
                drawlist.AddText(nodeScreenPos + new Vector2(7, -10), 0xffffffff, $"{node.State.ID:X} '{node.State.Name}'");

                if (progress != null)
                {
                    drawlist.AddCircle(nodeScreenPos, _nodeRadius + 3, nodeColor);
                }

                if (ImGui.IsMouseHoveringRect(nodeScreenPos - new Vector2(_nodeRadius), nodeScreenPos + new Vector2(_nodeRadius)))
                {
                    Timeline.AddTooltip(NodeTooltip(node));

                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        OnNodeActivated(node);
                    }
                }
            }
        }

        protected virtual void OnNodeActivated(StateMachineTree.Node node) { }

        private Vector2 NodeScreenPos(StateMachineTree.Node? node)
        {
            var (branch, time) = node != null ? (SingleBranchMode ? 0 : node.BranchID, node.Time) : (0, 0);
            return Timeline.ColumnCoordsToScreenCoords(_nodeHOffset + branch * PixelsPerBranch, time);
        }

        private List<string> NodeTooltip(StateMachineTree.Node n)
        {
            List<string> res = new();
            res.Add($"State: {n.State.ID:X} '{n.State.Name}'");
            res.Add($"Comment: {n.State.Comment}");
            res.Add($"Time: {n.Time:f1} ({n.State.Duration:f1} from prev)");
            res.Add($"Flags: {n.State.EndHint}");
            return res;
        }
    }
}
