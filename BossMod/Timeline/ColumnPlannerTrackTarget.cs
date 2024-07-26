using BossMod.Autorotation;
using ImGuiNET;

namespace BossMod;

public class ColumnPlannerTrackTarget(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, ModuleRegistry.Info? moduleInfo) : ColumnPlannerTrack(timeline, tree, phaseBranches, "Target")
{
    public class OverrideElement(Entry window) : Element(window)
    {
        public StrategyValue Value = new();
    }

    public void AddElement(StateMachineTree.Node attachNode, float delay, float windowLength, StrategyValue value)
    {
        var elem = (OverrideElement)AddElement(attachNode, delay, windowLength);
        elem.Value = value;
    }

    protected override Element CreateElement(Entry window) => new OverrideElement(window);

    protected override List<string> DescribeElement(Element e)
    {
        var cast = (OverrideElement)e;
        return [
            $"Comment: {cast.Value.Comment}",
            $"Target: {UIStrategyValue.PreviewTarget(ref cast.Value, moduleInfo)}"
        ];
    }

    protected override void EditElement(Element e)
    {
        var cast = (OverrideElement)e;
        if (UIStrategyValue.DrawEditorTarget(ref cast.Value, ActionTargets.All, false, moduleInfo))
            NotifyModified();
        if (ImGui.InputText("Comment", ref cast.Value.Comment, 256))
            NotifyModified();
    }
}
