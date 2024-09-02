using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Autorotation;

public sealed class UIPlanDatabaseEditor
{
    // utility: draw combo that allows selecting a plan from a list; used in variety of contexts
    public static int DrawPlanCombo(PlanDatabase.PlanList list, int selected, string label)
    {
        ImGui.SetNextItemWidth(150);
        using var combo = ImRaii.Combo(label, selected >= 0 && selected < list.Plans.Count ? list.Plans[selected].Name : "<none>");
        if (combo)
        {
            if (ImGui.Selectable("<none>", selected < 0))
            {
                selected = -1;
            }
            for (int i = 0; i < list.Plans.Count; ++i)
            {
                if (ImGui.Selectable(list.Plans[i].Name, selected == i))
                {
                    selected = i;
                }
            }
        }
        return selected;
    }

    public static void StartPlanEditor(PlanDatabase db, Plan plan, StateMachine sm)
    {
        _ = new UIPlanEditorWindow(db, plan, sm);
    }

    public static void StartPlanEditor(PlanDatabase db, Plan plan)
    {
        var m = ModuleRegistry.CreateModuleForConfigPlanning(plan.Encounter);
        if (m != null)
            StartPlanEditor(db, plan, m.StateMachine);
    }
}
