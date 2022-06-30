using ImGuiNET;
using System;
using System.Collections.Generic;

namespace BossMod
{
    // attribute that associates config type to boss module type
    // if not set, boss module won't support cooldown planning
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CooldownPlanningAttribute : Attribute
    {
        public Type ConfigType { get; private init; }

        public CooldownPlanningAttribute(Type configType)
        {
            ConfigType = configType;
        }
    }

    // base class for encounter configurations that support cooldown planning
    public abstract class CooldownPlanningConfigNode : ConfigNode
    {
        // per-class list of plans
        public class PlanList
        {
            public List<CooldownPlan> Available = new();
            public int SelectedIndex = -1;

            public CooldownPlan? Selected() => SelectedIndex >= 0 ? Available[SelectedIndex] : null;
        }

        public Dictionary<Class, PlanList> CooldownPlans = new();

        public CooldownPlan? SelectedPlan(Class c) => CooldownPlans.GetValueOrDefault(c)?.Selected();

        public CooldownPlanningConfigNode()
        {
            foreach (var c in AbilityDefinitions.Classes.Keys)
                CooldownPlans[c] = new();
        }

        public void DrawSelectionUI(Class c, StateMachine sm)
        {
            if (!AbilityDefinitions.Classes.ContainsKey(c))
                return; // class is not supported

            var plans = CooldownPlans.GetOrAdd(c);
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
                    plans.Available.Add(new(c, $"New {plans.Available.Count + 1}"));
                    plans.SelectedIndex = plans.Available.Count - 1;
                    NotifyModified();
                }
                StartPlanEditor(plans.Available[plans.SelectedIndex], sm);
            }
        }

        public override void DrawCustom(UITree tree, WorldState ws)
        {
            foreach (var _ in tree.Node("Cooldown plans"))
            {
                foreach (var (c, plans) in CooldownPlans)
                {
                    for (int i = 0; i < plans.Available.Count; ++i)
                    {
                        ImGui.PushID($"{c}/{i}");
                        if (ImGui.Button($"Edit"))
                        {
                            StartPlanEditor(plans.Available[i], CreateStateMachine());
                        }
                        ImGui.SameLine();
                        if (ImGui.Button($"Copy"))
                        {
                            var plan = plans.Available[i].Clone();
                            plan.Name += " Copy";
                            plans.Available.Add(plan);
                            NotifyModified();
                            StartPlanEditor(plan, CreateStateMachine());
                        }
                        ImGui.SameLine();
                        if (ImGui.Button($"Delete"))
                        {
                            if (plans.SelectedIndex == i)
                                plans.SelectedIndex = -1;
                            else if (plans.SelectedIndex > i)
                                plans.SelectedIndex--;
                            plans.Available.RemoveAt(i);
                            --i;
                            NotifyModified();
                        }
                        ImGui.SameLine();
                        bool selected = plans.SelectedIndex == i;
                        if (ImGui.Checkbox($"{c} '{plans.Available[i].Name}'", ref selected))
                        {
                            plans.SelectedIndex = selected ? i : -1;
                            NotifyModified();
                        }
                        ImGui.PopID();
                    }
                }
                ImGui.TextUnformatted("Add new plan:");
                foreach (var (c, plans) in CooldownPlans)
                {
                    ImGui.SameLine();
                    if (ImGui.Button(c.ToString()))
                    {
                        var plan = new CooldownPlan(c, $"New {plans.Available.Count}");
                        plans.Available.Add(plan);
                        NotifyModified();
                        StartPlanEditor(plan, CreateStateMachine());
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

        private StateMachine? CreateStateMachine()
        {
            return ModuleRegistry.CreateModuleForConfigPlanning(GetType())?.StateMachine;
        }
    }
}
