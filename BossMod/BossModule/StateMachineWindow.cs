using ImGuiNET;

namespace BossMod;

public class StateMachineWindow : UIWindow
{
    private readonly Timeline _timeline = new();
    private readonly ColumnStateMachineTree _col;

    public StateMachineWindow(BossModule module) : base($"{module.GetType().Name} timeline", true, new(600, 600))
    {
        _col = _timeline.Columns.Add(new ColumnStateMachineTree(_timeline, new(module.StateMachine), module.StateMachine));
        _timeline.MaxTime = _col.Tree.TotalMaxTime;
    }

    public override void Draw()
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
