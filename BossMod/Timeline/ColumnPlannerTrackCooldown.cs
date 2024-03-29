using ImGuiNET;
using System.Reflection;

namespace BossMod;

public class ColumnPlannerTrackCooldown : ColumnPlannerTrack
{
    public class ActionElement : Element
    {
        public ActionID Action;
        public bool LowPriority;
        public PlanTarget.ISelector Target;
        public string Comment;

        public ActionElement(Entry window, bool lowPriority, PlanTarget.ISelector target, string comment) : base(window)
        {
            LowPriority = lowPriority;
            Target = target;
            Comment = comment;
        }
    }

    public ModuleRegistry.Info? ModuleInfo;
    public PlanDefinitions.ClassData ClassDef;
    public PlanDefinitions.CooldownTrack TrackDef;
    public ActionID DefaultAction;
    public int Level;

    public ColumnPlannerTrackCooldown(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, string name, ModuleRegistry.Info? moduleInfo, PlanDefinitions.ClassData classDef, PlanDefinitions.CooldownTrack trackDef, ActionID defaultAction, int level)
        : base(timeline, tree, phaseBranches, name)
    {
        ModuleInfo = moduleInfo;
        ClassDef = classDef;
        TrackDef = trackDef;
        DefaultAction = defaultAction;
        Level = level;
    }

    public void AddElement(StateMachineTree.Node attachNode, float delay, float windowLength, ActionID aid, bool lowPriority, PlanTarget.ISelector target, string comment)
    {
        var elem = (ActionElement)AddElement(attachNode, delay, windowLength);
        elem.LowPriority = lowPriority;
        elem.Target = target;
        elem.Comment = comment;
        SetElementAction(elem, aid);
    }

    protected override Element CreateElement(Entry window)
    {
        return SetElementAction(new ActionElement(window, false, new PlanTarget.Self(), ""), DefaultAction);
    }

    protected override List<string> DescribeElement(Element e)
    {
        var cast = (ActionElement)e;
        List<string> res = new();
        res.Add($"Comment: {cast.Comment}");
        res.Add($"Action: {cast.Action}");
        res.Add($"Target: {cast.Target.GetType().Name}: {cast.Target.Describe(ModuleInfo)}");
        res.Add($"Priority: {(cast.LowPriority ? "low" : "high")}");
        return res;
    }

    protected override void EditElement(Element e)
    {
        var cast = (ActionElement)e;
        if (ImGui.BeginCombo("Action", cast.Action.ToString()))
        {
            foreach (var a in TrackDef.Actions.Where(a => a.minLevel <= Level))
            {
                if (ImGui.Selectable(a.aid.ToString(), cast.Action == a.aid))
                {
                    SetElementAction(cast, a.aid);
                    NotifyModified();
                }
            }
            ImGui.EndCombo();
        }
        if (ImGui.InputText("Comment", ref cast.Comment, 256))
        {
            NotifyModified();
        }
        if (ImGui.Checkbox("Low priority", ref cast.LowPriority))
        {
            NotifyModified();
        }
        if (ImGui.BeginCombo("Target", cast.Target.GetType().Name))
        {
            foreach (var t in Utils.GetDerivedTypes<PlanTarget.ISelector>(Assembly.GetExecutingAssembly()))
            {
                if (ImGui.Selectable(t.Name, t == cast.Target.GetType()))
                {
                    cast.Target = (PlanTarget.ISelector)Activator.CreateInstance(t)!;
                    NotifyModified();
                }
            }
            ImGui.EndCombo();
        }
        ImGui.Indent();
        if (cast.Target.Edit(ModuleInfo))
            NotifyModified();
        ImGui.Unindent();
    }

    private ActionElement SetElementAction(ActionElement e, ActionID action)
    {
        e.Action = action;
        var actionDef = ClassDef.Abilities[action];
        e.EffectLength = actionDef.EffectDuration;
        e.CooldownLength = actionDef.Cooldown;
        return e;
    }
}
