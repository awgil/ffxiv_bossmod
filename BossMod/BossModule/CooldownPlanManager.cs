using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    [ConfigDisplay(Name = "Cooldown Plans", Order = 5)]
    public class CooldownPlanManager : ConfigNode
    {
        public class PlanList
        {
            public List<CooldownPlan> Available = new();
            public int SelectedIndex = -1;

            public CooldownPlan? Selected() => SelectedIndex >= 0 ? Available[SelectedIndex] : null;
        }

        public Dictionary<uint, Dictionary<Class, PlanList>> Plans = new(); // [encounter-oid][class]

        public CooldownPlan? SelectedPlan(uint encounterOID, Class curClass) => Plans.GetValueOrDefault(encounterOID)?.GetValueOrDefault(curClass)?.Selected();

        public CooldownPlanManager()
        {
            var simple = typeof(SimpleBossModule);
            foreach (var (oid, t) in ModuleRegistry.RegisteredModules.Where(kv => !kv.Value.IsAssignableTo(simple)))
            {
                var p = Plans[oid] = new();
                foreach (var c in AbilityDefinitions.Classes.Keys)
                    p[c] = new();
            }
        }

        public void DrawSelectionUI(uint encounterOID, Class curClass, StateMachine sm)
        {
            if (!AbilityDefinitions.Classes.ContainsKey(curClass))
                return; // class is not supported

            var plans = Plans.GetOrAdd(encounterOID).GetOrAdd(curClass);
            ImGui.SetNextItemWidth(100);
            if (ImGui.BeginCombo("Cooldown plan", plans.Selected()?.Name ?? "none"))
            {
                if (ImGui.Selectable("none", plans.SelectedIndex < 0))
                {
                    plans.SelectedIndex = -1;
                    NotifyModified();
                }
                for (int i = 0; i < plans.Available.Count; ++i)
                {
                    if (ImGui.Selectable(plans.Available[i].Name, plans.SelectedIndex == i))
                    {
                        plans.SelectedIndex = i;
                        NotifyModified();
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            if (ImGui.Button(plans.SelectedIndex >= 0 ? "Edit plan" : "Create new plan"))
            {
                if (plans.SelectedIndex < 0)
                {
                    plans.Available.Add(new(curClass, $"New {plans.Available.Count + 1}"));
                    plans.SelectedIndex = plans.Available.Count - 1;
                    NotifyModified();
                }
                StartPlanEditor(plans.Available[plans.SelectedIndex], sm);
            }
        }

        public override void DrawCustom(UITree tree, WorldState ws)
        {
            foreach (var (e, eEntries) in tree.Nodes(Plans, kv => new(ModuleRegistry.TypeForOID(kv.Key)?.Name ?? $"{kv.Key:X}")))
            {
                foreach (var (c, plans) in tree.Nodes(eEntries, kv => new(kv.Key.ToString())))
                {
                    for (int i = 0; i < plans.Available.Count; ++i)
                    {
                        if (ImGui.Button($"Edit {plans.Available[i].Name}##{e}/{c}/{i}"))
                        {
                            StartPlanEditor(plans.Available[i], CreateStateMachineForOID(e));
                        }
                        ImGui.SameLine();
                        if (ImGui.Button($"Copy##{e}/{c}/{i}"))
                        {
                            var plan = plans.Available[i].Clone();
                            plan.Name += " Copy";
                            plans.Available.Add(plan);
                            NotifyModified();
                            StartPlanEditor(plan, CreateStateMachineForOID(e));
                        }
                        ImGui.SameLine();
                        if (ImGui.Button($"Delete##{e}/{c}/{i}"))
                        {
                            if (plans.SelectedIndex == i)
                                plans.SelectedIndex = -1;
                            else if (plans.SelectedIndex > i)
                                plans.SelectedIndex--;
                            plans.Available.RemoveAt(i);
                            --i;
                            NotifyModified();
                        }
                    }
                    if (ImGui.Button($"Add new...##{e}/{c}"))
                    {
                        var plan = new CooldownPlan(c, $"New {plans.Available.Count}");
                        plans.Available.Add(plan);
                        NotifyModified();
                        StartPlanEditor(plan, CreateStateMachineForOID(e));
                    }
                }
            }
        }

        private void StartPlanEditor(CooldownPlan plan, StateMachine? sm)
        {
            if (sm == null)
                return;
            var editor = new CooldownPlanEditor(plan, sm, NotifyModified);
            var w = WindowManager.CreateWindow($"Cooldown planner", editor.Draw, () => { }, () => true);
            w.SizeHint = new(600, 600);
            w.MinSize = new(100, 100);
        }

        private static StateMachine? CreateStateMachineForOID(uint oid)
        {
            return ModuleRegistry.CreateModule(oid, new(new()), new(0, oid, "", ActorType.None, Class.None, new()))?.StateMachine;
        }
    }
}
