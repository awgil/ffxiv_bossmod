using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;

namespace BossMod;

public class ColumnPlannerTrackTarget(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, BossModuleRegistry.Info? moduleInfo) : ColumnPlannerTrack(timeline, tree, phaseBranches, "Target")
{
    protected override StrategyValueTrack GetDefaultValue() => new();

    protected override void RefreshElement(Element e)
    {
        e.Window.Color = Timeline.Colors.PlannerWindow[0];
    }

    protected override List<string> DescribeElement(Element e)
    {
        return [
            $"Comment: {e.Value.Comment}",
            $"Target: {UIStrategyValue.PreviewTarget((StrategyValueTrack)e.Value, moduleInfo)}"
        ];
    }

    protected override bool EditElement(Element e)
    {
        var modified = false;
        modified |= UIStrategyValue.DrawEditorTarget((StrategyValueTrack)e.Value, ActionTargets.All, moduleInfo);
        modified |= ImGui.InputText("Comment", ref e.Value.Comment, 256);
        modified |= EditElementWindow(e);
        return modified;
    }
}
