using ImGuiNET;
using System.Collections.Generic;

namespace BossMod
{
    public class CooldownPlanManager : ConfigNode
    {
        public Dictionary<uint, Dictionary<Class, List<CooldownPlan>>> Plans = new(); // [encounter-oid][class]

        public CooldownPlanManager()
        {
            Visible = false;
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
                var editor = new CooldownPlanEditor(current, initial, NotifyModified);
                var w = WindowManager.CreateWindow($"Cooldown planner", editor.Draw, () => { }, () => true);
                w.SizeHint = new(600, 600);
                w.MinSize = new(100, 100);
            }
            return current;
        }
    }
}
