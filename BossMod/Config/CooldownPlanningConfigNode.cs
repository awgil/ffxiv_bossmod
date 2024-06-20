//using ImGuiNET;
//using System.Reflection;

//namespace BossMod;

//// base class for encounter configurations that support cooldown planning
//public abstract class CooldownPlanningConfigNode : ConfigNode
//{
//    // per-class list of plans
//    public class PlanList
//    {
//        public List<CooldownPlan> Available = [];
//        public int SelectedIndex = -1;

//        public CooldownPlan? Selected() => SelectedIndex >= 0 && SelectedIndex < Available.Count ? Available[SelectedIndex] : null;
//    }

//    public int SyncLevel { get; private init; }
//    public Dictionary<Class, PlanList> CooldownPlans = [];

//    public CooldownPlan? SelectedPlan(Class c) => CooldownPlans.GetValueOrDefault(c)?.Selected();

//    protected CooldownPlanningConfigNode(int syncLevel)
//    {
//        SyncLevel = syncLevel;
//        foreach (var c in PlanDefinitions.Classes.Keys)
//            CooldownPlans[c] = new();
//    }

//    public void DrawSelectionUI(Class c, StateMachine sm, ModuleRegistry.Info? moduleInfo)
//    {
//        if (!PlanDefinitions.Classes.ContainsKey(c))
//            return; // class is not supported

//        var plans = CooldownPlans.GetOrAdd(c);
//        var newSelected = DrawPlanCombo(plans, plans.SelectedIndex, "Cooldown plan");
//        if (newSelected != plans.SelectedIndex)
//        {
//            plans.SelectedIndex = newSelected;
//            Modified.Fire();
//        }
//        ImGui.SameLine();
//        if (ImGui.Button(plans.SelectedIndex >= 0 ? "Edit plan" : "Create new plan"))
//        {
//        }
//    }

//    public override void DrawCustom(UITree tree, WorldState ws)
//    {
//        foreach (var _ in tree.Node("Cooldown plans"))
//        {
//            foreach (var (c, plans) in CooldownPlans)
//            {
//                for (int i = 0; i < plans.Available.Count; ++i)
//                {
//                    ImGui.PushID($"{c}/{i}");
//                    if (ImGui.Button($"Edit"))
//                    {
//                        StartPlanEditor(plans.Available[i]);
//                    }
//                    ImGui.SameLine();
//                    if (ImGui.Button($"Copy"))
//                    {
//                        var plan = plans.Available[i].Clone();
//                        plan.Name += " Copy";
//                        plans.Available.Add(plan);
//                        Modified.Fire();
//                        StartPlanEditor(plan);
//                    }
//                    ImGui.SameLine();
//                    if (UIMisc.DangerousButton($"Delete"))
//                    {
//                        if (plans.SelectedIndex == i)
//                            plans.SelectedIndex = -1;
//                        else if (plans.SelectedIndex > i)
//                            plans.SelectedIndex--;
//                        plans.Available.RemoveAt(i);
//                        --i;
//                        Modified.Fire();
//                    }
//                    ImGui.SameLine();
//                    bool selected = plans.SelectedIndex == i;
//                    if (ImGui.Checkbox($"{c} '{plans.Available[i].Name}'", ref selected))
//                    {
//                        plans.SelectedIndex = selected ? i : -1;
//                        Modified.Fire();
//                    }
//                    ImGui.PopID();
//                }
//            }
//            ImGui.TextUnformatted("Add new plan:");
//            foreach (var (c, plans) in CooldownPlans)
//            {
//                ImGui.SameLine();
//                if (ImGui.Button(c.ToString()))
//                {
//                    var plan = new CooldownPlan(c, SyncLevel, $"New {plans.Available.Count}");
//                    plans.Available.Add(plan);
//                    Modified.Fire();
//                    StartPlanEditor(plan);
//                }
//            }
//        }
//    }

