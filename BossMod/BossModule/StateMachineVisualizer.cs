using ImGuiNET;
using System;

namespace BossMod
{
    public class StateMachineVisualizer
    {
        private Timeline _timeline = new();
        private StateMachineTreeColumn _col;

        public StateMachineVisualizer(StateMachine.State? initial, StateMachine? sm)
        {
            _col = _timeline.AddColumn(new StateMachineTreeColumn(_timeline, new(initial), sm));
            _timeline.MaxTime = _col.Tree.MaxTime;
        }

        public void Draw()
        {
            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.Checkbox("Draw unnamed nodes", ref _col.DrawUnnamedNodes);
                ImGui.Checkbox("Draw tankbuster nodes only", ref _col.DrawTankbusterNodesOnly);
                ImGui.Checkbox("Draw raidwide nodes only", ref _col.DrawRaidwideNodesOnly);
            }

            _timeline.CurrentTime = null;
            if (_col.ControlledSM?.ActiveState != null)
            {
                var dt = _col.ControlledSM.ActiveState.Duration - _col.ControlledSM.TimeSinceTransitionClamped;
                var activeNode = _col.Tree.Nodes[_col.ControlledSM.ActiveState.ID];
                _timeline.CurrentTime = activeNode.Time - dt;
            }

            _timeline.Draw();
        }
    }
}
