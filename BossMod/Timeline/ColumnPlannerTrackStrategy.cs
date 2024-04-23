using ImGuiNET;
using System.Reflection;

namespace BossMod;

public class ColumnPlannerTrackStrategy(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, string name, PlanDefinitions.ClassData classDef, PlanDefinitions.StrategyTrack trackDef)
    : ColumnPlannerTrack(timeline, tree, phaseBranches, name)
{
    public class OverrideElement : Element
    {
        public uint Value;
        public string Comment;

        public OverrideElement(Entry window, uint value, string comment, float cooldown) : base(window)
        {
            Value = value;
            Comment = comment;
            CooldownLength = cooldown;
        }
    }

    public PlanDefinitions.ClassData ClassDef = classDef;
    public PlanDefinitions.StrategyTrack TrackDef = trackDef;

    public void AddElement(StateMachineTree.Node attachNode, float delay, float windowLength, uint value, string comment)
    {
        var elem = (OverrideElement)AddElement(attachNode, delay, windowLength);
        elem.Comment = comment;
        SetElementValue(elem, value);
    }

    protected override Element CreateElement(Entry window)
    {
        return SetElementValue(new OverrideElement(window, 0, "", TrackDef.Cooldown), 1);
    }

    protected override List<string> DescribeElement(Element e)
    {
        var cast = (OverrideElement)e;
        List<string> res = [$"Comment: {cast.Comment}"];
        if (TrackDef.Values != null)
        {
            res.Add($"Value: {ValueString(cast.Value)}");
        }
        return res;
    }

    protected override void EditElement(Element e)
    {
        var cast = (OverrideElement)e;
        if (TrackDef.Values != null && ImGui.BeginCombo("Value", ValueString(cast.Value)))
        {
            foreach (var opt in TrackDef.Values.GetEnumValues())
            {
                var uopt = (uint)opt;
                if (uopt == 0)
                    continue;

                if (ImGui.Selectable(ValueString(uopt), cast.Value == uopt))
                {
                    SetElementValue(cast, uopt);
                    NotifyModified();
                }
            }
            ImGui.EndCombo();
        }
        if (ImGui.InputText("Comment", ref cast.Comment, 256))
        {
            NotifyModified();
        }
    }

    private string ValueString(uint value)
    {
        var name = TrackDef.Values?.GetEnumName(value);
        if (name == null)
            return value.ToString();
        return TrackDef.Values?.GetField(name)?.GetCustomAttribute<PropertyDisplayAttribute>()?.Label ?? name;
    }

    private OverrideElement SetElementValue(OverrideElement e, uint value)
    {
        e.Value = value;

        var fn = TrackDef.Values?.GetEnumName(value);
        var prop = fn != null ? TrackDef.Values?.GetField(fn)?.GetCustomAttribute<PropertyDisplayAttribute>()?.Color : null;
        if (prop != null)
            e.Window.Color = prop.Value;

        return e;
    }
}
