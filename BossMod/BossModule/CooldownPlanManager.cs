using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public class CooldownPlanManager : ConfigNode
    {
        public Dictionary<uint, Dictionary<Class, List<CooldownPlan>>> Plans = new(); // [encounter-oid][class]

        public CooldownPlanManager()
        {
            var simple = typeof(SimpleBossModule);
            foreach (var (oid, t) in ModuleRegistry.RegisteredModules.Where(kv => !kv.Value.IsAssignableTo(simple)))
            {
                var p = Plans[oid] = new();
                foreach (var c in CooldownPlan.SupportedClasses.Keys)
                    p[c] = new();
            }

            DisplayName = "Cooldown Plans";
            DisplayOrder = 4;
        }

        public CooldownPlan? DrawSelectionUI(CooldownPlan? current, uint encounterOID, Class curClass, StateMachine.State? initial)
        {
            if (!CooldownPlan.SupportedClasses.ContainsKey(curClass))
                return null; // class is not supported

            if (current != null && current.Class != curClass)
                current = null;

            var plans = Plans.GetOrAdd(encounterOID).GetOrAdd(curClass);
            ImGui.SetNextItemWidth(100);
            if (ImGui.BeginCombo("Cooldown plan", current?.Name ?? "none"))
            {
                if (ImGui.Selectable("none", current == null))
                    current = null;
                foreach (var plan in plans)
                    if (ImGui.Selectable(plan.Name, current == plan))
                        current = plan;
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            if (ImGui.Button(current != null ? "Edit plan" : "Create new plan"))
            {
                if (current == null)
                {
                    current = new(curClass, $"New {plans.Count}");
                    plans.Add(current);
                }
                StartPlanEditor(current, initial);
            }
            return current;
        }

        protected override void DrawContents()
        {
            Tree tree = new();
            foreach (var (e, eEntries) in tree.Nodes(Plans, kv => (ModuleRegistry.TypeForOID(kv.Key)?.Name ?? $"{kv.Key:X}", false)))
            {
                foreach (var (c, cEntries) in tree.Nodes(eEntries, kv => (kv.Key.ToString(), false)))
                {
                    int i = 0;
                    foreach (var plan in cEntries)
                    {
                        if (ImGui.Button($"Edit {plan.Name}##{i++}"))
                        {
                            StartPlanEditor(plan, CreateStateForOID(e));
                        }
                    }
                    if (ImGui.Button($"Add new...##{i++}"))
                    {
                        var plan = new CooldownPlan(c, $"New {cEntries.Count}");
                        cEntries.Add(plan);
                        StartPlanEditor(plan, CreateStateForOID(e));
                    }
                }
            }
        }

        private void StartPlanEditor(CooldownPlan plan, StateMachine.State? initial)
        {
            var editor = new CooldownPlanEditor(plan, initial, NotifyModified);
            var w = WindowManager.CreateWindow($"Cooldown planner", editor.Draw, () => { }, () => true);
            w.SizeHint = new(600, 600);
            w.MinSize = new(100, 100);
        }

        private StateMachine.State? CreateStateForOID(uint oid)
        {
            return ModuleRegistry.CreateModule(oid, new(new(), new()), new(0, oid, "", ActorType.None, Class.None, new(), 0, false, 0))?.InitialState;
        }
    }
}
